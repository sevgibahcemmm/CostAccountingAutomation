using System.Drawing;
using Cost.Accounting.Automation.Application.Messages;
using Cost.Accounting.Automation.WinFormsApp.Forms.BaseForm;
using Cost.Accounting.Automation.WinFormsApp.Forms.MainForms;
using Cost.Accounting.Automation.WinFormsApp.Tools;
using Cost.Accounting.Automation.WinFormsApp.Utils;
using DevExpress.Utils;
using DevExpress.XtraEditors;
using DevExpress.XtraGrid;
using DevExpress.XtraGrid.Columns;
using DevExpress.XtraGrid.Views.Base;
using DevExpress.XtraGrid.Views.Grid;
using DevExpress.XtraTab;
using Microsoft.Extensions.DependencyInjection;
using TS.MediatR;
using TS.Result;

namespace Cost.Accounting.Automation.WinFormsApp.Forms.MessageForms;

internal sealed class AnnouncementComposePage : IDisposable
{
    /// <summary>Arama kutusu boşaltıldıktan sonra bekleme süresi.</summary>
    private const int SearchDebounceMilliseconds = 300;

    private List<MessageRecipientDto> _recipients = [];

    private readonly System.Windows.Forms.Timer _searchTimer;

    private bool _disposed;

    /// <summary>Alıcı listesi en az bir kez yüklenmeye çalışıldı mı (arama hariç).</summary>
    private bool _recipientsLoaded;

    public AnnouncementComposePage()
    {
        Page = new XtraTabPage { Text = "Duyuru Gönder" };
        _searchTimer = new System.Windows.Forms.Timer { Interval = SearchDebounceMilliseconds };

        BuildLayout();
        WireEvents();
    }

    public XtraTabPage Page { get; }

    public PanelControl pnlHeader { get; private set; } = null!;

    public Panel pnlText { get; private set; } = null!;

    public LabelControl lblTitle { get; private set; } = null!;

    public LabelControl lblDetail { get; private set; } = null!;

    public SimpleButton btnClose { get; private set; } = null!;

    public PanelControl pnlRecipients { get; private set; } = null!;

    public PanelControl pnlMode { get; private set; } = null!;

    public RadioGroup grpMode { get; private set; } = null!;

    public PanelControl pnlSearch { get; private set; } = null!;

    public TextEdit txtSearch { get; private set; } = null!;

    public GridControl gridRecipients { get; private set; } = null!;

    public GridView viewRecipients { get; private set; } = null!;

    /// <summary>Liste durumu (yükleniyor / boş / hata) etiketi.</summary>
    public LabelControl lblRecipientsState { get; private set; } = null!;

    public Panel pnlSelectionBar { get; private set; } = null!;

    public LabelControl lblSelection { get; private set; } = null!;

    public SimpleButton btnSelectAll { get; private set; } = null!;

    public SimpleButton btnClearSelection { get; private set; } = null!;

    public PanelControl pnlSubject { get; private set; } = null!;

    public TextEdit txtSubject { get; private set; } = null!;

    public PanelControl pnlBody { get; private set; } = null!;

    public MemoEdit txtBody { get; private set; } = null!;

    public PanelControl pnlFooter { get; private set; } = null!;

    public LabelControl lblHint { get; private set; } = null!;

    public SimpleButton btnSend { get; private set; } = null!;

    /// <summary>Sayfa kapatılmak istendiğinde (X düğmesi) sahibi form dinler.</summary>
    public event EventHandler? CloseRequested;

    /// <summary>Gönder düğmesine basıldığında sahibi formun gönderim akışı çalışır.</summary>
    public Func<AnnouncementComposePage, Task>? SendRequested { get; set; }

    /// <summary>Arama kutusundaki term ile alıcı listesini süzer.</summary>
    public string SearchTerm => EditText(txtSearch);

    /// <summary>Duyuru konusu; boşken <see cref="string.Empty"/>.</summary>
    public string SubjectText => EditText(txtSubject);

    /// <summary>Duyuru metni; boşken <see cref="string.Empty"/>.</summary>
    public string BodyText => EditText(txtBody);

