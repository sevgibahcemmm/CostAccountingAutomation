using System.Drawing;
using Cost.Accounting.Automation.Application.Messages;
using Cost.Accounting.Automation.WinFormsApp.Forms.BaseForm;
using Cost.Accounting.Automation.WinFormsApp.Forms.MainForms;
using Cost.Accounting.Automation.WinFormsApp.Tools;
using Cost.Accounting.Automation.WinFormsApp.Utils;
using DevExpress.Utils;
using DevExpress.XtraEditors;
using DevExpress.XtraGrid.Columns;
using DevExpress.XtraGrid.Views.Grid;
using Microsoft.Extensions.DependencyInjection;
using TS.MediatR;
using TS.Result;

namespace Cost.Accounting.Automation.WinFormsApp.Forms.MessageForms
{
    /// <summary>
    /// Tek kullanıcıya mesaj gönderme ve yöneticiler için duyuru gönderme ekranı.
    /// </summary>
    /// <remarks>
    /// <para>
    /// İki işlem tek ekranda toplanmıştır. Ayrı ayrı ekranlar yapılabilirdi,
    /// ancak alıcı seçme, konu ve gövde yazma akışları birebir aynıdır;
    /// ayrım yalnızca son düğmenin etiketi ve gönderim biçimindedir. Bu
    /// yüzden kullanıcı "duyuru gönder" dediğinde aynı alışkanlıkla çalışır.
    /// </para>
    /// <para>
    /// Duyuru modunda tek alıcı da seçilebilir: tek kişiye yapılan duyuru
    /// geçerli bir işlemdir ve her alıcı için ayrı kayıt tutulması sayesinde
    /// okunma durumu kişi bazında izlenebilir.
    /// </para>
    /// </remarks>
    public sealed partial class NewMessageForm : XtraFormMdiBase
    {
        /// <summary>Arama kutusu boşaltıldıktan sonra bekleme süresi.</summary>
        private const int SearchDebounceMilliseconds = 300;

        private readonly bool _announcement;

        private readonly IDisposable? skinBinding;

        /// <summary>Ana liste ekranının gönderim sonrası tazelemesini beklettirir.</summary>
        private readonly TaskCompletionSource _completed = new(TaskCreationOptions.RunContinuationsAsynchronously);

        private List<MessageRecipientDto> _recipients = [];

        private System.Windows.Forms.Timer? _searchTimer;

        public NewMessageForm(bool announcement) : base("Yeni Mesaj")
        {
            _announcement = announcement;

            InitializeComponent();

            Text = announcement ? "Duyuru Gönder" : "Yeni Mesaj";
            lblTitle.Text = Text;

            lblSubtitle.Text = announcement
                ? "Duyuru modunda: her alıcı için ayrı kayıt oluşturulur ve okunma durumu kişi bazında izlenir."
                : "Sicil numarası, TC kimlik numarası, ad soyad veya kullanıcı adıyla alıcı arayın.";

            btnSend.Text = announcement ? "Duyuruyu Gönder" : "Gönder";

            IconOptions.SvgImage = DxIcon.Mail;

ConfigureGrid();
        WireEvents();
        ApplySkin();
        skinBinding = SkinTheme.Bind(ApplySkin);

        // Pencere görünür olduğu anda alıcılar yüklenir. OnLoad tek başına
        // yeterli değildir: MDI altında açılan pencerelerde handle henüz
        // oluşmamış olabiliyor ve yükleme atlanıyordu.
        Shown += async (_, _) => await LoadRecipientsAsync();
    }

        /// <summary>
        /// Form kapanana kadar bekler. Ana liste ekranı, gönderim yapılıp
        /// yapılmadığını bilmeden tazeleme yapmamalıdır.
        /// </summary>
        public Task WaitForCompletionAsync() => _completed.Task;

        private void WireEvents()
        {
            btnCancel.Click += (_, _) => Close();
            btnClearSelection.Click += (_, _) => ClearSelection();
            btnSend.Click += async (_, _) => await SendAsync();
            txtSearch.TextChanged += TxtSearch_TextChanged;
            txtBody.KeyDown += TxtBody_KeyDown;

            // Konu alanından da Enter ile gönderilebilir.
            txtSubject.KeyDown += TxtBody_KeyDown;
            viewRecipients.SelectionChanged += (_, _) => UpdateSelectionLabel();
            FormClosed += (_, _) => _completed.TrySetResult();
        }

