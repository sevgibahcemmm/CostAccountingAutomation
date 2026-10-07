using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.IO;
using Cost.Accounting.Automation.Application.Messages;
using Cost.Accounting.Automation.Application.Services;
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

namespace Cost.Accounting.Automation.WinFormsApp.Forms.MessageForms
{

    public sealed partial class MessagesListForm : XtraFormMdiBase
    {
        private readonly IDisposable? skinBinding;

        private readonly LiveMessagingService _live;

        private List<DirectoryRow> _allContacts = [];

        /// <summary>Sağ alanda açık olan konuşma sekmeleri; biri açıkken diğeri kapanmaz.</summary>
        private readonly Dictionary<Guid, InlineChatPage> _chatPages = [];

        /// <summary>Şu an görüntülenen sekmeye karşılık gelen karşı taraf.</summary>
        private Guid? _activeChatId;

        /// <summary>
        /// Sağ alanda açık olan duyuru yazma sayfası. Sohbetler gibi o da bu
        /// ekranın sağ yarısındaki bir sekmede durur; aynı anda yalnızca bir
        /// tane açılır.
        /// </summary>
        private AnnouncementComposePage? _announcementPage;

        /// <summary>Bildirimden gelen ve liste yüklendikten sonra açılacak konuşma.</summary>
        private (Guid Id, string Name)? _pendingNotificationConversation;

        /// <summary>Açık sohbetlerin arka plan tazelemesi sırasında yinelenen sorguyu önler.</summary>
        private bool _chatReloading;

        /// <summary>Çevrimiçi kullanıcı satırlarının hafif yeşil zemini.</summary>
        private Color? _onlineRowColor;

        /// <summary>Pasif kullanıcı satırlarının soluk metin rengi.</summary>
        private Color? _offlineTextColor;

        /// <summary>
        /// Kullanıcı başına profil fotoğrafı. Boyama olayı her satır için çok
        /// sık çalıştığı için dosya her seferinde okunmaz; bir kez yüklenip
        /// bellekte tutulur. Yol değiştiyse (kullanıcı fotoğrafını
        /// güncellediyse) kayıt yeniden okunur. Değer <c>null</c> ise dosya
        /// bulunamadı/dokunulamadı demektir ve baş harf avatarı çizilir.
        /// </summary>
        private readonly Dictionary<Guid, (string Path, Image? Image)> _avatarCache = [];

        private readonly object _avatarCacheLock = new();

        // ----------------------------------------------------------------
        // Sohbet tarzı satır çizimi: avatar, iki satır metin ve rozet.
        // Statik yazı tipleri her boyamada yeniden üretilmez; renkler ise
        // her seferinde SkinTheme'den çözünür, böylece tema değişimi anında
        // görünür.
        // ----------------------------------------------------------------

        private static readonly Font InitialsFont = new("Segoe UI", 9.5F, FontStyle.Bold);

        private static readonly Font RowNameFont = new("Segoe UI", 9.5F, FontStyle.Bold);

        private static readonly Font RowDetailFont = new("Segoe UI", 8.5F);

        private static readonly Font BadgeFont = new("Segoe UI", 8F, FontStyle.Bold);

        /// <summary>Ad soyaddan üretilen avatarın zemin renkleri.</summary>
        private static readonly Color[] AvatarPalette =
        [
            Color.FromArgb(37, 99, 235),
            Color.FromArgb(106, 27, 154),
            Color.FromArgb(0, 121, 107),
            Color.FromArgb(198, 102, 0),
            Color.FromArgb(55, 71, 79)
        ];

        /// <summary>Kullanıcı listesi arama kutusundaki yazı.</summary>
        private string _contactSearch = string.Empty;

        /// <summary>
        /// Listeye en son atanan satırların kimlikleri. Yenileme sırasında liste
        /// içeriğinin değişip değişmediğini bununla karşılaştırarak anlarız.
        /// </summary>
        private string _contactsKey = string.Empty;

        private bool _reloading;

        // ----------------------------------------------------------------
        // Sohbet geçmişi ve ekler
        // ----------------------------------------------------------------

        /// <summary>Sohbet açılırken/tazelenirken yüklenen en yeni mesaj sayısı.</summary>
        private const int InitialHistoryPageSize = 50;

        /// <summary>"Geçmişi Göster" ile bir seferde yüklenen eski mesaj sayısı.</summary>
        private const int OlderHistoryPageSize = 50;

        /// <summary>Ek için izin verilen en büyük dosya boyutu: 25 MB.</summary>
        private const long MaxAttachmentBytes = 25L * 1024 * 1024;

        public MessagesListForm()
            : base("Mesajlar")
        {
            InitializeComponent();

            IconOptions.SvgImage = DxIcon.WhatsApp;
            btnAnnouncement.ImageOptions.SvgImage = DxIcon.WhatsApp;
            lblTitle.Text = "Mesajlar";

            _live = Program.Services.GetRequiredService<LiveMessagingService>();

            WireEvents();
            ConfigureContactsGrid();
            ApplySkin();
            skinBinding = SkinTheme.Bind(ApplySkin);
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            // Küçük bir açılış penceresi gibi ortalanır; sol üst köşeye
            // yapışması sayfayı olduğu gibi bir "diyalog" hissine çevirir.
            CenterInMdiClient();

            _ = ReloadAsync(showLoading: true);

            // Canlı güncelleme yalnızca ekran açıkken dinlenir; pencere kapanınca
            // abonelik bırakılır (OnFormClosed).
            _live.Polled += Live_Polled;
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            _live.Polled -= Live_Polled;

            _announcementPage?.Dispose();
            _announcementPage = null;

            // Bellekte tutulan profil fotoğrafları artık boyanmıyor; kaynak
            // bırakılmazsa pencere her açılıp kapandığında sızıntı olur.
            lock (_avatarCacheLock)
            {
                foreach (KeyValuePair<Guid, (string Path, Image? Image)> avatar in _avatarCache)
                {
                    avatar.Value.Image?.Dispose();
                }

                _avatarCache.Clear();
            }

            base.OnFormClosed(e);
        }

        private void WireEvents()
        {
            btnAnnouncement.Click += async (_, _) => await ComposeAnnouncementAsync();
            txtContactSearch.TextChanged += TxtContactSearch_TextChanged;

            // Sohbet uygulamalarındaki gibi tek tık yeterlidir: kullanıcı
            // listede bir isme basar, konuşma sağ alanda yeni bir sekmeyle açılır.
            viewUsers.RowClick += ViewUsers_RowClick;
            viewUsers.KeyDown += ViewUsers_KeyDown;

            tabConversations.SelectedPageChanged += TabConversations_SelectedPageChanged;
        }

        /// <summary>
        /// <b>Enter</b> gönderir; <b>Shift+Enter</b> satır atlar.
        /// </summary>
        private async void OnReplyKeyDown(InlineChatPage page, object? sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Enter)
            {
                return;
            }

            if (e.Shift)
            {
                return;
            }

            e.SuppressKeyPress = true;

            await SendAsync(page);
        }

        // ----------------------------------------------------------------
        // Canlı güncelleme
        // ----------------------------------------------------------------

        /// <summary>
        /// Yoklama tamamlandığında listeler sessizce tazelenir.
        /// </summary>
        /// <remarks>
        /// Olay arka plan iş parçacığından gelir; bu yüzden arayüz iş parçacığına
        /// taşınır. "Yenile" düğmesiyle aynı yol kullanılır ama bekleme penceresi
        /// gösterilmez: on saniyede bir açılıp kapanan bir pencere rahatsız ederdi.
        /// </remarks>
        private void Live_Polled(object? sender, EventArgs e)
        {
            _live.PostToUi(() =>
            {
                if (IsDisposed || Disposing)
                {
                    return;
                }

                _ = ReloadAsync(showLoading: false);

                // Açık sohbet sekmeleri varsa arka planda tazelenir; okunma
                // yalnızca o an görüntülenen sekmeye işaretlenir.
                _ = RefreshChatPagesAsync();
            });
        }

        // ----------------------------------------------------------------
        // Görünüm
        // ----------------------------------------------------------------