    /// <summary>
    /// DevExpress editörlerinde değer nullken <c>Text</c>, ipucu yazısını
    /// (Properties.NullText) döndürür; bu durum boş kabul edilir. Yoksa
    /// arama/konu/metin alanları hiç dokunulmadan hint metniyle dolu görünür.
    /// </summary>
    private static string EditText(BaseEdit edit)
    {
        string text = edit.Text ?? string.Empty;

        return string.IsNullOrWhiteSpace(text) || text == edit.Properties.NullText
            ? string.Empty
            : text.Trim();
    }

    /// <summary>
    /// <see langword="true"/> ise duyuru alıcı listesiyle sınırlı değildir:
    /// pasif olmayan (çevrimdışı olanlar dâhil) tüm kullanıcılara gönderilir.
    /// </summary>
    public bool SendToAll => Equals(grpMode.EditValue, true);

    /// <summary>Şu an seçili olan alıcılar.</summary>
    public List<MessageRecipientDto> GetSelectedRecipients()
    {
        List<MessageRecipientDto> selected = [];

        foreach (int row in viewRecipients.GetSelectedRows())
        {
            if (viewRecipients.GetRow(row) is MessageRecipientDto recipient)
            {
                selected.Add(recipient);
            }
        }

        return selected;
    }

    /// <summary>Gönderilecek duyurunun alanlarını doğrular; sorun varsa metni döndürür.</summary>
    public string? Validate()
    {
        if (!SendToAll && GetSelectedRecipients().Count == 0)
        {
            return "En az bir alıcı seçin veya \"Tüm Kullanıcılara\" seçeneğini işaretleyin.";
        }

        if (SubjectText.Length == 0)
        {
            return "Duyuru konusu zorunludur.";
        }

        if (BodyText.Length == 0)
        {
            return "Duyuru metni boş olamaz.";
        }

        return null;
    }

    /// <summary>Alıcı listesini (boş arama = tüm kullanıcılar) tazeler.</summary>
    /// <remarks>
    /// Boş liste şikâyetlerini izlemek için sorgu başlangıcı, sonucu ve
    /// kayıt sayısı geçici olarak crash.log'a yazılır (bkz. CrashLog).
    /// </remarks>
    public async Task LoadRecipientsAsync()
    {
        if (_disposed || Page.IsDisposed)
        {
            return;
        }

        string term = SearchTerm;

        CrashLog.Write("Message.Announce", $"Alıcı sorgusu başlıyor (term='{term}').");
        ShowRecipientsState("Alıcılar yükleniyor...");

        try
        {
            using var scope = Program.Services.CreateScope();
            ISender mediator = scope.ServiceProvider.GetRequiredService<ISender>();

            // Duyuru "şirket geneline" yapılır; arama boşken listenin mümkün
            // olduğunca geniş olması gerekir, bu yüzden sorgunun üst sınırı
            // (200) kullanılır.
            Result<List<MessageRecipientDto>> result = await mediator.Send(
                new MessageRecipientSearchQuery(
                    term,
                    ExcludeSelf: true,
                    MaxResults: string.IsNullOrEmpty(term) ? 200 : 50),
                CancellationToken.None);

            if (result.IsSuccessful)
            {
                _recipients = result.Data ?? [];
                _recipientsLoaded = true;
                CrashLog.Write(
                    "Message.Announce",
                    $"Alıcı sorgusu tamamlandı: {_recipients.Count} kayıt (term='{term}').");
            }
            else
            {
                // Başarısız sonuç sessizce boş listeye çevrilmez: hem log'a
                // yazılır hem uyarı gösterilir. _recipientsLoaded false
                // kaldığı için kip değiştirilince sorgu tekrar denenir.
                string error = AuthFormStyles.GetErrorText(result.ErrorMessages);
                CrashLog.Write("Message.Announce", $"Alıcı sorgusu başarısız: {error}");
                _recipients = [];
                ToastHelper.Show("Alıcılar yüklenemedi: " + error, ToastType.Error, 4000);
            }
        }
        catch (Exception ex)
        {
            CrashLog.WriteException("Message.AnnouncementRecipients", ex);
            _recipients = [];
            ToastHelper.Show("Alıcılar yüklenemedi: " + ex.Message, ToastType.Error, 4000);
        }

        if (_disposed || Page.IsDisposed)
        {
            return;
        }

        // Arama değişirken seçili satırlar korunur: alıcı seçtikten sonra
        // listeyi yenilemek seçimi silmemelidir.
        HashSet<Guid> previouslySelected = GetSelectedRecipients()
            .Select(r => r.Id)
            .ToHashSet();

        gridRecipients.DataSource = _recipients;

        foreach (MessageRecipientDto recipient in _recipients.Where(r => previouslySelected.Contains(r.Id)))
        {
            int index = _recipients.IndexOf(recipient);

            if (index >= 0)
            {
                viewRecipients.SelectRow(viewRecipients.GetRowHandle(index));
            }
        }

        viewRecipients.RefreshData();

        ShowRecipientsState(_recipients.Count switch
        {
            0 when term.Length > 0 => "Arama ile eşleşen aktif kullanıcı yok.",
            0 => "Gösterilecek aktif kullanıcı bulunamadı.",
            _ => null
        });

        UpdateSendState();
    }

