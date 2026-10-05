using System.Drawing;
using Cost.Accounting.Automation.Application.Messages;
using Cost.Accounting.Automation.WinFormsApp.Forms.BaseForm;
using Cost.Accounting.Automation.WinFormsApp.Forms.MainForms;
using Cost.Accounting.Automation.WinFormsApp.Tools;
using Cost.Accounting.Automation.WinFormsApp.Utils;
using DevExpress.XtraEditors;
using DevExpress.XtraGrid.Columns;
using DevExpress.XtraGrid.Views.Grid;
using Microsoft.Extensions.DependencyInjection;
using TS.MediatR;
using TS.Result;

namespace Cost.Accounting.Automation.WinFormsApp.Forms.MessageForms
{
    /// <summary>
    /// Kullanıcının mesaj ekranı: duyuru kanalı ve kişisel konuşmaların listesi.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Konuşmalar gönderene göre gruplanır: aynı kişiden gelen mesajlar tek
    /// satırda birleşir. Yönetici duyuruları ayrı grupta toplanır; çünkü bir
    /// kişiye birebir yazmakla herkese duyuru göndermek farklı bir konuşmadır.
    /// </para>
    /// <para>
    /// Gönderen kendi mesajını gelen kutusunda görmez. Gönderdiği mesajlara
    /// <b>Yeni Mesaj</b> ekranından ulaşır; kalıcı bir "gönderilmiş" kutusu
    /// bu ekranın kapsamı dışındadır.
    /// </para>
    /// </remarks>
    public sealed partial class MessagesListForm : XtraFormMdiBase
    {
private readonly IDisposable? skinBinding;

        private List<ConversationDto> _allConversations = [];

        /// <summary>Okunmamış satırların zemin rengi (skin'e göre hesaplanır).</summary>
        private Color? _unreadRowColor;

public MessagesListForm()
            : base("Mesajlar")
        {
            InitializeComponent();

            IconOptions.SvgImage = DxIcon.Mail;
            lblTitle.Text = "Mesajlar";

            WireEvents();
            ConfigureGrid();
            ApplySkin();
            skinBinding = SkinTheme.Bind(ApplySkin);
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            _ = ReloadAsync();
        }

        private void WireEvents()
        {
            btnRefresh.Click += async (_, _) => await ReloadAsync();
            btnClosePage.Click += (_, _) => Close();
            btnNewMessage.Click += async (_, _) => await ComposeAsync(announcement: false);
            btnAnnouncement.Click += async (_, _) => await ComposeAsync(announcement: true);
            btnUnreadOnly.CheckedChanged += async (_, _) => await ReloadAsync();
            gridConversations.DoubleClick += GridConversations_DoubleClick;
        }

        /// <summary>
        /// Etiket renkleri aktif skinden çözülür. Buton rengi atanmaz; DevExpress
        /// skin'inin kendi buton görünümü geçerli kalır.
        /// </summary>
        private void ApplySkin()
        {
            Color surface = SkinTheme.SurfaceOf(this);
            Color primary = SkinTheme.Text;
            Color secondary = SkinTheme.Blend(primary, surface, 0.22F);

            pnlHeader.Appearance.BackColor = SkinTheme.SurfaceMuted(surface);
            pnlHeader.Appearance.Options.UseBackColor = true;
            pnlToolbar.Appearance.BackColor = surface;
            pnlToolbar.Appearance.Options.UseBackColor = true;

            lblTitle.Appearance.ForeColor = primary;
            lblTitle.Appearance.Options.UseForeColor = true;
            lblSubtitle.Appearance.ForeColor = secondary;
            lblSubtitle.Appearance.Options.UseForeColor = true;
            lblEmpty.Appearance.ForeColor = SkinTheme.MutedText(surface);
            lblEmpty.Appearance.Options.UseForeColor = true;

            _unreadRowColor = SkinTheme.Blend(SkinTheme.Primary, surface, 0.86F);

            if (!gridConversations.IsDisposed)
            {
                viewConversations.RefreshData();
            }
        }