        private void ApplySkin()
        {
            Color surface = SkinTheme.SurfaceOf(this);
            Color primary = SkinTheme.Text;
            Color secondary = SkinTheme.Blend(primary, surface, 0.22F);

            pnlHeader.Appearance.BackColor = SkinTheme.SurfaceMuted(surface);
            pnlHeader.Appearance.Options.UseBackColor = true;
            pnlToolbar.Appearance.BackColor = surface;
            pnlToolbar.Appearance.Options.UseBackColor = true;
            pnlContacts.Appearance.BackColor = surface;
            pnlContacts.Appearance.Options.UseBackColor = true;
            pnlConversations.Appearance.BackColor = surface;
            pnlConversations.Appearance.Options.UseBackColor = true;

            lblTitle.Appearance.ForeColor = primary;
            lblTitle.Appearance.Options.UseForeColor = true;
            lblSubtitle.Appearance.ForeColor = secondary;
            lblSubtitle.Appearance.Options.UseForeColor = true;
            lblEmpty.Appearance.ForeColor = SkinTheme.MutedText(surface);
            lblEmpty.Appearance.Options.UseForeColor = true;

            Color panelLabel = SkinTheme.Blend(primary, surface, 0.10F);

            lblContactsTitle.Appearance.ForeColor = panelLabel;
            lblContactsTitle.Appearance.Options.UseForeColor = true;

            lblContactsCount.Appearance.ForeColor = SkinTheme.MutedText(surface);
            lblContactsCount.Appearance.Options.UseForeColor = true;

            _onlineRowColor = SkinTheme.Blend(SkinTheme.Success, surface, SkinTheme.IsDarkSkin ? 0.16F : 0.10F);
            _offlineTextColor = SkinTheme.MutedText(surface);

            foreach (InlineChatPage page in _chatPages.Values)
            {
                ApplyChatPageSkin(page);
            }

            _announcementPage?.ApplySkin();

            if (!gridUsers.IsDisposed)
            {
                viewUsers.RefreshData();
            }
        }

        /// <summary>Sol sütundaki kullanıcı listesinin sütunlarını kurar.</summary>
        /// <remarks>
        /// Liste bir mesajlaşma uygulaması listesi gibi okunur: solda avatar
        /// (çevrimiçi ise üstünde yeşil nokta), ortada ad ve alt satırda durum,
        /// sağda okunmamış rozeti. Eski "Durum" ve "Son Görülme" sütunları
        /// kaldırıldı; bilgi adın altındaki ikinci satırda yaşar.
        /// </remarks>
        private void ConfigureContactsGrid()
        {
            viewUsers.OptionsBehavior.AutoPopulateColumns = false;
            viewUsers.OptionsView.EnableAppearanceEvenRow = true;
            viewUsers.OptionsView.EnableAppearanceOddRow = true;
            viewUsers.OptionsView.ShowVerticalLines = DefaultBoolean.False;
            viewUsers.OptionsView.ShowHorizontalLines = DefaultBoolean.True;
            viewUsers.RowHeight = 52;

            // Sütun genişlikleri toplamı (330), panelin kullanım alanından (≈378)
            // bilinçli olarak dar tutulur: eşit olsaydı taşma altta yatay kaydırma
            // çubuğu çıkarır ve rozetin kenarı kesilirdi.
            const int AvatarWidth = 50;
            const int NameWidth = 236;
            const int UnreadWidth = 44;

            // Avatar hücresi odaklanamaz: odaklanan hücre DevExpress tarafından
            // düzenleyici ile yeniden boyanır ve elle çizilen avatar kaybolur.
            GridColumn avatar = viewUsers.Columns.AddField(nameof(DirectoryRow.StatusMark));
            avatar.Caption = " ";
            avatar.Width = AvatarWidth;
            avatar.VisibleIndex = 0;
            avatar.OptionsColumn.AllowFocus = false;
            avatar.ToolTip = "Avatarın yeşil noktası kullanıcının şu anda çevrimiçi olduğunu gösterir.";

            GridColumn name = viewUsers.Columns.AddField(nameof(DirectoryRow.FullName));
            name.Caption = "Kullanıcı";
            name.Width = NameWidth;
            name.VisibleIndex = 1;
            name.ToolTip = "Üst satır ad soyad, alt satır çevrimiçi durumu ya da son görülme bilgisidir.";

            GridColumn unread = viewUsers.Columns.AddField(nameof(DirectoryRow.UnreadCount));
            unread.Caption = " ";
            unread.Width = UnreadWidth;
            unread.VisibleIndex = 2;
            unread.OptionsColumn.AllowFocus = false;
            unread.ToolTip = "Bu kullanıcıdan gelen okunmamış mesaj sayısı.";

            // Satır rengini belirlemek için görünmez hesap alanı.
            GridColumn online = viewUsers.Columns.AddField(nameof(DirectoryRow.IsOnline));
            online.Visible = false;

            viewUsers.CustomDrawCell += ViewUsers_CustomDrawCell;
            viewUsers.RowStyle += ViewUsers_RowStyle;
        }

        /// <summary>
        /// Kullanıcı satırındaki avatarı, iki satırlık metni ve okunmamış
        /// rozetini çizer.
        /// </summary>
        /// <remarks>
        /// Hücre arka zemini DevExpress'in olaydan önce boyadığı için satır
        /// rengi (çevrimiçi satırın hafif yeşil zemini) korunur; yalnızca
        /// içerik elle çizilir ve <c>e.Handled = true</c> ile varsayılan
        /// boyama bastırılır.
        /// </remarks>
        private void ViewUsers_CustomDrawCell(object? sender, RowCellCustomDrawEventArgs e)
        {
            if (e.RowHandle < 0 || viewUsers.GetRow(e.RowHandle) is not DirectoryRow row)
            {
                return;
            }

            if (string.Equals(e.Column.FieldName, nameof(DirectoryRow.StatusMark), StringComparison.Ordinal))
            {
                DrawAvatar(e, row);
                e.Handled = true;
            }
            else if (string.Equals(e.Column.FieldName, nameof(DirectoryRow.FullName), StringComparison.Ordinal))
            {
                DrawPresenceLine(e, row);
                e.Handled = true;
            }
            else if (string.Equals(e.Column.FieldName, nameof(DirectoryRow.UnreadCount), StringComparison.Ordinal))
            {
                DrawUnreadBadge(e.Graphics, e.Bounds, row.UnreadCount);
                e.Handled = true;
            }
        }

        /// <summary>
        /// Ad soyadı ile altındaki durum satırını (Çevrimiçi / Son görülme) çizer.
        /// </summary>
        /// <remarks>
        /// Satır zemini DevExpress tarafından boyanmıştır; seçili (odaklı) satırda bu
        /// zemin seçim rengine çevrilir. Yazı renkleri zemine göre seçilir: öyle
        /// olmazsa seçili satırda metin seçim zemininde okunmaz kalır.
        /// </remarks>
        private void DrawPresenceLine(RowCellCustomDrawEventArgs e, DirectoryRow row)
        {
            Rectangle cell = e.Bounds;
            Color surface = SkinTheme.SurfaceOf(gridUsers);

            // Hücrenin gerçek zemini: normalde satır zemin rengi, seçili satırda
            // DevExpress'in seçim rengi gelir.
            Color background = e.Appearance.BackColor == Color.Empty
                ? (_onlineRowColor ?? surface)
                : e.Appearance.BackColor;

            bool selected = gridUsers.Focused
                && viewUsers.FocusedRowHandle == e.RowHandle;

            Color nameColor;
            Color detailColor;

            if (selected)
            {
                // Seçim zemini üzerinde daima okunur zıt metin; durum satırı ana
                // metinden hafif ayrışır ve gerekirse kontrasta çekilir.
                nameColor = SkinTheme.GetContrastText(background);

                detailColor = SkinTheme.EnsureReadable(
                    SkinTheme.Blend(nameColor, background, SkinTheme.IsDarkSkin ? 0.22F : 0.28F),
                    background,
                    3.5);
            }
            else
            {
                nameColor = row.IsOnline
                    ? SkinTheme.Text
                    : (_offlineTextColor ?? SkinTheme.MutedText(surface));

                detailColor = row.IsOnline
                    ? SkinTheme.Success
                    : nameColor;
            }

            var nameRect = new Rectangle(cell.X + 4, cell.Y + 7, cell.Width - 8, 18);

            TextRenderer.DrawText(
                e.Graphics,
                row.FullName,
                RowNameFont,
                nameRect,
                nameColor,
                TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix);

            string detail = row.IsOnline
                ? "Çevrimiçi"
                : $"Son görülme: {row.LastSeen}";

            var detailRect = new Rectangle(cell.X + 4, cell.Y + 27, cell.Width - 8, 16);

            TextRenderer.DrawText(
                e.Graphics,
                detail,
                RowDetailFont,
                detailRect,
                detailColor,
                TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix);
        }

