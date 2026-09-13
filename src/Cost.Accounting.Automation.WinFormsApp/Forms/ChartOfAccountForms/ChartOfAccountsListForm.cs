using Cost.Accounting.Automation.Application.Behaviors;
using Cost.Accounting.Automation.Application.ChartOfAccounts;
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

    public ChartOfAccountsListForm() : base("Hesap Planı")
    {
        InitializeComponent();
    }

    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);
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
            _lblSub.Text = "Yenileniyor...";

            using var scope = Program.Services.CreateScope();
            ISender mediator = scope.ServiceProvider.GetRequiredService<ISender>();

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

            _tree.DataSource = null;
            _tree.DataSource = items;
            _tree.ForceInitialize();
            _tree.ExpandAll();

            _tree.Refresh();
            System.Windows.Forms.Application.DoEvents();

            if (_tree.Nodes.Count > 0)
            {
                _tree.MakeNodeVisible(_tree.Nodes[0]);
            }

            _lblSub.Text = $"{items.Count} hesap listeleniyor (kök: {_tree.Nodes.Count}, düğüm: {_tree.AllNodesCount})";
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
        if (e.Node is null || e.Node.Selected)
        {
            return;
        }

        // Ağaç satır renkleri sabit RGB yerine aktif DevExpress skin'inin
        // birincil rengi üzerinden türetiliyor: skin değişse bile uyumlu kalır.
        Color primary = DevExpress.LookAndFeel.DXSkinColors.FillColors.Primary;
        int level = e.Node.Level;
        e.Appearance.BackColor = level switch
        {
            0 => Tint(primary, 0.78f),
            1 => Tint(primary, 0.87f),
            2 => Tint(primary, 0.94f),
            _ => Color.White
        };

        if (level == 0)
        {
            e.Appearance.Font = Level0Font;
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
    }

    private void BtnClearSelection_Click(object? sender, EventArgs e)
    {
        SetAllChecked(false);
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

    /// <summary>
    /// Bir rengi verilen oranda beyaza (amount > 0) ya da siyaha (amount &lt; 0) yaklaştırır.
    /// Skin'in birincil renginden açık/koyu tonlar türetmek için kullanılır.
    /// </summary>
    private static Color Tint(Color color, float amount)
    {
        if (amount >= 0)
        {
            int r = color.R + (int)((255 - color.R) * amount);
            int g = color.G + (int)((255 - color.G) * amount);
            int b = color.B + (int)((255 - color.B) * amount);
            return Color.FromArgb(Clamp(r), Clamp(g), Clamp(b));
        }

        float factor = 1 + amount;
        return Color.FromArgb(
            Clamp((int)(color.R * factor)),
            Clamp((int)(color.G * factor)),
            Clamp((int)(color.B * factor)));
    }

    private static int Clamp(int value) => Math.Max(0, Math.Min(255, value));
}