        private void ConfigureGrid()
        {
            viewConversations.OptionsBehavior.AutoPopulateColumns = false;
            viewConversations.OptionsView.ShowGroupPanel = true;
            viewConversations.OptionsView.ShowGroupPanelColumnsAsSingleRow = true;
            viewConversations.OptionsView.EnableAppearanceEvenRow = true;
            viewConversations.OptionsView.EnableAppearanceOddRow = true;

            GridColumn counterpart = viewConversations.Columns.AddField(nameof(ConversationDto.CounterpartFullName));
            counterpart.Caption = "Karşı Taraf";
            counterpart.Width = 190;
            counterpart.VisibleIndex = 1;

            GridColumn registry = viewConversations.Columns.AddField(nameof(ConversationDto.CounterpartRegistryNumber));
            registry.Caption = "Sicil No";
            registry.Width = 90;
            registry.VisibleIndex = 2;

            GridColumn subject = viewConversations.Columns.AddField(nameof(ConversationDto.LastSubject));
            subject.Caption = "Son Mesaj Konusu";
            subject.Width = 210;
            subject.VisibleIndex = 3;

            GridColumn preview = viewConversations.Columns.AddField(nameof(ConversationDto.LastPreview));
            preview.Caption = "Önizleme";
            preview.Width = 300;
            preview.VisibleIndex = 4;

            GridColumn unread = viewConversations.Columns.AddField(nameof(ConversationDto.UnreadCount));
            unread.Caption = "Okunmamış";
            unread.Width = 90;
            unread.VisibleIndex = 5;

            GridColumn lastAt = viewConversations.Columns.AddField(nameof(ConversationDto.LastMessageAt));
            lastAt.Caption = "Tarih";
            lastAt.Width = 130;
            lastAt.VisibleIndex = 6;
            lastAt.SortOrder = DevExpress.Data.ColumnSortOrder.Descending;
            lastAt.SortIndex = 0;

            GridColumn company = viewConversations.Columns.AddField(nameof(ConversationDto.CounterpartCompanyName));
            company.Caption = "Kurum";
            company.Width = 130;
            company.VisibleIndex = 7;

            GridColumn userName = viewConversations.Columns.AddField(nameof(ConversationDto.CounterpartUserName));
            userName.Caption = "Kullanıcı Adı";
            userName.Width = 120;
            userName.VisibleIndex = 8;

            GridColumn tcNo = viewConversations.Columns.AddField(nameof(ConversationDto.CounterpartTcNo));
            tcNo.Caption = "TC Kimlik No";
            tcNo.Width = 120;
            tcNo.VisibleIndex = 9;

// Kanal, boolean yerine okunabilir metin üzerinden gruplanır: ham
            // "True/False" grup başlığı kullanıcıya hiçbir şey anlatmaz.
            GridColumn channel = viewConversations.Columns.AddField(nameof(ConversationDto.ChannelCaption));
            channel.Caption = "Kanal";
            channel.Width = 150;
            channel.VisibleIndex = 0;
            channel.Group();

            viewConversations.Appearance.GroupRow.Font = new Font("Segoe UI Semibold", 9.5F);
            viewConversations.Appearance.GroupRow.Options.UseFont = true;

            viewConversations.RowStyle += ViewConversations_RowStyle;

            viewConversations.Columns[nameof(ConversationDto.CounterpartTcNo)]!
                .ToolTip = "Karşı tarafın TC kimlik numarası; mesaj gönderirken aramada kullanılabilir.";
        }

        /// <summary>Okunmamış konuşmaların satırı belirgin biçimde vurgulanır.</summary>
        private void ViewConversations_RowStyle(object? sender, RowStyleEventArgs e)
        {
            if (e.RowHandle < 0
                || viewConversations.IsGroupRow(e.RowHandle)
                || viewConversations.GetRow(e.RowHandle) is not ConversationDto conversation
                || conversation.UnreadCount <= 0)
            {
                return;
            }

            if (_unreadRowColor is null)
            {
                Color surface = SkinTheme.SurfaceOf(gridConversations);
                _unreadRowColor = SkinTheme.Blend(SkinTheme.Primary, surface, 0.86F);
            }

            e.Appearance.BackColor = _unreadRowColor.Value;
            e.Appearance.Options.UseBackColor = true;
        }

        private async Task ReloadAsync()
        {
            await LoadingHelper.RunAsync(
                ReloadCoreAsync,
                caption: "Mesajlar yükleniyor...",
                description: "Lütfen bekleyin...");

            bool onlyUnread = btnUnreadOnly.Checked;

            List<ConversationDto> visible = onlyUnread
                ? _allConversations.Where(c => c.UnreadCount > 0).ToList()
                : _allConversations;

            gridConversations.DataSource = visible;

            lblEmpty.Visible = visible.Count == 0;
            lblEmpty.Text = onlyUnread
                ? "Okunmamış mesajınız yok."
                : "Henüz mesajınız yok.\r\n\"Yeni Mesaj\" ile bir kullanıcıya yazabilir, \"Duyuru Gönder\" ile şirket geneline duyuru iletebilirsiniz.";

            int unreadTotal = _allConversations.Sum(c => c.UnreadCount);

            lblSubtitle.Text = _allConversations.Count == 0
                ? "Gelen kutunuz boş"
                : $"{_allConversations.Count} konuşma listeleniyor"
                    + (unreadTotal > 0 ? $"  •  {unreadTotal} okunmamış mesaj" : string.Empty);
        }