        /// <summary>
        /// Yuvarlak avatarı (varsa profil fotoğrafı, yoksa baş harfleri) ve
        /// (çevrimiçi ise) yeşil durum noktasını çizer.
        /// </summary>
        /// <remarks>
        /// Yeşil nokta avatarın sağ alt köşesindedir ve etrafında satırın
        /// zeminiyle aynı renkte bir halka taşır; böylece renkli bir avatarın
        /// üzerinde de okunur kalır. Halka, seçili satırda seçim zeminiyle
        /// eşleşir.
        /// </remarks>
        private void DrawAvatar(RowCellCustomDrawEventArgs e, DirectoryRow row)
        {
            const int Diameter = 34;

            Rectangle cell = e.Bounds;
            var circle = new Rectangle(
                cell.X + ((cell.Width - Diameter) / 2),
                cell.Y + ((cell.Height - Diameter) / 2),
                Diameter,
                Diameter);

            Image? photo = TryGetAvatarImage(row);

            if (photo is not null)
            {
                // Fotoğraf dairesel maskenin dışına taşmamalı: kayan kare bir
                // görsel avatar hücresinin satıra sığmasını bozar.
                GraphicsState state = e.Graphics.Save();

                using (var mask = new GraphicsPath())
                {
                    mask.AddEllipse(circle);
                    e.Graphics.SetClip(mask);
                    e.Graphics.DrawImage(photo, circle);
                }

                e.Graphics.Restore(state);
            }
            else
            {
                Color fill = AvatarColorFor(row.FullName);

                using (var brush = new SolidBrush(fill))
                {
                    e.Graphics.FillEllipse(brush, circle);
                }

                TextRenderer.DrawText(
                    e.Graphics,
                    BuildInitials(row.FullName),
                    InitialsFont,
                    circle,
                    SkinTheme.GetContrastText(fill),
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPrefix);
            }

            if (!row.IsOnline)
            {
                return;
            }

            const int DotDiameter = 11;
            var dot = new Rectangle(
                circle.Right - DotDiameter - 1,
                circle.Bottom - DotDiameter - 1,
                DotDiameter,
                DotDiameter);

            Color ring = e.Appearance.BackColor == Color.Empty
                ? (_onlineRowColor ?? SkinTheme.SurfaceOf(gridUsers))
                : e.Appearance.BackColor;

            using (var ringBrush = new SolidBrush(ring))
            {
                e.Graphics.FillEllipse(
                    ringBrush,
                    new Rectangle(dot.X - 2, dot.Y - 2, DotDiameter + 4, DotDiameter + 4));
            }

            using (var dotBrush = new SolidBrush(SkinTheme.Success))
            {
                e.Graphics.FillEllipse(dotBrush, dot);
            }
        }

        /// <summary>
        /// Kullanıcının profil fotoğrafını verir; fotoğraf yoksa ya da
        /// okunamadıysa <c>null</c> döner ve baş harf avatarı çizilir.
        /// </summary>
        /// <remarks>
        /// Sonuç (olumlu da olumsuz da) önbelleğe alınır: bulunmayan bir
        /// dosyanın her boyamada diske sorulması, listede kaydırmayı
        /// hissedilir ölçüde yavaşlatırdı.
        /// </remarks>
        private Image? TryGetAvatarImage(DirectoryRow row)
        {
            string? path = row.AvatarPath;

            if (string.IsNullOrWhiteSpace(path))
            {
                return null;
            }

            lock (_avatarCacheLock)
            {
                if (_avatarCache.TryGetValue(row.UserId, out (string Path, Image? Image) entry)
                    && string.Equals(entry.Path, path, StringComparison.OrdinalIgnoreCase))
                {
                    return entry.Image;
                }

                // Kullanıcı fotoğrafını değiştirmiş: eski görsel bırakılır.
                entry.Image?.Dispose();

                Image? loaded = LoadAvatar(path);
                _avatarCache[row.UserId] = (path, loaded);

                return loaded;
            }
        }

        /// <summary>Depodaki fotoğraf dosyasını okur; başarısız olursa <c>null</c> döner.</summary>
        private static Image? LoadAvatar(string relativePath)
        {
            try
            {
                string fullPath = StorageRoot.Resolve(relativePath);

                if (!File.Exists(fullPath))
                {
                    return null;
                }

                using var stream = new MemoryStream();
                using (var file = new FileStream(fullPath, FileMode.Open, FileAccess.Read))
                {
                    file.CopyTo(stream);
                }

                return new Bitmap(stream);
            }
            catch (Exception ex)
            {
                // Kırık bir dosya listeyi boyamayı durdurmamalı; yalnızca
                // baş harf avatarına düşülür.
                CrashLog.WriteException("Message.Avatar", ex);
                return null;
            }
        }

        /// <summary>
        /// Çevrimiçi ve pasif satırları birbirinden ayırır.
        /// </summary>
        private void ViewUsers_RowStyle(object? sender, RowStyleEventArgs e)
        {
            if (e.RowHandle < 0 || viewUsers.GetRow(e.RowHandle) is not DirectoryRow row)
            {
                return;
            }

            if (row.IsOnline)
            {
                if (_onlineRowColor is { } online)
                {
                    e.Appearance.BackColor = online;
                    e.Appearance.Options.UseBackColor = true;
                }

                return;
            }

            // Pasif satırlar soluklaştırılır: listede öne çıkanlar, o an
            // ulaşılabilen kullanıcılar olmalıdır.
            if (_offlineTextColor is { } muted)
            {
                e.Appearance.ForeColor = muted;
                e.Appearance.Options.UseForeColor = true;
            }
        }

        

        /// <summary>
        /// Okunmamış sayısını yuvarlak rozet olarak çizer; sayı yoksa
        /// hücre boş bırakılır.
        /// </summary>
        private static void DrawUnreadBadge(Graphics graphics, Rectangle cell, int count)
        {
            if (count <= 0)
            {
                return;
            }

            const int Diameter = 22;
            var circle = new Rectangle(
                cell.X + ((cell.Width - Diameter) / 2),
                cell.Y + ((cell.Height - Diameter) / 2),
                Diameter,
                Diameter);

            Color accent = SkinTheme.Primary;

            using (var brush = new SolidBrush(accent))
            {
                graphics.FillEllipse(brush, circle);
            }

            TextRenderer.DrawText(
                graphics,
                count > 99 ? "99+" : count.ToString(),
                BadgeFont,
                circle,
                SkinTheme.GetContrastText(accent),
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPrefix);
        }

        /// <summary>
        /// Ad soyaddan üretilen avatarın zemin rengini seçer: aynı ad her
        /// zaman aynı rengi alır.
        /// </summary>
        private static Color AvatarColorFor(string fullName)
        {
            int hash = 0;

            foreach (char c in fullName)
            {
                hash = unchecked((hash * 31) + c);
            }

            int index = ((hash % AvatarPalette.Length) + AvatarPalette.Length) % AvatarPalette.Length;
            return AvatarPalette[index];
        }

        /// <summary>Ad soyaddan iki harflik baş harf üretir ("Ayşe Yılmaz" → "AY").</summary>
        private static string BuildInitials(string fullName)
        {
            string[] parts = fullName.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            if (parts.Length == 0)
            {
                return "?";
            }

            if (parts.Length == 1)
            {
                string first = parts[0];
                return UpperTr(first[0]) + (first.Length > 1 ? UpperTr(first[1]) : string.Empty);
            }

            return $"{UpperTr(parts[0][0])}{UpperTr(parts[^1][0])}";
        }

        /// <summary>Türkçe büyük harf: "i" harfi "İ" olur ("İsmail" → "İY").</summary>
        private static string UpperTr(char value)
        {
            return value.ToString().ToUpper(CultureInfo.GetCultureInfo("tr-TR"));
        }

        // ----------------------------------------------------------------
        // Veri yükleme
        // ----------------------------------------------------------------

        private async Task ReloadAsync(bool showLoading)
        {
            if (_reloading)
            {
                return;
            }

            _reloading = true;

            try
            {
                if (showLoading)
                {
                    await LoadingHelper.RunAsync(
                        LoadAsync,
                        caption: "Mesajlar yükleniyor...",
                        description: "Lütfen bekleyin...");
                }
                else
                {
                    await LoadAsync();
                }
            }
            finally
            {
                _reloading = false;
            }
        }

