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
using TS.MediatR;
using TS.Result;

namespace Cost.Accounting.Automation.WinFormsApp.Forms.MessageForms
{
    /// <summary>
    /// İki kullanıcı arasındaki konuşmanın geçmişi ve yanıt kutusu.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Geçmiş, yeni mesajlarla birlikte yeniden yüklenir; böylece karşı tarafın
    /// yazdığı mesajlar ekrandaki "Yenile" düğmesine basılmadan da görünür. Bu
    /// ekranda periyodik otomatik yenileme yapılmaz: uygulama tek kullanıcılı
    /// masaüstü aracıdır ve arka planda sorgu çalıştırmak yalnızca gereksiz
    /// yük demektir.
    /// </para>
    /// <para>
    /// Duyuru kanalında mesaj gönderilemez; duyuru tek seferlik ve çok alıcılı
    /// bir işlem olduğu için yalnızca <b>Duyuru Gönder</b> ekranından iletilir.
    /// </para>
    /// </remarks>
    public sealed partial class ConversationForm : XtraFormMdiBase
    {
        private readonly Guid _counterpartId;
        private readonly string _counterpartFullName;
        private readonly string? _counterpartRegistryNumber;
        private readonly string? _counterpartUserName;
        private readonly bool _isAnnouncementChannel;

        /// <summary>
        /// Geçmiş verisini önceden getirilmiş olarak alan <see cref="ISender"/>.
        /// Liste ekranı konuşmayı açmadan önce veriyi çektiği için aynı veri
        /// ikinci kez sorgulanmaz.
        /// </summary>
        private readonly ISender _mediator;

        private readonly IDisposable? skinBinding;

        private Color? _ownRowColor;
        private Color? _ownTextColor;

        public ConversationForm(
            Guid counterpartId,
            string counterpartFullName,
            string? counterpartRegistryNumber,
            string? counterpartUserName,
            bool isAnnouncementChannel,
            List<MessageDto> initialMessages,
            ISender mediator)
            : base(BuildFormTitle(counterpartFullName, isAnnouncementChannel))
        {
            _counterpartId = counterpartId;
            _counterpartFullName = counterpartFullName;
            _counterpartRegistryNumber = counterpartRegistryNumber;
            _counterpartUserName = counterpartUserName;
            _isAnnouncementChannel = isAnnouncementChannel;
            _mediator = mediator;

            InitializeComponent();

            IconOptions.SvgImage = DxIcon.Mail;

            Text = _isAnnouncementChannel
                ? $"Duyuru Kanalı — {_counterpartFullName}"
                : _counterpartFullName;

            lblTitle.Text = Text;
            lblSubtitle.Text = BuildSubtitle();

            ConfigureGrid();
            ShowMessages(initialMessages);

            // Duyuru kanalı tek yönlüdür: yanıt kutusu gizlenir.
            pnlCompose.Visible = !_isAnnouncementChannel;

            btnSend.Click += async (_, _) => await SendAsync();
            btnRefresh.Click += async (_, _) => await ReloadAsync();
            btnClosePage.Click += (_, _) => Close();
            txtReply.KeyDown += TxtReply_KeyDown;

            ApplySkin();
            skinBinding = SkinTheme.Bind(ApplySkin);
        }

        private static string BuildFormTitle(string counterpartFullName, bool isAnnouncementChannel)
            => isAnnouncementChannel
                ? $"Duyuru Kanalı — {counterpartFullName}"
                : counterpartFullName;

        private string BuildSubtitle()
        {
            List<string> parts = [];

            if (!string.IsNullOrWhiteSpace(_counterpartRegistryNumber))
            {
                parts.Add($"Sicil No: {_counterpartRegistryNumber}");
            }

            if (!string.IsNullOrWhiteSpace(_counterpartUserName))
            {
                parts.Add($"Kullanıcı: {_counterpartUserName}");
            }

            if (_isAnnouncementChannel)
            {
                parts.Add("Bu kanalda yalnızca yönetici duyuru gönderebilir");
            }

            return string.Join("   •   ", parts);
        }

        private void ApplySkin()
        {
            Color surface = SkinTheme.SurfaceOf(this);
            Color primary = SkinTheme.Text;
            Color secondary = SkinTheme.Blend(primary, surface, 0.22F);

            pnlHeader.Appearance.BackColor = SkinTheme.SurfaceMuted(surface);
            pnlHeader.Appearance.Options.UseBackColor = true;
            pnlCompose.Appearance.BackColor = surface;
            pnlCompose.Appearance.Options.UseBackColor = true;

            lblTitle.Appearance.ForeColor = primary;
            lblTitle.Appearance.Options.UseForeColor = true;
            lblSubtitle.Appearance.ForeColor = secondary;
            lblSubtitle.Appearance.Options.UseForeColor = true;

            _ownRowColor = SkinTheme.Blend(SkinTheme.Primary, surface, 0.88F);
            _ownTextColor = SkinTheme.GetContrastText(_ownRowColor.Value);

            if (!gridMessages.IsDisposed)
            {
                viewMessages.RefreshData();
            }
        }