        private async Task ReloadCoreAsync()
        {
            using var scope = Program.Services.CreateScope();
            ISender mediator = scope.ServiceProvider.GetRequiredService<ISender>();

            Result<List<ConversationDto>> result = await mediator.Send(
                new MessageInboxQuery(), CancellationToken.None);

            _allConversations = result.Data ?? [];

            // Duyuru yetkisi olmayan kullanıcıda düğme hiç gösterilmez.
            btnAnnouncement.Visible = await CurrentUserPermissions.HasAsync(MessagePermissions.Announce);
        }

private async void GridConversations_DoubleClick(object? sender, EventArgs e)
        {
            if (viewConversations.GetFocusedRow() is ConversationDto conversation)
            {
                await OpenConversationAsync(conversation);
            }
        }

private async Task OpenConversationAsync(ConversationDto conversation)
        {
            XtraForm? container = ResolveMdiContainer();

            if (container is null)
            {
                ToastHelper.Show("Pencere açılamadı.", ToastType.Error);
                return;
            }

            using var scope = Program.Services.CreateScope();
            ISender mediator = scope.ServiceProvider.GetRequiredService<ISender>();

            Result<List<MessageDto>> result = await mediator.Send(
                new MessageConversationQuery(
                    conversation.CounterpartId,
                    conversation.IsAnnouncementChannel),
                CancellationToken.None);

            if (!result.IsSuccessful || result.Data is null)
            {
                ToastHelper.Show(
                    AuthFormStyles.GetErrorText(result.ErrorMessages), ToastType.Error, 4000);
                return;
            }

MdiFormManager.Instance.OpenForm<ConversationForm>(
                container,
                conversation.CounterpartFullName,
                () => new ConversationForm(
                    conversation.CounterpartId,
                    conversation.CounterpartFullName,
                    conversation.CounterpartRegistryNumber,
                    conversation.CounterpartUserName,
                    conversation.IsAnnouncementChannel,
                    result.Data,
                    mediator));
        }

/// <summary>
        /// Mesaj yazma ve duyuru ekranını açar, kapanmasını bekler ve listeyi
        /// tazeler.
        /// </summary>
        /// <remarks>
        /// Yazma ekranı bir MDI çocuğu olarak açılır ve kapanana kadar beklenir.
        /// Böylece mesaj gönderildikten sonra gelen kutusu kullanıcı ekrana
        /// döndüğünde güncel görünür; kullanıcının "Yenile" düğmesine basması
        /// gerekmez.
        /// </remarks>
        private async Task ComposeAsync(bool announcement)
        {
            if (announcement && !await CurrentUserPermissions.HasAsync(MessagePermissions.Announce))
            {
                ToastHelper.Show("Duyuru gönderme yetkiniz yok.", ToastType.Warning);
                return;
            }

            if (!await CurrentUserPermissions.HasAsync(MessagePermissions.Send))
            {
                ToastHelper.Show("Mesaj gönderme yetkiniz yok.", ToastType.Warning);
                return;
            }

            XtraForm? container = ResolveMdiContainer();

            if (container is null)
            {
                ToastHelper.Show("Pencere açılamadı.", ToastType.Error);
                return;
            }

            NewMessageForm form = MdiFormManager.Instance.OpenForm<NewMessageForm>(
                container,
                announcement ? "Duyuru Gönder" : "Yeni Mesaj",
                () => new NewMessageForm(announcement));

            await form.WaitForCompletionAsync();
            await ReloadAsync();
        }

        /// <summary>
        /// MDI çocuğu açılacak asıl kapsayıcıyı bulur.
        /// </summary>
        /// <remarks>
        /// MDI çocuk form, başka bir MDI çocuğu doğuramaz. Bu liste ekranı bir
        /// MDI çocuğu olduğu için hedef daima asıl kapsayıcıdır; form henüz
        /// MDI'ye eklenmemişse (tasarım/test) formun kendisi kullanılır.
        /// </remarks>
        private XtraForm? ResolveMdiContainer()
        {
            if (MdiParent is XtraForm parent)
            {
                return parent;
            }

            // MDI'ye eklenmeden önce çağrılırsa (tasarım yüzeyi, test) formun
            // kendisi hedeflenir; MdiParent ataması WinForms tarafından reddedilir.
            return this;
        }
    }
}