using Cost.Accounting.Automation.Application.Behaviors;
using Cost.Accounting.Automation.Application.ChartOfAccounts;
using Cost.Accounting.Automation.Domain.ChartOfAccounts;
using Cost.Accounting.Automation.WinFormsApp.Forms.BaseForm;
using Cost.Accounting.Automation.WinFormsApp.Forms.MainForms;
using Cost.Accounting.Automation.WinFormsApp.Tools;
using Cost.Accounting.Automation.WinFormsApp.Utils;
using DevExpress.XtraTreeList;
using DevExpress.XtraTreeList.Nodes;
using Microsoft.Extensions.DependencyInjection;
using TS.MediatR;

namespace Cost.Accounting.Automation.WinFormsApp.Forms.ChartOfAccountForms;

public sealed partial class ChartOfAccountsListForm : XtraFormMdiBase
{
    private static readonly Font Level0Font = new("Segoe UI", 9F, FontStyle.Bold);

    private bool _cascading;

    private decimal _totalDebit;
    private decimal _totalCredit;
    private decimal _totalDebitBalance;
    private decimal _totalCreditBalance;

    public ChartOfAccountsListForm() : base("Hesap Planı")
    {
        InitializeComponent();
    }

    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);
        _filterMoved = true;
        _tswShowMoved.IsOn = true;
        _btnManualAdd.Enabled = false;
        _ = ReloadAsync();
    }

    protected override void OnShown(EventArgs e)
    {
        base.OnShown(e);
        _tswShowMoved.Left = _pnlToolbar.ClientSize.Width - _tswShowMoved.Width - 14;
        _tswShowMoved.Top = Math.Max(10, (_pnlToolbar.ClientSize.Height - _tswShowMoved.Height) / 2);
        _tswShowMoved.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        _tswShowMoved.BringToFront();
    }

    private void Tree_AfterCheckNode(object? sender, NodeEventArgs e)
    {
        if (_cascading || e.Node is null)
        {
            return;
        }

        _cascading = true;
        try
        {
            _tree.BeginUpdate();
            SetDescendantsChecked(e.Node, e.Node.Checked);
            _tree.EndUpdate();
        }
        finally
        {
            _cascading = false;
        }

        UpdateManualAddButtonState();
    }

    private void UpdateManualAddButtonState()
    {
        _btnManualAdd.Enabled = _tree.GetAllCheckedNodes().Any();
    }

    private static void SetDescendantsChecked(TreeListNode node, bool check)
    {
        foreach (TreeListNode child in node.Nodes)
        {
            child.Checked = check;
            SetDescendantsChecked(child, check);
        }
    }

    private void SetAllChecked(bool check)
    {
        _tree.BeginUpdate();
        _cascading = true;
        try
        {
            foreach (TreeListNode root in _tree.Nodes)
            {
                root.Checked = check;
                SetDescendantsChecked(root, check);
            }
        }
        finally
        {
            _cascading = false;
            _tree.EndUpdate();
        }
    }

    private async Task ReloadAsync()
    {
        await LoadingHelper.RunAsync(
            ReloadCoreAsync,
            caption: "Hesap planı yükleniyor...",
            description: "Lütfen bekleyin...");
    }

    private async Task ReloadCoreAsync()
    {
        int version = ++_reloadVersion;
        try
        {
            _lblSub.Text = "Yevmiye kayıtları güncelleniyor...";

            List<ChartOfAccountDto> items = await Task.Run(async () =>
            {
                using var scope = Program.Services.CreateScope();
                ISender mediator = scope.ServiceProvider.GetRequiredService<ISender>();
                await mediator.Send(new ChartOfAccountLedgerSyncCommand(), CancellationToken.None);
                return (await mediator.Send(new ChartOfAccountGetAllQuery(), CancellationToken.None)).ToList();
            });

            if (version != _reloadVersion)
            {
                return;
            }

            _lblSub.Text = "Yenileniyor...";

            Dictionary<Guid, ChartOfAccountDto> byId = items.ToDictionary(x => x.Id, x => x);

            foreach (ChartOfAccountDto item in items)
            {
                if (item.SemiFinishedAccountId is Guid semiId && byId.TryGetValue(semiId, out ChartOfAccountDto? semi))
                {
                    item.SemiFinishedCode = semi.Code;
                }

                if (item.FinishedAccountId is Guid finishedId && byId.TryGetValue(finishedId, out ChartOfAccountDto? finished))
                {
                    item.FinishedCode = finished.Code;
                }
            }

            _items = items;
            ApplyTreeFilter();
        }
        catch (AuthorizationException ex)
        {
            ToastHelper.Show(ex.Message, ToastType.Warning, 4000);
            _lblSub.Text = "Yetkiniz yok";
        }
        catch (Exception ex)
        {
            ToastHelper.Show("Hesap planı yüklenemedi: " + ex.Message, ToastType.Error, 4000);
            _lblSub.Text = "Yükleme hatası";
        }
    }

    private List<ChartOfAccountDto> _items = [];

    private int _reloadVersion;

    private bool _filterMoved;

    private void TswShowMoved_IsOnChanged(object? sender, EventArgs e)
    {
        _filterMoved = _tswShowMoved.IsOn;
        ApplyTreeFilter();
    }

    private static bool HasMovement(ChartOfAccountDto item)
        => item.DebitAmount != 0 || item.CreditAmount != 0 || item.DebitBalance != 0 || item.CreditBalance != 0;

    private void ApplyTreeFilter()
    {
        List<ChartOfAccountDto> display = _filterMoved
            ? _items.Where(HasMovement).ToList()
            : _items;

        _totalDebit = display.Where(i => i.ParentId is null).Sum(i => i.DebitAmount);
        _totalCredit = display.Where(i => i.ParentId is null).Sum(i => i.CreditAmount);
        _totalDebitBalance = display.Where(i => i.ParentId is null).Sum(i => i.DebitBalance);
        _totalCreditBalance = display.Where(i => i.ParentId is null).Sum(i => i.CreditBalance);

        _tree.BeginUpdate();
        try
        {
            _tree.DataSource = null;
            _tree.DataSource = display;
            _tree.ForceInitialize();
            _tree.ExpandAll();
            _tree.Refresh();
        }
        finally
        {
            _tree.EndUpdate();
        }

        UpdateManualAddButtonState();

        if (_tree.Nodes.Count > 0)
        {
            _tree.MakeNodeVisible(_tree.Nodes[0]);
        }

        _lblSub.Text = _filterMoved
            ? $"{display.Count} hesap hareket görüyor (kök: {_tree.Nodes.Count}, düğüm: {_tree.AllNodesCount})"
            : $"{display.Count} hesap listeleniyor (kök: {_tree.Nodes.Count}, düğüm: {_tree.AllNodesCount})";

        _lblFooterTotals.Text = string.Empty;
    }

    private void Tree_CustomDrawNodeCell(object? sender, DevExpress.XtraTreeList.CustomDrawNodeCellEventArgs e)
    {
        if (e.Node is null)
        {
            return;
        }

        if (e.Node.Level == 0)
        {
            e.Appearance.Font = Level0Font;
        }
    }

    private void Tree_GetCustomSummaryValue(object? sender, DevExpress.XtraTreeList.GetCustomSummaryValueEventArgs e)
    {
        if (!e.IsSummaryFooter)
        {
            return;
        }

        switch (e.Column.FieldName)
        {
            case nameof(ChartOfAccountDto.Name):
                e.CustomValue = "Genel toplam";
                break;
            case nameof(ChartOfAccountDto.DebitAmount):
                e.CustomValue = _totalDebit;
                break;
            case nameof(ChartOfAccountDto.CreditAmount):
                e.CustomValue = _totalCredit;
                break;
            case nameof(ChartOfAccountDto.DebitBalance):
                e.CustomValue = _totalDebitBalance;
                break;
            case nameof(ChartOfAccountDto.CreditBalance):
                e.CustomValue = _totalCreditBalance;
                break;
        }
    }

    private void Tree_CustomDrawFooterCell(object? sender, DevExpress.XtraTreeList.CustomDrawFooterCellEventArgs e)
    {
        switch (e.Column.FieldName)
        {
            case nameof(ChartOfAccountDto.Name):
                e.Info.DisplayText = "Genel toplam";
                e.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Far;
                e.Appearance.Font = Level0Font;
                break;
            case nameof(ChartOfAccountDto.DebitAmount):
                e.Info.DisplayText = _totalDebit.ToString("N2");
                e.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Far;
                break;
            case nameof(ChartOfAccountDto.CreditAmount):
                e.Info.DisplayText = _totalCredit.ToString("N2");
                e.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Far;
                break;
            case nameof(ChartOfAccountDto.DebitBalance):
                e.Info.DisplayText = _totalDebitBalance.ToString("N2");
                e.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Far;
                break;
            case nameof(ChartOfAccountDto.CreditBalance):
                e.Info.DisplayText = _totalCreditBalance.ToString("N2");
                e.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Far;
                break;
        }
    }

    private void BtnClosePage_Click(object? sender, EventArgs e)
    {
        Close();
    }

    private async void BtnRefresh_Click(object? sender, EventArgs e)
    {
        await ReloadAsync();
    }

    private void BtnExpandAll_Click(object? sender, EventArgs e)
    {
        _tree.ExpandAll();
    }

    private void BtnCollapseAll_Click(object? sender, EventArgs e)
    {
        _tree.CollapseAll();
    }

    private void BtnSelectAll_Click(object? sender, EventArgs e)
    {
        SetAllChecked(true);
        UpdateManualAddButtonState();
    }

    private void BtnClearSelection_Click(object? sender, EventArgs e)
    {
        SetAllChecked(false);
        UpdateManualAddButtonState();
    }

    private async void BtnDelete_Click(object? sender, EventArgs e)
    {
        List<Guid> ids = _tree.GetAllCheckedNodes()
            .Select(n => n.GetValue(nameof(ChartOfAccountDto.Id)))
            .OfType<Guid>()
            .Distinct()
            .ToList();

        if (ids.Count == 0)
        {
            ToastHelper.Show("Silmek için önce satırları işaretleyin", ToastType.Warning, 3200);
            return;
        }

        AccountDeletionCheck check;
        try
        {
            using var scope = Program.Services.CreateScope();
            IChartOfAccountRepository accounts = scope.ServiceProvider.GetRequiredService<IChartOfAccountRepository>();
            check = await accounts.GetDeletionCheckAsync(ids, CancellationToken.None);
        }
        catch (Exception ex)
        {
            CrashLog.WriteException("ChartAccount.Delete.PreCheck", ex);
            ToastHelper.Show("Silme kontrolü yapılamadı: " + ex.Message, ToastType.Error, 6000);
            return;
        }

        if (check.MovementAccountIds.Count > 0)
        {
            string codes = string.Join(", ", check.MovementAccountIds.Take(5));
            string suffix = check.MovementAccountIds.Count > 5 ? $" ve {check.MovementAccountIds.Count - 5} hesap daha" : "";
            ToastHelper.Show($"{check.MovementAccountIds.Count} hesap hareket gördüğü için silinemez: {codes}{suffix}",
                ToastType.Warning, 6000);
            return;
        }

        if (MsgBox.Confirm(
            $"{ids.Count} hesap silinecek. Alt hesapları da silinir.\n\nSilinenler 'Hesap Planı İçe Aktar' ile yeniden yüklenebilir.\nEmin misiniz?",
            "Hesap Sil") != DialogResult.Yes)
        {
            return;
        }

        bool ok = await CrudExecutor.ExecuteAsync(new ChartOfAccountDeleteCommand(ids));
        if (ok)
        {
            await ReloadAsync();
        }
    }

    private void BtnManualAdd_Click(object? sender, EventArgs e)
    {
        Guid? parentId = SelectCheckedParent();

        using ManualAccountEditForm form = new(parentId);
        if (form.ShowDialog(this) == DialogResult.OK)
        {
            _ = ReloadAsync();
        }
    }

    private Guid? SelectCheckedParent()
    {
        List<TreeListNode> checkedNodes = _tree.GetAllCheckedNodes().ToList();

        if (checkedNodes.Count == 0)
        {
            ToastHelper.Show("Alt hesap eklemek için önce bir hesabı işaretleyin.", ToastType.Warning, 3200);
            return null;
        }

        if (_tree.FocusedNode is { } focused
            && focused.Checked
            && focused.GetValue(nameof(ChartOfAccountDto.Id)) is Guid focusedId)
        {
            return focusedId;
        }

        return checkedNodes[0].GetValue(nameof(ChartOfAccountDto.Id)) is Guid firstId ? firstId : null;
    }

    private async void BtnImport_Click(object? sender, EventArgs e)
    {
        using OpenFileDialog dialog = new()
        {
            Title = "Hesap Planı Dosyası Seçin",
            Filter = "Excel / CSV dosyaları (*.xlsx;*.csv)|*.xlsx;*.csv"
        };

        if (dialog.ShowDialog(this) != DialogResult.OK)
        {
            return;
        }

        try
        {
            List<ChartOfAccountImportRow> rows = ChartOfAccountFileReader.Read(dialog.FileName);

            if (rows.Count == 0)
            {
                ToastHelper.Show("Dosyada geçerli hesap planı satırı bulunamadı", ToastType.Warning, 4000);
                return;
            }

            if (MsgBox.Confirm(
                $"{rows.Count} hesap satırı bulundu.\n\nMevcut hesap planı bu dosyayla değiştirilecek.\nEmin misiniz?",
                "Hesap Planı İçe Aktar") != DialogResult.Yes)
            {
                return;
            }

            _btnImport.Enabled = false;

            bool ok = await CrudExecutor.ExecuteAsync(new ChartOfAccountImportCommand(rows));
            if (ok)
            {
                await ReloadAsync();
            }
        }
        catch (Exception ex)
        {
            ToastHelper.Show("Dosya okunamadı: " + ex.Message, ToastType.Error, 8000);
        }
        finally
        {
            _btnImport.Enabled = true;
        }
    }
}