        private void ConfigureGrid()
        {
            viewMessages.OptionsBehavior.AutoPopulateColumns = false;
            viewMessages.OptionsView.EnableAppearanceEvenRow = false;
            viewMessages.OptionsView.EnableAppearanceOddRow = false;

            GridColumn senderColumn = viewMessages.Columns.AddField(nameof(MessageDto.SenderFullName));
            senderColumn.Caption = "Gönderen";
            senderColumn.Width = 150;
            senderColumn.VisibleIndex = 1;

            GridColumn sentAt = viewMessages.Columns.AddField(nameof(MessageDto.SentAt));
            sentAt.Caption = "Tarih";
            sentAt.Width = 120;
            sentAt.VisibleIndex = 2;
            sentAt.SortOrder = DevExpress.Data.ColumnSortOrder.Ascending;
            sentAt.SortIndex = 0;

            GridColumn subjectColumn = viewMessages.Columns.AddField(nameof(MessageDto.Subject));
            subjectColumn.Caption = "Konu";
            subjectColumn.Width = 160;
            subjectColumn.VisibleIndex = 3;

            GridColumn bodyColumn = viewMessages.Columns.AddField(nameof(MessageDto.Body));
            bodyColumn.Caption = "Mesaj";
            bodyColumn.Width = 430;
            bodyColumn.VisibleIndex = 4;

            // "Benden mi?" kolonu görünmez ama satır boyamanın anahtarıdır.
            GridColumn ownColumn = viewMessages.Columns.AddField(nameof(MessageDto.SentByCurrentUser));
            ownColumn.Visible = false;

            GridColumn readColumn = viewMessages.Columns.AddField(nameof(MessageDto.IsRead));
            readColumn.Caption = "Okundu";
            readColumn.Width = 80;
            readColumn.VisibleIndex = 5;

            viewMessages.OptionsView.ColumnAutoWidth = false;
            viewMessages.Columns[nameof(MessageDto.Body)]!.Width = 430;

            viewMessages.RowStyle += ViewMessages_RowStyle;
            viewMessages.OptionsView.ShowGroupPanel = false;
            viewMessages.OptionsView.ShowIndicator = false;
            viewMessages.OptionsSelection.MultiSelect = false;
            viewMessages.OptionsBehavior.Editable = false;
        }

        /// <summary>
        /// Kendi mesajları ayırt edilebilir biçimde boyar.
        /// </summary>
        /// <remarks>
        /// Kaynak <b>zemin rengi</b> seçilmiştir: DevExpress'te satır zemini,
        /// yazı tipi renginden daha güçlü bir görsel ayrım sağlar. Kendi
        /// mesajınızın rengi skin'in Primary rengiyle harmanlanarak üretilir;
        /// tam doygun bir renk koyu temada okunmaz hâle gelirdi.
        /// </remarks>
        private void ViewMessages_RowStyle(object? sender, RowStyleEventArgs e)
        {
            if (e.RowHandle < 0 || viewMessages.GetRow(e.RowHandle) is not MessageDto message)
            {
                return;
            }

            if (!message.SentByCurrentUser)
            {
                return;
            }

            if (_ownRowColor is null || _ownTextColor is null)
            {
                Color surface = SkinTheme.SurfaceOf(gridMessages);
                _ownRowColor = SkinTheme.Blend(SkinTheme.Primary, surface, 0.88F);
                _ownTextColor = SkinTheme.GetContrastText(_ownRowColor.Value);
            }

            e.Appearance.BackColor = _ownRowColor.Value;
            e.Appearance.Options.UseBackColor = true;
            e.Appearance.Font = new Font("Segoe UI", 9.25F);
            e.Appearance.Options.UseFont = true;
        }

        private void ShowMessages(List<MessageDto> messages)
        {
            gridMessages.DataSource = messages
                .OrderBy(m => m.SentAt)
                .ToList();

            viewMessages.RefreshData();

            // Yeni mesajlar görünür olsun diye en son satıra kaydırılır.
            if (viewMessages.DataRowCount > 0)
            {
                viewMessages.MoveLast();
            }
        }

        /// <summary>Ctrl+Enter ile gönderme kısayolu.</summary>
        private async void TxtReply_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                await SendAsync();
            }
        }

        private async Task ReloadAsync()
        {
            await LoadingHelper.RunAsync(async () =>
            {
                Result<List<MessageDto>> result = await _mediator.Send(
                    new MessageConversationQuery(_counterpartId, _isAnnouncementChannel),
                    CancellationToken.None);

                if (result.IsSuccessful && result.Data is not null)
                {
                    ShowMessages(result.Data);
                }
            },
            caption: "Konuşma yükleniyor...",
            description: "Lütfen bekleyin...");
        }

        private async Task SendAsync()
        {
            string body = txtReply.Text.Trim();

            if (body.Length == 0)
            {
                ToastHelper.Show("Mesaj metni boş olamaz.", ToastType.Warning);
                txtReply.Focus();
                return;
            }

            string? subject = txtSubject.Text.Trim();

            btnSend.Enabled = false;

            try
            {
                Result<string> result = await _mediator.Send(
                    new MessageSendCommand(_counterpartId, body, string.IsNullOrWhiteSpace(subject) ? null : subject),
                    CancellationToken.None);

                if (!result.IsSuccessful)
                {
                    ToastHelper.Show(
                        AuthFormStyles.GetErrorText(result.ErrorMessages), ToastType.Error, 5000);
                    return;
                }

                txtReply.Text = string.Empty;
                txtSubject.Text = string.Empty;

                ToastHelper.Show("Mesaj gönderildi", ToastType.Success, 2000);

                await ReloadAsync();
            }
            catch (Exception ex)
            {
                CrashLog.WriteException("Message.Send", ex);
                ToastHelper.Show("Mesaj gönderilemedi: " + ex.Message, ToastType.Error, 5000);
            }
            finally
            {
                btnSend.Enabled = true;
            }
        }
    }
}