        private void ApplySkin()
        {
            Color surface = SkinTheme.SurfaceOf(this);
            Color primary = SkinTheme.Text;
            Color secondary = SkinTheme.Blend(primary, surface, 0.22F);

            pnlHeader.Appearance.BackColor = SkinTheme.SurfaceMuted(surface);
            pnlHeader.Appearance.Options.UseBackColor = true;

            lblTitle.Appearance.ForeColor = primary;
            lblTitle.Appearance.Options.UseForeColor = true;
            lblSubtitle.Appearance.ForeColor = secondary;
            lblSubtitle.Appearance.Options.UseForeColor = true;
        }

        private void ConfigureGrid()
        {
            viewRecipients.OptionsBehavior.AutoPopulateColumns = false;
            viewRecipients.OptionsView.ShowIndicator = false;

            GridColumn fullName = viewRecipients.Columns.AddField(nameof(MessageRecipientDto.FullName));
            fullName.Caption = "Ad Soyad";
            fullName.Width = 190;
            fullName.VisibleIndex = 0;

            GridColumn registry = viewRecipients.Columns.AddField(nameof(MessageRecipientDto.RegistryNumber));
            registry.Caption = "Sicil No";
            registry.Width = 90;
            registry.VisibleIndex = 1;

            GridColumn tcNo = viewRecipients.Columns.AddField(nameof(MessageRecipientDto.TcNo));
            tcNo.Caption = "TC Kimlik No";
            tcNo.Width = 130;
            tcNo.VisibleIndex = 2;

            GridColumn userName = viewRecipients.Columns.AddField(nameof(MessageRecipientDto.UserName));
            userName.Caption = "Kullanıcı Adı";
            userName.Width = 130;
            userName.VisibleIndex = 3;

            GridColumn company = viewRecipients.Columns.AddField(nameof(MessageRecipientDto.CompanyName));
            company.Caption = "Kurum";
            company.Width = 180;
            company.VisibleIndex = 4;

            GridColumn role = viewRecipients.Columns.AddField(nameof(MessageRecipientDto.RoleName));
            role.Caption = "Rol";
            role.Width = 150;
            role.VisibleIndex = 5;

            viewRecipients.OptionsView.ColumnAutoWidth = false;
        }

        /// <summary>
        /// Her tuş vuruşunda sorgu atmak yerine kısa bir gecikme beklenir;
        /// aksi hâlde "778" yazan kullanıcı üç ayrı sorgu tetikler.
        /// </summary>
        private void TxtSearch_TextChanged(object? sender, EventArgs e)
        {
            _searchTimer ??= new System.Windows.Forms.Timer { Interval = SearchDebounceMilliseconds };
            _searchTimer.Tick -= SearchTimer_Tick;
            _searchTimer.Tick += SearchTimer_Tick;
            _searchTimer.Stop();
            _searchTimer.Start();
        }

        private async void SearchTimer_Tick(object? sender, EventArgs e)
        {
            _searchTimer!.Stop();

            await LoadRecipientsAsync();
        }

        protected override async void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            // Yazma ekranı da konuşma ekranı gibi ekranı kaplamaz; MDI alanının
            // ortasında dar bir kart olarak açılır.
            CenterInMdiClient();

            // Alıcı listesi ilk açılışta bir kez yüklenir. Daha önce
            // OnShow/Shown yerine OnLoad kullanılmıyordu: yazma ekranı MDI
            // altında açıldığında liste boş kalıyordu.
            await LoadRecipientsAsync();
        }