        /// <summary>
        /// Kullanıcı listesini tazeler. Sohbet alanı ayrıca tazelenir (açık).
        /// </summary>
        private async Task LoadAsync()
        {
            using var scope = Program.Services.CreateScope();
            ISender mediator = scope.ServiceProvider.GetRequiredService<ISender>();

            Result<List<MessageDirectoryDto>> directory = await mediator.Send(
                new MessageDirectoryQuery(), CancellationToken.None);

            _allContacts = (directory.Data ?? [])
                .Select(u => new DirectoryRow(u))
                .ToList();

            ApplyContactFilter();
            UpdateHeader();

            // Duyuru yetkisi olmayan kullanıcıda düğme hiç gösterilmez.
            btnAnnouncement.Visible = await CurrentUserPermissions.HasAsync(MessagePermissions.Announce);

            // Bildirimden gelen konuşma ilk yüklemede açılamadıysa (liste henüz
            // dolmamışken tıklanmıştı) şimdi hedef kullanıcıya bir sekmeyle açılır.
            if (_pendingNotificationConversation is { } pending
                && _allContacts.FirstOrDefault(r => r.UserId == pending.Id) is { } pendingRow)
            {
                _pendingNotificationConversation = null;

                await OpenChatAsync(pendingRow);
            }
        }

        /// <summary>Arama kutusuna göre kullanıcı listesini süzer.</summary>
        private void ApplyContactFilter()
        {
            string term = _contactSearch.Trim();

            List<DirectoryRow> visible = term.Length == 0
                ? _allContacts
                : _allContacts.Where(row => Matches(row, term)).ToList();

            // Anahtar hem kimlikleri hem çevrimiçi durumu taşır: durum değişimi
            // sırada da değişiklik yaptığı için kaynak yeniden atanır. Kimlik ve
            // durum aynı kaldıysa ekranda eski satır nesneleri kalır; bu nesneler
            // yeni veriyle tazelenir ve yalnızca yeniden çizilir. Aksi hâlde
            // on saniyede bir gelen durum bilgisi eski nesnelerde kalır ve liste
            // başlıktaki "1 çevrimiçi" yazısına rağmen yeşil nokta göstermez.
            bool rebound = ApplyWhenChanged(
                gridUsers, ref _contactsKey, visible, ContactsKey);

            if (!rebound && gridUsers.DataSource is List<DirectoryRow> bound)
            {
                // Kimlik ve durum aynıysa sıralar da bire bir karşılıklıdır:
                // yeni satırlardaki alanlar eski nesnelere kopyalanır.
                for (int i = 0; i < bound.Count && i < visible.Count; i++)
                {
                    bound[i].Update(visible[i].User);
                }

                viewUsers.RefreshData();
            }

            lblContactsCount.Text = term.Length == 0
                ? $"{_allContacts.Count(r => r.IsOnline)} çevrimiçi / {_allContacts.Count} kullanıcı"
                : $"{visible.Count} / {_allContacts.Count}";
        }

        /// <summary>Kullanıcı satırının tazeleme anahtarı: kimlik ve çevrimiçi durum.</summary>
        private static string ContactsKey(DirectoryRow row)
            => $"{row.UserId:N}|{row.IsOnline}";

        /// <summary>
        /// Satır kümesi değiştiyse listeyi tazeler ve <c>true</c> döndürür.
        /// </summary>
        /// <remarks>
        /// Anahtar yalnızca kimlikleri değil, satırda görünen değişen alanları
        /// da içermelidir. Sıra da hesaba katılır: çevrimiçi kullanıcı listeye
        /// girip çıktığında ya da son mesaj değiştiğinde kimlik dizisi de
        /// değişir ve yeniden sıralama yapılır. Hiçbir değişiklik yoksa kaynak
        /// yeniden bağlanmaz; böylece kaydırma konumu ve imleç korunur.
        /// </remarks>
        private static bool ApplyWhenChanged<T>(
            GridControl grid,
            ref string appliedKey,
            List<T> rows,
            Func<T, string> keyOf)
        {
            string key = string.Join('|', rows.Select(keyOf));

            if (string.Equals(key, appliedKey, StringComparison.Ordinal))
            {
                return false;
            }

            grid.DataSource = rows;
            appliedKey = key;

            return true;
        }

        private static bool Matches(DirectoryRow row, string term)
        {
            return Contains(row.User.FullName, term)
                || Contains(row.UserName, term)
                || Contains(row.RegistryNumber, term)
                || Contains(row.User.TcNo, term)
                || Contains(row.CompanyName, term)
                || Contains(row.RoleName, term);
        }

        /// <summary>
        /// Büyük/küçük harf ve Türkçe harf farkına duyarsız arama.
        /// </summary>
        private static bool Contains(string? source, string term)
            => !string.IsNullOrWhiteSpace(source)
                && source.Contains(term, StringComparison.CurrentCultureIgnoreCase);

        private void UpdateHeader()
        {
            int online = _allContacts.Count(c => c.IsOnline);

            lblSubtitle.Text = $"{online} kullanıcı çevrimiçi";
        }

        private void TxtContactSearch_TextChanged(object? sender, EventArgs e)
        {
            _contactSearch = txtContactSearch.Text;

            ApplyContactFilter();
        }

        // ----------------------------------------------------------------
        // Konuşma sekmesi açma
        // ----------------------------------------------------------------

        private async void ViewUsers_RowClick(object? sender, RowClickEventArgs e)
        {
            if (e.Clicks != 1
                || e.RowHandle < 0
                || viewUsers.GetRow(e.RowHandle) is not DirectoryRow row)
            {
                return;
            }

            await OpenChatAsync(row);
        }