    /// <summary>Alıcı listesi altındaki durum etiketini ayarlar; metin yoksa gizler.</summary>
    private void ShowRecipientsState(string? text)
    {
        if (_disposed || Page.IsDisposed)
        {
            return;
        }

        lblRecipientsState.Visible = text is not null;

        if (text is not null)
        {
            lblRecipientsState.Text = text;
        }
    }

    private void BuildLayout()
    {
        //
        // pnlHeader
        //
        pnlHeader = new PanelControl
        {
            Dock = DockStyle.Top,
            Height = 64,
            Padding = new Padding(14, 8, 14, 8)
        };

        pnlText = new Panel { Dock = DockStyle.Fill };

        lblTitle = new LabelControl
        {
            AutoSizeMode = LabelAutoSizeMode.None,
            Dock = DockStyle.Top,
            Height = 24,
            Text = "Duyuru Gönder"
        };

        lblDetail = new LabelControl
        {
            AutoSizeMode = LabelAutoSizeMode.None,
            Dock = DockStyle.Fill,
            Text = "Seçilen her alıcının kutusuna ayrı satır düşer."
        };

        btnClose = new SimpleButton
        {
            Dock = DockStyle.Right,
            Width = 34,
            Text = "✕",
            ToolTip = "Sekmeyi kapat"
        };

        //
        // pnlRecipients: üstte arama, altta seçim çubuğu, dolguda liste.
        //
        pnlRecipients = new PanelControl
        {
            Dock = DockStyle.Top,
            Height = 214,
            Padding = new Padding(14, 8, 14, 8)
        };

        gridRecipients = new GridControl { Dock = DockStyle.Fill };

        viewRecipients = new GridView
        {
            GridControl = gridRecipients,
            OptionsBehavior = { Editable = false },
            OptionsSelection =
            {
                MultiSelect = true,
                MultiSelectMode = DevExpress.XtraGrid.Views.Grid.GridMultiSelectMode.CheckBoxRowSelect
            },
            OptionsView =
            {
                ShowIndicator = false,
                ShowGroupPanel = false,
                ColumnAutoWidth = true,
                ShowVerticalLines = DefaultBoolean.False
            }
        };

        GridColumn fullName = viewRecipients.Columns.AddField(nameof(MessageRecipientDto.FullName));
        fullName.Caption = "Ad Soyad";
        fullName.Width = 180;
        fullName.VisibleIndex = 0;

        GridColumn registry = viewRecipients.Columns.AddField(nameof(MessageRecipientDto.RegistryNumber));
        registry.Caption = "Sicil No";
        registry.Width = 80;
        registry.VisibleIndex = 1;

        GridColumn userName = viewRecipients.Columns.AddField(nameof(MessageRecipientDto.UserName));
        userName.Caption = "Kullanıcı Adı";
        userName.Width = 110;
        userName.VisibleIndex = 2;

        GridColumn company = viewRecipients.Columns.AddField(nameof(MessageRecipientDto.CompanyName));
        company.Caption = "Kurum";
        company.Width = 150;
        company.VisibleIndex = 3;

        gridRecipients.MainView = viewRecipients;
        gridRecipients.ViewCollection.AddRange(new BaseView[] { viewRecipients });

        lblRecipientsState = new LabelControl
        {
            AutoSizeMode = LabelAutoSizeMode.None,
            Dock = DockStyle.Top,
            Height = 22,
            Visible = false,
            Text = string.Empty
        };

        pnlSelectionBar = new Panel { Dock = DockStyle.Bottom, Height = 28 };

        lblSelection = new LabelControl
        {
            AutoSizeMode = LabelAutoSizeMode.None,
            Dock = DockStyle.Fill,
            Text = "Kimse seçilmedi"
        };

        btnSelectAll = new SimpleButton
        {
            Dock = DockStyle.Right,
            Width = 92,
            Text = "Hepsini Seç",
            ToolTip = "Listede görünen tüm kullanıcıları seçer."
        };

        btnClearSelection = new SimpleButton
        {
            Dock = DockStyle.Right,
            Width = 92,
            Text = "Seçimi Temizle"
        };

        pnlSearch = new PanelControl
        {
            Dock = DockStyle.Top,
            Height = 34,
            Padding = new Padding(0, 2, 0, 2)
        };

        txtSearch = new TextEdit { Dock = DockStyle.Fill };
        txtSearch.Properties.NullText = "Ad, kullanıcı adı, sicil no ile ara...";

        //
        // pnlMode: kip seçimi — varsayılan "Tüm Kullanıcılara".
        //
        pnlMode = new PanelControl
        {
            Dock = DockStyle.Top,
            Height = 38,
            Padding = new Padding(14, 5, 14, 5)
        };

        grpMode = new RadioGroup { Dock = DockStyle.Fill };
        grpMode.Properties.Items.AddRange(new[]
        {
            new DevExpress.XtraEditors.Controls.RadioGroupItem
            {
                Value = true,
                Description = "Tüm Kullanıcılara"
            },
            new DevExpress.XtraEditors.Controls.RadioGroupItem
            {
                Value = false,
                Description = "Seçili Kişilere"
            }
        });
        grpMode.Properties.Columns = 2;
        grpMode.EditValue = true;

        //
        // pnlSubject
        //
        pnlSubject = new PanelControl
        {
            Dock = DockStyle.Top,
            Height = 42,
            Padding = new Padding(14, 6, 14, 6)
        };

        txtSubject = new TextEdit { Dock = DockStyle.Fill };
        txtSubject.Properties.NullText = "Konu (zorunlu)";

        //
        // pnlBody
        //
        pnlBody = new PanelControl
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(14, 8, 14, 8)
        };