        private async Task LoadRecipientsAsync()
        {
            if (!IsHandleCreated && !IsDisposed)
            {
                // Tasarım yüzeyinde yüklemeye çalışmak, kontroller henüz
                // oluşmadığı için null başvuruya yol açar.
                return;
            }

            string term = txtSearch.Text.Trim();

            try
            {
                using var scope = Program.Services.CreateScope();
                ISender mediator = scope.ServiceProvider.GetRequiredService<ISender>();

                Result<List<MessageRecipientDto>> result = await mediator.Send(
                    new MessageRecipientSearchQuery(term, ExcludeSelf: true, MaxResults: 50),
                    CancellationToken.None);

                _recipients = result.Data ?? [];
            }
            catch (Exception ex)
            {
                CrashLog.WriteException("Message.RecipientSearch", ex);
                _recipients = [];
                ToastHelper.Show("Alıcılar yüklenemedi: " + ex.Message, ToastType.Error, 4000);
            }

            // Arama sırasında seçili satırlar korunur: kullanıcı adı yazarken
            // alıcılarını kaybetmek istemez.
            HashSet<Guid> previouslySelected = GetSelectedIds();

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

            UpdateSelectionLabel();
        }

        private List<MessageRecipientDto> GetSelectedRecipients()
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

        private HashSet<Guid> GetSelectedIds()
            => GetSelectedRecipients().Select(r => r.Id).ToHashSet();

        private void ClearSelection()
        {
            viewRecipients.ClearSelection();
            UpdateSelectionLabel();
        }

        private void UpdateSelectionLabel()
        {
            List<MessageRecipientDto> selected = GetSelectedRecipients();

            lblSelection.Text = selected.Count switch
            {
                0 => "Kimse seçilmedi",
                1 => $"1 alıcı seçildi: {selected[0].FullName}",
                _ => $"{selected.Count} alıcı seçildi"
                    + (_announcement ? string.Empty : "  •  Duyuru için birden çok kişi seçemezsiniz")
            };

            bool canSend = selected.Count > 0
                && (selected.Count == 1 || _announcement);

            btnSend.Enabled = canSend;
            btnClearSelection.Enabled = selected.Count > 0;
        }

        /// <summary>
        /// <b>Enter</b> gönderir; <b>Shift+Enter</b> satır atlar.
        /// </summary>
        /// <remarks>
        /// Konuşma penceresiyle aynı davranış. Alıcı seçilmemişse gönderme
        /// düğmesi pasif olduğu için Enter da bir şey yapmaz.
        /// </remarks>
        private async void TxtBody_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Enter || e.Shift)
            {
                return;
            }

            if (!btnSend.Enabled)
            {
                return;
            }

            e.SuppressKeyPress = true;

            await SendAsync();
        }

        private async Task SendAsync()
        {
            List<MessageRecipientDto> selected = GetSelectedRecipients();

            if (selected.Count == 0)
            {
                ToastHelper.Show("En az bir alıcı seçin.", ToastType.Warning);
                return;
            }

            if (selected.Count > 1 && !_announcement)
            {
                ToastHelper.Show(
                    "Birden çok kişiye göndermek için duyuru modunu kullanın.", ToastType.Warning, 4000);
                return;
            }

            string body = txtBody.Text.Trim();

            if (body.Length == 0)
            {
                ToastHelper.Show("Mesaj metni boş olamaz.", ToastType.Warning);
                txtBody.Focus();
                return;
            }

            string? subject = txtSubject.Text.Trim();

            if (string.IsNullOrWhiteSpace(subject))
            {
                subject = null;
            }

            Guid[] recipientIds = selected.Select(r => r.Id).ToArray();

            btnSend.Enabled = false;

            try
            {
                using var scope = Program.Services.CreateScope();
                ISender mediator = scope.ServiceProvider.GetRequiredService<ISender>();

                Result<string> result = _announcement
                    ? await mediator.Send(
                        new MessageBroadcastCommand(recipientIds, body, subject), CancellationToken.None)
                    : await mediator.Send(
                        new MessageSendCommand(recipientIds[0], body, subject), CancellationToken.None);

                if (!result.IsSuccessful)
                {
                    ToastHelper.Show(
                        AuthFormStyles.GetErrorText(result.ErrorMessages), ToastType.Error, 5000);
                    return;
                }

                ToastHelper.Show(
                    _announcement ? result.Data ?? "Duyuru gönderildi" : "Mesaj gönderildi",
                    ToastType.Success, 2500);

                Close();
            }
            catch (Exception ex)
            {
                CrashLog.WriteException("Message.Compose", ex);
                ToastHelper.Show("Mesaj gönderilemedi: " + ex.Message, ToastType.Error, 5000);
            }
            finally
            {
                btnSend.Enabled = true;
            }
        }
    }
}