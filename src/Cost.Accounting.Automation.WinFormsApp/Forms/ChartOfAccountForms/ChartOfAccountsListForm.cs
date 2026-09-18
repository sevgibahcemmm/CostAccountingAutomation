using Cost.Accounting.Automation.Application.Behaviors;
using Cost.Accounting.Automation.Application.ChartOfAccounts;
using Cost.Accounting.Automation.Application.ProductMovements;
using Cost.Accounting.Automation.Application.Products;
using Cost.Accounting.Automation.Domain.Products;
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
        _btnManualAdd.Enabled = false;
        _ = ReloadAsync();
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
        try
        {
            _lblSub.Text = "Yevmiye kayıtları güncelleniyor...";

            using var scope = Program.Services.CreateScope();
            ISender mediator = scope.ServiceProvider.GetRequiredService<ISender>();

            await mediator.Send(new ChartOfAccountLedgerSyncCommand(), CancellationToken.None);

            _lblSub.Text = "Yenileniyor...";

            List<ChartOfAccountDto> items = (await mediator.Send(new ChartOfAccountGetAllQuery(), CancellationToken.None)).ToList();

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

            _totalDebit = items.Where(i => i.ParentId is null).Sum(i => i.DebitAmount);
            _totalCredit = items.Where(i => i.ParentId is null).Sum(i => i.CreditAmount);
            _totalDebitBalance = items.Where(i => i.ParentId is null).Sum(i => i.DebitBalance);
            _totalCreditBalance = items.Where(i => i.ParentId is null).Sum(i => i.CreditBalance);

            _tree.DataSource = null;
            _tree.DataSource = items;
            _tree.ForceInitialize();
            _tree.ExpandAll();

            _tree.Refresh();
            System.Windows.Forms.Application.DoEvents();

            UpdateManualAddButtonState();

            if (_tree.Nodes.Count > 0)
            {
                _tree.MakeNodeVisible(_tree.Nodes[0]);
            }

            _lblSub.Text = $"{items.Count} hesap listeleniyor (kök: {_tree.Nodes.Count}, düğüm: {_tree.AllNodesCount})";

            _lblFooterTotals.Text = string.Empty;
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
                e.Appearance.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
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