        txtBody = new MemoEdit { Dock = DockStyle.Fill };
        txtBody.Properties.NullText = "Duyuru metni...";

        //
        // pnlFooter
        //
        pnlFooter = new PanelControl
        {
            Dock = DockStyle.Bottom,
            Height = 56,
            Padding = new Padding(14, 10, 14, 10)
        };

        lblHint = new LabelControl
        {
            AutoSizeMode = LabelAutoSizeMode.None,
            Dock = DockStyle.Fill,
            Text = "Konu ve metin zorunludur; gönderim sunucu tarafında ayrıca doğrulanır."
        };

        btnSend = new SimpleButton
        {
            Dock = DockStyle.Right,
            Width = 150,
            Text = "Duyuruyu Gönder"
        };

        // Ekleme sırası önemlidir: dolgu (Fill) önce, kenara yaslananlar sonra
        // eklenir; ters sırada yaslanan panel Fill'in altında kalır. Üstteki
        // yaslananlar da ters sırada yerleştirilir: son eklenen en üstte kalır
        // (başlık → kip seçimi → alıcı listesi → konu).
        Page.Controls.Add(pnlBody);
        Page.Controls.Add(pnlFooter);
        Page.Controls.Add(pnlSubject);
        Page.Controls.Add(pnlRecipients);
        Page.Controls.Add(pnlMode);
        Page.Controls.Add(pnlHeader);

        pnlHeader.Controls.Add(pnlText);
        pnlText.Controls.Add(lblDetail);
        pnlText.Controls.Add(lblTitle);
        pnlHeader.Controls.Add(btnClose);

        pnlRecipients.Controls.Add(gridRecipients);
        pnlRecipients.Controls.Add(lblRecipientsState);
        pnlRecipients.Controls.Add(pnlSelectionBar);
        pnlRecipients.Controls.Add(pnlSearch);

        pnlMode.Controls.Add(grpMode);

        pnlSelectionBar.Controls.Add(lblSelection);
        pnlSelectionBar.Controls.Add(btnSelectAll);
        pnlSelectionBar.Controls.Add(btnClearSelection);

        pnlSearch.Controls.Add(txtSearch);
        pnlSubject.Controls.Add(txtSubject);
        pnlBody.Controls.Add(txtBody);
        pnlFooter.Controls.Add(lblHint);
        pnlFooter.Controls.Add(btnSend);