        /// <summary>
        /// Klavyede <b>Enter</b> ile seçili kullanıcıyla sohbet açılır.
        /// </summary>
        private async void ViewUsers_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Enter)
            {
                return;
            }

            e.Handled = true;
            e.SuppressKeyPress = true;

            if (GetSelectedContact() is { } row)
            {
                await OpenChatAsync(row);
            }
        }

        /// <summary>Kullanıcı listesinde seçili olan satır.</summary>
        private DirectoryRow? GetSelectedContact()
        {
            return viewUsers.GetFocusedRow() as DirectoryRow;
        }

        /// <summary>
        /// Konuşma sekmesi başlığında gösterilecek çevrimiçi durum metni.
        /// </summary>
        private static string BuildPresenceCaption(DirectoryRow row)
        {
            return row.IsOnline
                ? "Çevrimiçi"
                : $"Son görülme: {row.LastSeen}";
        }

        /// <summary>
        /// Sağ alanda seçili kullanıcıyla sohbeti açar (yeni sekme) ya da zaten
        /// açıksa o sekmeye geçer.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Sohbetler mesaj listesi ekranının sağ yarısındaki sekmelerde açılır;
        /// her konuşma ayrı bir sekmedir ve biri açıkken diğeri kapanmaz. Sekme
        /// başlıkları üstte yan yana durur; taşarsa alt satıra sarar.
        /// </para>
        /// <para>
        /// Okunma işareti yalnızca o sekme ekranda gerçekten görüldüğünde
        /// (seçiliyken) yazılır; arka plandaki (görünmeyen) sekmeler teslim
        /// edilmiş sayılır ama okundu yazmaz.
        /// </para>
        /// </remarks>
        private async Task OpenChatAsync(DirectoryRow row)
        {
            await OpenChatAsync(row.UserId, row.FullName, BuildPresenceCaption(row));
        }

        private async Task OpenChatAsync(Guid counterpartId, string fullName, string? presenceCaption)
        {
            if (_chatPages.TryGetValue(counterpartId, out InlineChatPage? existing))
            {
                _activeChatId = counterpartId;
                tabConversations.SelectedTabPage = existing.Page;
                existing.txtReply.Focus();
                return;
            }

            InlineChatPage page = CreateChatPage(counterpartId, fullName, presenceCaption);

            _chatPages[counterpartId] = page;
            _activeChatId = counterpartId;

            // Sekme eklenir/seçilirken SelectedPageChanged aynı karşı taraf için
            // zaten _activeChatId aynı olduğundan geri döner; mesaj yüklemesi
            // burada bir kez yapılır.
            tabConversations.TabPages.Add(page.Page);
            tabConversations.SelectedTabPage = page.Page;

            lblEmpty.Visible = false;
            tabConversations.Visible = true;

            await LoadChatPageAsync(page, markRead: true);

            page.txtReply.Focus();

            await ReloadAsync(showLoading: false);
        }

        /// <summary>
        /// Sekme değiştiğinde takip edilen karşı tarafı günceller ve yeni seçilen
        /// sekme ekranda görüldüğü için onu tazeler (teslim + okunma).
        /// </summary>
        private async void TabConversations_SelectedPageChanged(object? sender, TabPageChangedEventArgs e)
        {
            Guid? next = (tabConversations.SelectedTabPage?.Tag as InlineChatPage)?.CounterpartId;

            if (next == _activeChatId)
            {
                return;
            }

            _activeChatId = next;

            if (next is { } id && _chatPages.TryGetValue(id, out InlineChatPage? page))
            {
                await LoadChatPageAsync(page, markRead: true);
                page.txtReply.Focus();
            }

            // Duyuru sekmesi de bu alanda yaşadığı için boş yer tutucu yalnızca
            // ne bir konuşma ne de duyuru yazımı açıkken gösterilir.
            if (_chatPages.Count == 0 && _announcementPage is null)
            {
                lblEmpty.Visible = true;
                tabConversations.Visible = false;
            }
        }

        /// <summary>
        /// Bir konuşmanın sekme sayfasını kurar: başlık, baloncuk alanı ve yazı
        /// çubuğu. Her kullanıcı için ayrı sayfa üretilir; böylece konuşmalar
        /// aynı anda yan yana açık kalabilir.
        /// </summary>
        private InlineChatPage CreateChatPage(Guid counterpartId, string fullName, string? presenceCaption)
        {
            InlineChatPage page = new()
            {
                CounterpartId = counterpartId,
                FullName = fullName
            };

            page.pnlHeader = new PanelControl
            {
                Dock = DockStyle.Top,
                Height = 54,
                Padding = new Padding(14, 8, 14, 8)
            };

            page.pnlText = new Panel { Dock = DockStyle.Fill };

            page.lblTitle = new LabelControl
            {
                AutoSizeMode = LabelAutoSizeMode.None,
                Dock = DockStyle.Top,
                Height = 22,
                Text = fullName
            };

            page.lblDetail = new LabelControl
            {
                AutoSizeMode = LabelAutoSizeMode.None,
                Dock = DockStyle.Top,
                Height = 16,
                Text = presenceCaption ?? string.Empty
            };

            page.btnClose = new SimpleButton
            {
                Dock = DockStyle.Right,
                Width = 34,
                Text = "✕",
                ToolTip = "Sekmeyi kapat"
            };

            // Düğmeler sağ kenara dizilir; ekleme sırası sağdan sola doğrudur
            // (son eklenen en sağda). Kapat en sağda: [Geçmişi Göster][Temizle][✕].
            page.btnHistory = new SimpleButton
            {
                Dock = DockStyle.Right,
                Width = 104,
                Text = "Geçmişi Göster",
                Visible = false,
                ToolTip = "Daha eski mesajları yükler"
            };

            page.btnClear = new SimpleButton
            {
                Dock = DockStyle.Right,
                Width = 62,
                Text = "Temizle",
                ToolTip = "Sohbeti yalnızca sizin tarafınızda temizler; karşı tarafın kopyası kalır"
            };

            page.pnlChat = new ChatBubblePanel { Dock = DockStyle.Fill };

            page.pnlCompose = new PanelControl
            {
                Dock = DockStyle.Bottom,
                Height = 84,
                Padding = new Padding(14, 10, 14, 10)
            };

            page.txtReply = new MemoEdit { Dock = DockStyle.Fill };

            page.txtReply.Properties.Appearance.Font = new Font("Segoe UI", 10F);
            page.txtReply.Properties.Appearance.Options.UseFont = true;
            page.txtReply.Properties.NullText = "Mesajınızı yazın... (Enter gönderir, Shift+Enter satır atlar)";
            page.txtReply.Properties.Padding = new Padding(6, 6, 6, 6);

            // Seçilen dosya gönderilmeyi beklerken çip satırında görünür;
            // tıklanınca seçim kaldırılır. Çip görünmezken yer kaplamaz.
            page.lblAttachment = new LabelControl
            {
                Dock = DockStyle.Top,
                Height = 20,
                AutoSizeMode = LabelAutoSizeMode.None,
                Visible = false,
                Cursor = Cursors.Hand,
                Text = string.Empty,
                ToolTip = "Kaldırmak için tıklayın"
            };

            page.btnAttach = new SimpleButton
            {
                Dock = DockStyle.Left,
                Width = 110,
                Text = "Dosya Ekle",
                ToolTip = "Mesaja dosya ekle (en fazla 25 MB)"
            };

            page.btnAttach.ImageOptions.Image = Properties.Resources.base_paperclip_32;
            page.btnAttach.ImageOptions.ImageToTextAlignment = ImageAlignToText.LeftCenter;

            page.btnSend = new SimpleButton
            {
                Dock = DockStyle.Right,
                Width = 100,
                Text = "Gönder"
            };
            page.btnSend.ImageOptions.Image = Properties.Resources.base_paperclip_32;
            page.btnSend.ImageOptions.ImageToTextAlignment = ImageAlignToText.LeftCenter;

            page.lblTitle.Appearance.Font = new Font("Segoe UI", 11.5F, FontStyle.Bold);
            page.lblTitle.Appearance.Options.UseFont = true;
            page.lblDetail.Appearance.Font = new Font("Segoe UI", 8.5F);
            page.lblDetail.Appearance.Options.UseFont = true;
            page.btnSend.Appearance.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            page.btnSend.Appearance.Options.UseFont = true;
            page.btnHistory.Appearance.Font = new Font("Segoe UI", 8.5F);
            page.btnHistory.Appearance.Options.UseFont = true;
            page.btnClear.Appearance.Font = new Font("Segoe UI", 8.5F);
            page.btnClear.Appearance.Options.UseFont = true;
            page.btnAttach.Appearance.Font = new Font("Segoe UI", 9F);
            page.btnAttach.Appearance.Options.UseFont = true;
            page.lblAttachment.Appearance.Font = new Font("Segoe UI", 8.5F);
            page.lblAttachment.Appearance.Options.UseFont = true;

            page.Page = new XtraTabPage { Text = fullName, Tag = page };

            // Başlık: sağda [Geçmişi Göster][Temizle][✕], kalan alanda ad ve durum.
            page.pnlHeader.Controls.Add(page.pnlText);
            page.pnlText.Controls.Add(page.lblDetail);
            page.pnlText.Controls.Add(page.lblTitle);
            page.pnlHeader.Controls.Add(page.btnHistory);
            page.pnlHeader.Controls.Add(page.btnClear);
            page.pnlHeader.Controls.Add(page.btnClose);

            // Ekleme sırası tersinden işlenir: Gönder en sağda, sonra Ekle,
            // sonra çip satırı (üstte), en altta yazı kutusu kalır.
            page.pnlCompose.Controls.Add(page.txtReply);
            page.pnlCompose.Controls.Add(page.lblAttachment);
            page.pnlCompose.Controls.Add(page.btnAttach);
            page.pnlCompose.Controls.Add(page.btnSend);

            page.Page.Controls.Add(page.pnlChat);
            page.Page.Controls.Add(page.pnlHeader);
            page.Page.Controls.Add(page.pnlCompose);

            page.btnSend.Click += async (_, _) => await SendAsync(page);
            page.btnClose.Click += (_, _) => CloseChatPage(page);
            page.txtReply.KeyDown += (sender, e) => OnReplyKeyDown(page, sender, e);
            page.btnAttach.Click += (_, _) => PickAttachment(page);
            page.lblAttachment.Click += (_, _) => ClearPendingAttachment(page);
            page.btnHistory.Click += async (_, _) => await HistoryButtonClickAsync(page);
            page.btnClear.Click += async (_, _) => await ClearChatAsync(page);
            page.pnlChat.AttachmentOpenRequested += OpenAttachment;

            ApplyChatPageSkin(page);

            return page;
        }

        private void ApplyChatPageSkin(InlineChatPage page)
        {
            Color surface = SkinTheme.SurfaceOf(this);
            Color primary = SkinTheme.Text;
            Color panelLabel = SkinTheme.Blend(primary, surface, 0.10F);

            page.pnlHeader.Appearance.BackColor = SkinTheme.SurfaceMuted(surface);
            page.pnlHeader.Appearance.Options.UseBackColor = true;

            page.pnlText.BackColor = SkinTheme.SurfaceMuted(surface);

            page.lblTitle.Appearance.ForeColor = panelLabel;
            page.lblTitle.Appearance.Options.UseForeColor = true;
            page.lblDetail.Appearance.ForeColor = SkinTheme.MutedText(surface);
            page.lblDetail.Appearance.Options.UseForeColor = true;

            page.pnlCompose.Appearance.BackColor = surface;
            page.pnlCompose.Appearance.Options.UseBackColor = true;

            // Gönder düğmesi sohbet alanının tek aksanıdır: birincil renkle
            // vurgulanır, metni zemin üzerinde okunur seçilir.
            Color accent = SkinTheme.Primary;

            page.btnSend.Appearance.BackColor = accent;
            page.btnSend.Appearance.Options.UseBackColor = true;
            page.btnSend.Appearance.ForeColor = SkinTheme.GetContrastText(accent);
            page.btnSend.Appearance.Options.UseForeColor = true;

            // Başlık ve yazı çubuğundaki ikincil düğmeler aynı soluk görünümü
            // paylaşır; biri diğerinden bağımsız renklenirse başlık karışık
            // görünür.
            foreach (SimpleButton button in new[] { page.btnClose, page.btnClear, page.btnHistory, page.btnAttach })
            {
                button.Appearance.BackColor = SkinTheme.SurfaceMuted(surface);
                button.Appearance.Options.UseBackColor = true;
                button.Appearance.ForeColor = SkinTheme.MutedText(surface);
                button.Appearance.Options.UseForeColor = true;
            }

            page.lblAttachment.Appearance.ForeColor = SkinTheme.MutedText(surface);
            page.lblAttachment.Appearance.Options.UseForeColor = true;

            page.pnlChat.BackColor = SkinTheme.SurfaceOf(this);
            page.pnlChat.Invalidate();
        }

        /// <summary>
        /// Bir sekmenin mesajlarını getirir, baloncuk alanına yerleştirir ve
        /// durum işaretler (teslim her zaman; okunma yalnızca sekme ekranda
        /// görüntüleniyorsa).
        /// </summary>
        /// <remarks>
        /// <para>
        /// Son <see cref="InitialHistoryPageSize"/> mesaj (ya da sayfada
        /// birikmiş olan daha geniş pencere) getirilir ve daha önce yüklenmiş
        /// eski mesajlarla birleştirilir; böylece on saniyelik tazeleme
        /// sayfalamayı geri almaz.
        /// </para>
        /// <para>
        /// Temizleme durumu yalnızca sekme ilk açılırken sunucudan okunur;
        /// kullanıcı temizleyip geri aldığında durum sayfa belleğinde yaşar.
        /// </para>
        /// </remarks>
        private async Task LoadChatPageAsync(InlineChatPage page, bool markRead)
        {
            using var scope = Program.Services.CreateScope();
            ISender mediator = scope.ServiceProvider.GetRequiredService<ISender>();

            if (!page.StateLoaded)
            {
                Result<DateTimeOffset?> state = await mediator.Send(
                    new MessageConversationStateQuery(page.CounterpartId),
                    CancellationToken.None);

                if (state.IsSuccessful)
                {
                    page.ClearedAt = state.Data;
                    page.StateLoaded = true;
                }
            }

            int window = Math.Max(InitialHistoryPageSize, page.LoadedMessages.Count);

            Result<List<MessageDto>> result = await mediator.Send(
                new MessageConversationQuery(
                    page.CounterpartId,
                    AnnouncementChannel: false,
                    Limit: window),
                CancellationToken.None);

            if (!result.IsSuccessful || result.Data is null)
            {
                return;
            }

            page.LoadedMessages = MergeMessages(page.LoadedMessages, result.Data);

            // Temizlenmişse eski mesajlar sunucu tarafından zaten elenir; pencere
            // dolu değilse de eski mesaj kalmadığı kabul edilir.
            page.AllHistoryLoaded = page.ClearedAt is not null || result.Data.Count < window;

            page.pnlChat.SetMessages(page.LoadedMessages);
            UpdateHistoryUi(page);

            // Sekme uygulamada açık olduğu için teslim işaretlenir.
            await mediator.Send(
                new MessageMarkConversationAsDeliveredCommand(page.CounterpartId),
                CancellationToken.None);

            // Okunma yalnızca ekranda görünen sekmeye yazılır; arka planda
            // duran sekme okundu yazmaz.
            if (markRead)
            {
                await mediator.Send(
                    new MessageMarkConversationAsReadCommand(page.CounterpartId),
                    CancellationToken.None);
            }
        }

        /// <summary>
        /// Sunucudan gelen pencere ile sayfada biriken mesajları kimliğe göre
        /// birleştirir, eskiden yeniye dizer. Taze satır (okundu/teslim bilgisi
        /// güncellenmiş) eskiyi ezer.
        /// </summary>
        private static List<MessageDto> MergeMessages(
            List<MessageDto> existing,
            List<MessageDto> fetched)
        {
            if (existing.Count == 0)
            {
                return fetched.OrderBy(m => m.SentAt).ToList();
            }

            if (fetched.Count == 0)
            {
                return existing;
            }

            var byId = new Dictionary<Guid, MessageDto>(existing.Count + fetched.Count);

            foreach (MessageDto message in existing)
            {
                byId[message.Id] = message;
            }

            foreach (MessageDto message in fetched)
            {
                byId[message.Id] = message;
            }

            return byId.Values.OrderBy(m => m.SentAt).ToList();
        }

        /// <summary>
        /// Başlık düğmelerinin durumunu günceller: temizlenmişse "Geçmişi
        /// Göster" geri yükleme yapar, değilse eski mesaj varsa görünür.
        /// </summary>
        private static void UpdateHistoryUi(InlineChatPage page)
        {
            bool restoreMode = page.ClearedAt is not null;
            bool showHistory = restoreMode || !page.AllHistoryLoaded;

            page.btnHistory.Visible = showHistory;
            page.btnHistory.Enabled = showHistory;
            page.btnHistory.ToolTip = restoreMode
                ? "Temizlenmiş geçmişi yeniden gösterir"
                : "Daha eski mesajları yükler";

            page.btnClear.Enabled = page.LoadedMessages.Count > 0;
        }

        /// <summary>"Geçmişi Göster" düğmesi: temizlendiyse geri yükler, değilse eskilerini yükler.</summary>
        private async Task HistoryButtonClickAsync(InlineChatPage page)
        {
            if (page.ClearedAt is not null)
            {
                await RestoreChatAsync(page);
                return;
            }

            await LoadOlderMessagesAsync(page);
        }

        /// <summary>
        /// Sayfadaki en eski mesajdan geriye doğru bir sayfa daha yükler;
        /// üstten eklendiği için okuma konumu ekran üzerinde korunur.
        /// </summary>
        private async Task LoadOlderMessagesAsync(InlineChatPage page)
        {
            if (page.AllHistoryLoaded || page.LoadedMessages.Count == 0)
            {
                return;
            }

            page.btnHistory.Enabled = false;

            try
            {
                using var scope = Program.Services.CreateScope();
                ISender mediator = scope.ServiceProvider.GetRequiredService<ISender>();

                DateTimeOffset before = page.LoadedMessages[0].SentAt;

                Result<List<MessageDto>> older = await mediator.Send(
                    new MessageConversationQuery(
                        page.CounterpartId,
                        AnnouncementChannel: false,
                        Limit: OlderHistoryPageSize,
                        Before: before),
                    CancellationToken.None);

                if (!older.IsSuccessful || older.Data is null)
                {
                    return;
                }

                List<MessageDto> merged = MergeMessages(page.LoadedMessages, older.Data);
                int added = merged.Count - page.LoadedMessages.Count;

                page.LoadedMessages = merged;

                if (older.Data.Count < OlderHistoryPageSize)
                {
                    page.AllHistoryLoaded = true;
                }

                if (added > 0)
                {
                    page.pnlChat.SetMessages(page.LoadedMessages, preserveTopAnchor: true);
                }

                UpdateHistoryUi(page);
            }
            catch (Exception ex)
            {
                CrashLog.WriteException("Message.History", ex);
                ToastHelper.Show("Eski mesajlar yüklenemedi: " + ex.Message, ToastType.Error, 5000);
            }
            finally
            {
                page.btnHistory.Enabled = true;
            }
        }

        /// <summary>
        /// Sohbeti yalnızca bu kullanıcının görünümünden temizler. Mesajlar ve
        /// karşı tarafın kopyası etkilenmez; "Geçmişi Göster" geri yükler.
        /// </summary>
        private async Task ClearChatAsync(InlineChatPage page)
        {
            if (page.LoadedMessages.Count == 0)
            {
                return;
            }

            DialogResult choice = MsgBox.Confirm(
                this,
                "Sohbet yalnızca sizin görünümünüzden temizlenecek.\n\n"
                + "Mesajların kendisi silinmez ve karşı tarafın kopyası etkilenmez; "
                + "\"Geçmişi Göster\" düğmesiyle geri yükleyebilirsiniz.\n\nDevam edilsin mi?",
                "Sohbeti Temizle");

            if (choice != DialogResult.Yes)
            {
                return;
            }

            try
            {
                using var scope = Program.Services.CreateScope();
                ISender mediator = scope.ServiceProvider.GetRequiredService<ISender>();

                Result<DateTimeOffset?> result = await mediator.Send(
                    new MessageSetClearedCommand(page.CounterpartId, Cleared: true),
                    CancellationToken.None);

                if (!result.IsSuccessful)
                {
                    ToastHelper.Show(
                        AuthFormStyles.GetErrorText(result.ErrorMessages), ToastType.Error, 5000);
                    return;
                }

                // Sunucu saati esas alınır: konuşma sorgusunun filtresi de aynı
                // değeri kullandığı için saat kayması mesaj saklamaz/açmaz.
                page.ClearedAt = result.Data;
                page.StateLoaded = true;
                page.LoadedMessages = [];
                page.AllHistoryLoaded = false;

                page.pnlChat.SetMessages(page.LoadedMessages);
                UpdateHistoryUi(page);

                ToastHelper.Show("Sohbet temizlendi (yalnızca sizin görünümünüz)", ToastType.Success, 2500);
            }
            catch (Exception ex)
            {
                CrashLog.WriteException("Message.Clear", ex);
                ToastHelper.Show("Sohbet temizlenemedi: " + ex.Message, ToastType.Error, 5000);
            }
        }

        /// <summary>Temizlemeyi geri alır ve geçmişi yeniden yükler.</summary>
        private async Task RestoreChatAsync(InlineChatPage page)
        {
            try
            {
                using var scope = Program.Services.CreateScope();
                ISender mediator = scope.ServiceProvider.GetRequiredService<ISender>();

                Result<DateTimeOffset?> result = await mediator.Send(
                    new MessageSetClearedCommand(page.CounterpartId, Cleared: false),
                    CancellationToken.None);

                if (!result.IsSuccessful)
                {
                    ToastHelper.Show(
                        AuthFormStyles.GetErrorText(result.ErrorMessages), ToastType.Error, 5000);
                    return;
                }

                page.ClearedAt = null;
                page.StateLoaded = true;
                page.LoadedMessages = [];
                page.AllHistoryLoaded = false;

                await LoadChatPageAsync(page, markRead: page.CounterpartId == _activeChatId);

                ToastHelper.Show("Geçmiş yeniden gösteriliyor", ToastType.Success, 2000);
            }
            catch (Exception ex)
            {
                CrashLog.WriteException("Message.Restore", ex);
                ToastHelper.Show("Geçmiş yüklenemedi: " + ex.Message, ToastType.Error, 5000);
            }
        }

        /// <summary>
        /// Açık sohbet sekmelerinin mesajlarını yoklamayla sessizce tazeler;
        /// okunma yalnızca o an görüntülenen sekmeye işaretlenir.
        /// </summary>
        private async Task RefreshChatPagesAsync()
        {
            if (_chatReloading || _chatPages.Count == 0)
            {
                return;
            }

            _chatReloading = true;

            try
            {
                foreach (InlineChatPage page in _chatPages.Values.ToList())
                {
                    await LoadChatPageAsync(page, markRead: page.CounterpartId == _activeChatId);
                }
            }
            finally
            {
                _chatReloading = false;
            }
        }

        /// <summary>
        /// "X size mesaj gönderdi" bildirimine basıldığında bu sayfayı açar ve o
        /// konuşmayı sağ alanda bir sekmeyle başlatır. Liste henüz yüklenmediyse
        /// ilk yüklemenin sonunda açılması için hedef sıraya alınır.
        /// </summary>
        public void OpenConversationFromNotification(Guid counterpartId, string counterpartFullName)
        {
            if (_allContacts.FirstOrDefault(r => r.UserId == counterpartId) is { } row)
            {
                _ = OpenChatAsync(row);
                return;
            }

            _pendingNotificationConversation = (counterpartId, counterpartFullName);
        }

        /// <summary>Belirli bir konuşma sekmesini kapatır.</summary>
        private void CloseChatPage(InlineChatPage page)
        {
            _chatPages.Remove(page.CounterpartId);

            tabConversations.TabPages.Remove(page.Page);
            page.Page.Dispose();

            if (_chatPages.Count == 0)
            {
                _activeChatId = null;

                // Duyuru yazma sayfası hâlâ açıksa alan boşaltılmaz.
                if (_announcementPage is null)
                {
                    lblEmpty.Visible = true;
                    tabConversations.Visible = false;
                }

                return;
            }

            // Kapatılan sekme seçiliyse seçim otomatik başka bir sekmeye geçer;
            // takip edilen karşı taraf o sekmeye güncellenir.
            if (_activeChatId == page.CounterpartId)
            {
                _activeChatId = (tabConversations.SelectedTabPage?.Tag as InlineChatPage)?.CounterpartId;
            }
        }

        /// <summary>
        /// Açıktan bir yanıt gönderir: metin ve/veya bekleyen ek dosya.
        /// </summary>
        /// <remarks>
        /// Metin kutusu hiç dokunulmadıysa <c>Text</c> ipucu yazısını
        /// (NullText) döndürür; <see cref="EditText"/> bunu boş sayar. Aksi
        /// hâlde "Mesajınızı yazın..." ipucu gerçek mesaj olarak gönderilirdi.
        /// </remarks>
        private async Task SendAsync(InlineChatPage page)
        {
            string body = EditText(page.txtReply);
            (string FileName, byte[] Data)? attachment = page.PendingAttachment;

            if (body.Length == 0 && attachment is null)
            {
                ToastHelper.Show("Mesaj metni boş olamaz.", ToastType.Warning);
                page.txtReply.Focus();
                return;
            }

            if (attachment is { Data: { } data } && data.Length > MaxAttachmentBytes)
            {
                ToastHelper.Show("Dosya 25 MB sınırını aşıyor.", ToastType.Warning, 4000);
                return;
            }

            page.btnSend.Enabled = false;

            try
            {
                using var scope = Program.Services.CreateScope();
                ISender mediator = scope.ServiceProvider.GetRequiredService<ISender>();

                MessageAttachmentInput? input = attachment is { } pending
                    ? new MessageAttachmentInput(pending.FileName, pending.Data)
                    : null;

                // Konu alanı bilinçli olarak yok: sohbette tek metin kutusu
                // bulunur; geçmişteki başlık mesajın içinde taşınır.
                Result<string> result = await mediator.Send(
                    new MessageSendCommand(page.CounterpartId, body, null, input),
                    CancellationToken.None);

                if (!result.IsSuccessful)
                {
                    ToastHelper.Show(
                        AuthFormStyles.GetErrorText(result.ErrorMessages), ToastType.Error, 5000);
                    return;
                }

                page.txtReply.Text = string.Empty;
                ClearPendingAttachment(page);

                ToastHelper.Show(
                    input is null ? "Mesaj gönderildi" : "Dosya gönderildi",
                    ToastType.Success, 2000);

                await RefreshChatPagesAsync();
            }
            catch (Exception ex)
            {
                CrashLog.WriteException("Message.Send", ex);
                ToastHelper.Show("Mesaj gönderilemedi: " + ex.Message, ToastType.Error, 5000);
            }
            finally
            {
                page.btnSend.Enabled = true;
                page.txtReply.Focus();
            }
        }

        /// <summary>
        /// Dosya seçer ve gönderilmek üzere sayfaya bekleyen ek olarak bırakır;
        /// çip satırında görünür, tıklanınca seçim kalkar.
        /// </summary>
        private void PickAttachment(InlineChatPage page)
        {
            using var dialog = new OpenFileDialog
            {
                Title = "Dosya seç",
                Filter = "Tüm dosyalar (*.*)|*.*",
                Multiselect = false
            };

            if (dialog.ShowDialog(this) != DialogResult.OK)
            {
                return;
            }

            var info = new FileInfo(dialog.FileName);

            if (!info.Exists)
            {
                ToastHelper.Show("Dosya bulunamadı.", ToastType.Warning);
                return;
            }

            if (info.Length > MaxAttachmentBytes)
            {
                ToastHelper.Show(
                    $"Dosya 25 MB sınırını aşıyor ({ChatBubblePanel.FormatSize(info.Length)}).",
                    ToastType.Warning, 5000);
                return;
            }

            try
            {
                byte[] content = File.ReadAllBytes(dialog.FileName);

                page.PendingAttachment = (info.Name, content);
                page.lblAttachment.Text =
                    $"Ek: {info.Name} ({ChatBubblePanel.FormatSize(content.LongLength)}) — kaldırmak için tıklayın";
                page.lblAttachment.Visible = true;
            }
            catch (Exception ex)
            {
                CrashLog.WriteException("Message.Attach", ex);
                ToastHelper.Show("Dosya okunamadı: " + ex.Message, ToastType.Error, 5000);
            }
        }

        /// <summary>Bekleyen ek dosyayı kaldırır (çip gizlenir).</summary>
        private static void ClearPendingAttachment(InlineChatPage page)
        {
            page.PendingAttachment = null;
            page.lblAttachment.Text = string.Empty;
            page.lblAttachment.Visible = false;
        }

        /// <summary>
        /// Baloncuktaki eke tıklanınca dosya işletim sistemine açtırılır.
        /// Yol çözümü mesajın gelirken doldurduğu tam yolla yapılır.
        /// </summary>
        private void OpenAttachment(MessageDto message)
        {
            if (string.IsNullOrWhiteSpace(message.AttachmentFullPath))
            {
                return;
            }

            try
            {
                System.Diagnostics.Process.Start(
                    new System.Diagnostics.ProcessStartInfo(message.AttachmentFullPath)
                    {
                        UseShellExecute = true
                    });
            }
            catch (Exception ex)
            {
                CrashLog.WriteException("Message.Attachment", ex);
                ToastHelper.Show("Dosya açılamadı: " + ex.Message, ToastType.Error, 5000);
            }
        }

        /// <summary>
        /// DevExpress editörlerinde değer nullken <c>Text</c>, ipucu yazısını
        /// (Properties.NullText) döndürür; bu durum boş kabul edilir.
        /// </summary>
        private static string EditText(BaseEdit edit)
        {
            string text = edit.Text ?? string.Empty;

            return string.IsNullOrWhiteSpace(text) || text == edit.Properties.NullText
                ? string.Empty
                : text.Trim();
        }

        /// <summary>Açık bir konuşmanın sekme sayfasına ait denetimler.</summary>
        private sealed class InlineChatPage
        {
            public Guid CounterpartId { get; set; }

            public string FullName { get; set; } = string.Empty;

            public XtraTabPage Page { get; set; } = null!;

            public PanelControl pnlHeader { get; set; } = null!;

            public Panel pnlText { get; set; } = null!;

            public LabelControl lblTitle { get; set; } = null!;

            public LabelControl lblDetail { get; set; } = null!;

            public SimpleButton btnClose { get; set; } = null!;

            /// <summary>Eski mesajları yükler; temizlenmişse geçmişi geri yükler.</summary>
            public SimpleButton btnHistory { get; set; } = null!;

            /// <summary>Sohbeti yalnızca bu kullanıcının görünümünden temizler.</summary>
            public SimpleButton btnClear { get; set; } = null!;

            public ChatBubblePanel pnlChat { get; set; } = null!;

            public PanelControl pnlCompose { get; set; } = null!;

            public MemoEdit txtReply { get; set; } = null!;

            /// <summary>Gönderilmeyi bekleyen ek dosya (ad + içerik).</summary>
            public LabelControl lblAttachment { get; set; } = null!;

            public SimpleButton btnAttach { get; set; } = null!;

            public SimpleButton btnSend { get; set; } = null!;

            /// <summary>
            /// Sayfada gösterilen mesajlar (sayfalamayla biriken küme). Her
            /// tazelemede sunucudan gelen pencereyle birleştirilir.
            /// </summary>
            public List<MessageDto> LoadedMessages { get; set; } = [];

            /// <summary>Eski mesaj kalmadıysa <see langword="true"/>; düğme gizlenir.</summary>
            public bool AllHistoryLoaded { get; set; }

            /// <summary>Temizleme durumu sunucudan okundu mu?</summary>
            public bool StateLoaded { get; set; }

            /// <summary>Kullanıcının bu konuşma için temizleme anı; temizlenmemişse null.</summary>
            public DateTimeOffset? ClearedAt { get; set; }

            /// <summary>Gönderilmeyi bekleyen ek.</summary>
            public (string FileName, byte[] Data)? PendingAttachment { get; set; }
        }

        /// <summary>
        /// Duyuru yazma sayfasını sağ alanda bir sekme olarak açar.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Duyuru da tıpkı sohbetler gibi bu ekranın sağ yarısında açılır;
        /// ayrı bir MDI penceresi doğurmak hem "çocuk form başka çocuk
        /// açamaz" kuralını zorlar hem de kullanıcıyı liste ekranından
        /// koparırdı.
        /// </para>
        /// <para>
        /// Sayfa zaten açıksa yenisi üretilmez, o sekme seçilecek: ikinci bir
        /// yazım alanı (ve dolayısıyla iki ayrı gönderim) engellenir.
        /// </para>
        /// </remarks>
        private async Task ComposeAnnouncementAsync()
        {
            if (!await CurrentUserPermissions.HasAsync(MessagePermissions.Announce))
            {
                ToastHelper.Show("Duyuru gönderme yetkiniz yok.", ToastType.Warning);
                return;
            }

            if (_announcementPage is not null)
            {
                tabConversations.SelectedTabPage = _announcementPage.Page;
                _announcementPage.txtSearch.Focus();
                return;
            }

            AnnouncementComposePage page = new()
            {
                SendRequested = SendAnnouncementAsync
            };

            page.CloseRequested += (_, _) => CloseAnnouncementPage();

            _announcementPage = page;

            tabConversations.TabPages.Add(page.Page);
            tabConversations.SelectedTabPage = page.Page;

            lblEmpty.Visible = false;
            tabConversations.Visible = true;

            // Alıcı listesi açılışta arka planda hazırlanır: "Seçili
            // Kişilere" kipine geçişte liste hazır olsun ve olası bir sorun
            // (yetki, boş sonuç, bağlantı hatası) daha ilk açılışta toast +
            // crash.log ile görünür olsun (bkz. AnnouncementComposePage).
            _ = page.LoadRecipientsAsync();

            page.txtSearch.Focus();
        }

        /// <summary>
        /// Yazılan duyuruyu seçilen tüm alıcılara iletir; başarılıysa yazma
        /// sekmesi kapanır ve liste tazelenir.
        /// </summary>
        /// <remarks>
        /// Alan doğrulaması (<see cref="AnnouncementComposePage.Validate"/>)
        /// sunucudaki <c>MessageBroadcastCommandValidator</c> ile aynı
        /// koşulları uygular; böylece eksik konu gibi bir hata daha gönderim
        /// sırasında değil yazım sırasında görünür.
        /// </remarks>
        private async Task SendAnnouncementAsync(AnnouncementComposePage page)
        {
            string? problem = page.Validate();

            if (problem is not null)
            {
                ToastHelper.Show(problem, ToastType.Warning);
                return;
            }

            Guid[] recipientIds = page.SendToAll
                ? []
                : page.GetSelectedRecipients().Select(r => r.Id).ToArray();

            try
            {
                using var scope = Program.Services.CreateScope();
                ISender mediator = scope.ServiceProvider.GetRequiredService<ISender>();

                Result<string> result = await mediator.Send(
                    new MessageBroadcastCommand(
                        recipientIds,
                        page.BodyText,
                        page.SubjectText,
                        page.SendToAll),
                    CancellationToken.None);

                if (!result.IsSuccessful)
                {
                    ToastHelper.Show(
                        AuthFormStyles.GetErrorText(result.ErrorMessages), ToastType.Error, 5000);
                    return;
                }

                ToastHelper.Show(result.Data ?? "Duyuru gönderildi", ToastType.Success, 2500);

                CloseAnnouncementPage();
                await ReloadAsync(showLoading: false);
            }
            catch (Exception ex)
            {
                CrashLog.WriteException("Message.Announce", ex);
                ToastHelper.Show("Duyuru gönderilemedi: " + ex.Message, ToastType.Error, 5000);
            }
        }

        /// <summary>Açık duyuru yazma sekmesini kapatır.</summary>
        private void CloseAnnouncementPage()
        {
            if (_announcementPage is null)
            {
                return;
            }

            AnnouncementComposePage page = _announcementPage;

            // Önce sahiplik bırakılır: sekmeyi kaldırırken tetiklenen
            // SelectedPageChanged olayı sayfayı hâlâ açık sanmamalı.
            _announcementPage = null;

            tabConversations.TabPages.Remove(page.Page);
            page.Dispose();

            if (_chatPages.Count == 0)
            {
                _activeChatId = null;
                lblEmpty.Visible = true;
                tabConversations.Visible = false;
            }
        }
    }
}