        lblTitle.Appearance.Font = new Font("Segoe UI", 11.5F, FontStyle.Bold);
        lblTitle.Appearance.Options.UseFont = true;
        lblDetail.Appearance.Font = new Font("Segoe UI", 8.5F);
        lblDetail.Appearance.Options.UseFont = true;
        btnSend.Appearance.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        btnSend.Appearance.Options.UseFont = true;
        txtBody.Properties.Appearance.Font = new Font("Segoe UI", 10F);
        txtBody.Properties.Appearance.Options.UseFont = true;
        txtSubject.Properties.Appearance.Font = new Font("Segoe UI", 10F);
        txtSubject.Properties.Appearance.Options.UseFont = true;
        grpMode.Properties.Appearance.Font = new Font("Segoe UI", 9.5F);
        grpMode.Properties.Appearance.Options.UseFont = true;
        lblRecipientsState.Appearance.Font = new Font("Segoe UI", 9F);
        lblRecipientsState.Appearance.Options.UseFont = true;

        ApplySkin();

        // Düğmeler ilk boyamada hazır görünsün: liste boşken gönder kapalı,
        // "Hepsini Seç" açık olur. Kip UI'ı da ilk kez burada uygulanır
        // (varsayılan "Tüm Kullanıcılara": liste gizli, açıklama metni hazır).
        ApplyModeUi();
    }

    private void WireEvents()
    {
        _searchTimer.Tick += async (_, _) =>
        {
            _searchTimer.Stop();
            await LoadRecipientsAsync();
        };

        txtSearch.TextChanged += (_, _) =>
        {
            _searchTimer.Stop();
            _searchTimer.Start();
        };

        viewRecipients.SelectionChanged += (_, _) => UpdateSendState();
        txtSubject.TextChanged += (_, _) => UpdateSendState();
        txtBody.TextChanged += (_, _) => UpdateSendState();

        grpMode.EditValueChanged += (_, _) => ApplyModeUi();

        btnSelectAll.Click += (_, _) => SelectAll();
        btnClearSelection.Click += (_, _) => ClearSelection();
        btnClose.Click += (_, _) => CloseRequested?.Invoke(this, EventArgs.Empty);
        btnSend.Click += async (_, _) => await RaiseSendAsync();
    }

    /// <summary>
    /// Gönderim sahibi formda yapılır; burada yalnızca düğme, hata durumunda
    /// yeniden açılır ve formun işi bitince sayfa kapanabilir.
    /// </summary>
    private async Task RaiseSendAsync()
    {
        if (SendRequested is null)
        {
            return;
        }

        btnSend.Enabled = false;

        try
        {
            await SendRequested(this);
        }
        finally
        {
            if (!_disposed && !Page.IsDisposed)
            {
                btnSend.Enabled = true;
            }
        }
    }

    private void SelectAll()
    {
        viewRecipients.ClearSelection();

        for (int handle = 0; handle < viewRecipients.RowCount; handle++)
        {
            viewRecipients.SelectRow(handle);
        }

        UpdateSendState();
    }

    private void ClearSelection()
    {
        viewRecipients.ClearSelection();
        UpdateSendState();
    }

    /// <summary>
    /// Seçili kipe göre alıcı listesinin görünürlüğünü, başlık açıklamasını
    /// ve gönder düğmesini tazeler.
    /// </summary>
    /// <remarks>
    /// Varsayılan kip "Tüm Kullanıcılara" olduğu için liste açılışta arka
    /// planda hazırlanır ve yalnızca kullanıcı "Seçili Kişilere" kipine
    /// geçtiğinde görünür olur. Böylece duyuru göndermek liste sorunlarına
    /// (boş sonuç, yükleme hatası) bağlı kalmaz.
    /// </remarks>
    private void ApplyModeUi()
    {
        if (_disposed || Page.IsDisposed)
        {
            return;
        }

        bool sendToAll = SendToAll;

        lblDetail.Text = sendToAll
            ? "Duyuru, pasif olmayan tüm kullanıcılara iletilir; çevrimdışı olmaları engel değildir."
            : "Seçilen her alıcının kutusuna ayrı satır düşer.";

        pnlRecipients.Visible = !sendToAll;

        if (!sendToAll && !_recipientsLoaded)
        {
            // Hata durumunda yakalama LoadRecipientsAsync içinde yapılır;
            // kip değiştirilince tekrar denenebilir (_recipientsLoaded false kalır).
            _ = LoadRecipientsAsync();
        }

        UpdateSendState();
    }

    /// <summary>Seçim çubuğunu ve gönder düğmesinin durumunu günceller.</summary>
    private void UpdateSendState()
    {
        if (_disposed || Page.IsDisposed)
        {
            return;
        }

        List<MessageRecipientDto> selected = GetSelectedRecipients();

        lblSelection.Text = selected.Count switch
        {
            0 => "Kimse seçilmedi",
            1 => $"1 alıcı seçildi: {selected[0].FullName}",
            _ => $"{selected.Count} alıcı seçildi"
        };

        // "Tüm Kullanıcılara" kipinde hedef kitledir; seçili kipte en az bir
        // satır seçilmelidir. Konu ve metin de zorunlu olduğu için düğme, üçü
        // birden hazır olmadan açılmaz; kullanıcı gönder'e basıp hata yerine
        // eksik alanı görür.
        btnSend.Enabled = (SendToAll || selected.Count > 0)
            && SubjectText.Length > 0
            && BodyText.Length > 0;

        btnClearSelection.Enabled = selected.Count > 0;
        btnSelectAll.Enabled = _recipients.Count > 0 && selected.Count < _recipients.Count;
    }

    /// <summary>Tema değiştiğinde sayfanın renkleri tazelenir.</summary>
    public void ApplySkin()
    {
        if (_disposed || Page.IsDisposed)
        {
            return;
        }

        Color surface = SkinTheme.SurfaceOf(Page);
        Color primary = SkinTheme.Text;
        Color panelLabel = SkinTheme.Blend(primary, surface, 0.10F);
        Color secondary = SkinTheme.Blend(primary, surface, 0.22F);

        void SetSurface(PanelControl panel)
        {
            panel.Appearance.BackColor = surface;
            panel.Appearance.Options.UseBackColor = true;
        }

        pnlHeader.Appearance.BackColor = SkinTheme.SurfaceMuted(surface);
        pnlHeader.Appearance.Options.UseBackColor = true;
        pnlText.BackColor = SkinTheme.SurfaceMuted(surface);

        lblTitle.Appearance.ForeColor = panelLabel;
        lblTitle.Appearance.Options.UseForeColor = true;
        lblDetail.Appearance.ForeColor = secondary;
        lblDetail.Appearance.Options.UseForeColor = true;

        btnClose.Appearance.BackColor = SkinTheme.SurfaceMuted(surface);
        btnClose.Appearance.Options.UseBackColor = true;
        btnClose.Appearance.ForeColor = SkinTheme.MutedText(surface);
        btnClose.Appearance.Options.UseForeColor = true;

        SetSurface(pnlRecipients);
        SetSurface(pnlMode);
        SetSurface(pnlSearch);
        SetSurface(pnlSubject);
        SetSurface(pnlBody);
        SetSurface(pnlFooter);

        grpMode.Properties.Appearance.BackColor = surface;
        grpMode.Properties.Appearance.Options.UseBackColor = true;
        grpMode.Properties.Appearance.ForeColor = primary;
        grpMode.Properties.Appearance.Options.UseForeColor = true;

        gridRecipients.BackColor = surface;
        pnlSelectionBar.BackColor = surface;

        lblSelection.Appearance.ForeColor = secondary;
        lblSelection.Appearance.Options.UseForeColor = true;
        lblRecipientsState.Appearance.ForeColor = secondary;
        lblRecipientsState.Appearance.Options.UseForeColor = true;
        lblHint.Appearance.ForeColor = SkinTheme.MutedText(surface);
        lblHint.Appearance.Options.UseForeColor = true;

        // Gönder düğmesi sayfanın tek aksanıdır.
        Color accent = SkinTheme.Primary;

        btnSend.Appearance.BackColor = accent;
        btnSend.Appearance.Options.UseBackColor = true;
        btnSend.Appearance.ForeColor = SkinTheme.GetContrastText(accent);
        btnSend.Appearance.Options.UseForeColor = true;
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;

        _searchTimer.Stop();
        _searchTimer.Dispose();

        if (!Page.IsDisposed)
        {
            Page.Dispose();
        }
    }
}
