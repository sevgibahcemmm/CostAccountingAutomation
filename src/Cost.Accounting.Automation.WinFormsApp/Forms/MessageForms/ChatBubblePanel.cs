using System.Drawing.Drawing2D;
using System.Globalization;
using Cost.Accounting.Automation.Application.Messages;
using Cost.Accounting.Automation.WinFormsApp.Utils;

namespace Cost.Accounting.Automation.WinFormsApp.Forms.MessageForms
{
    /// <summary>
    /// Sohbet uygulamalarındaki gibi baloncuk tabanlı mesaj yüzeyi.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Mesajlar ızgara satırı değil, <b>baloncuk</b> olarak çizilir: kendi
    /// mesajlarınız sağda ve konuşma renginde, karşı tarafın mesajları solda
    /// ve yüzey renginde. Her baloncuğun sağ altında saat ile durum tiki
    /// durur — tek tik gönderildi, gri çift tik teslim edildi, mavi çift tik
    /// okundu.
    /// </para>
    /// <para>
    /// Baloncuklar ayrı denetim (control) olarak oluşturulmaz; yüzlerce
    /// mesajda binlerce denetim hem yavaş hem bellek düşmanıdır. Yerleşim
    /// yalnızca ölçü değiştiğinde (mesaj kümesi ya da pencere boyutu)
    /// yeniden hesaplanır ve her şey <see cref="OnPaint(PaintEventArgs)"/>
    /// içinde çizilir.
    /// </para>
    /// <para>
    /// Tekerlek olayı WinForms'ta odak alan denetime gider; kullanıcı yazı
    /// kutusundayken üzerine geldiği sohbet alanını kaydırabilmesi için
    /// panel kendisi bir mesaj filtresi kurar ve imleç üzerindeyken
    /// kaydırmayı üstlenir.
    /// </para>
    /// </remarks>
    internal sealed class ChatBubblePanel : Panel, IMessageFilter
    {
        private const int WmMouseWheel = 0x020A;

        private const int SideMargin = 12;
        private const int TopMargin = 12;
        private const int BottomMargin = 12;
        private const int BubbleGap = 7;
        private const int BubblePadX = 11;
        private const int BubblePadY = 7;
        private const int MetaGap = 3;
        private const int BubbleRadius = 10;

        /// <summary>Her tekerlek tıklamasında kaydırılacak piksel.</summary>
        private const int WheelStepPixels = 48;

        private readonly Font _bodyFont = new("Segoe UI", 9.5F);
        private readonly Font _subjectFont = new("Segoe UI", 9.5F, FontStyle.Bold);
        private readonly Font _metaFont = new("Segoe UI", 7.75F);
        private readonly Font _dayFont = new("Segoe UI", 8.25F, FontStyle.Bold);
        private readonly Font _emptyFont = new("Segoe UI", 10F);

        private IReadOnlyList<MessageDto> _messages = [];
        private List<BubbleEntry> _entries = [];

        /// <summary>İçeriğin toplam yüksekliği (kaydırma menzili).</summary>
        private int _contentHeight;

        private bool _hasRendered;
        private bool _inLayout;

        public ChatBubblePanel()
        {
            SetStyle(
                ControlStyles.UserPaint
                | ControlStyles.AllPaintingInWmPaint
                | ControlStyles.OptimizedDoubleBuffer
                | ControlStyles.ResizeRedraw,
                true);

            AutoScroll = true;
            BackColor = Color.FromArgb(240, 235, 227);
        }

        // ----------------------------------------------------------------
        // Dışa açılan yüzey
        // ----------------------------------------------------------------

        /// <summary>
        /// Mesajları baloncuklar hâlinde yerleştirir ve görünümü tazeler.
        /// </summary>
        /// <remarks>
        /// Kullanıcı geçmişe kaydırmışsa ve yeni mesaj gelmediyse konumu
        /// korunur; yalnızca yeni bir mesaj geldiğinde ya da en alttaysa
        /// en alta düşülür. Böylece on saniyelik tazeleme okuma yaptığı
        /// yeri sıfırlamaz.
        /// </remarks>
        public void SetMessages(IReadOnlyList<MessageDto> messages)
        {
            bool wasAtBottom = IsAtBottom();
            Guid? previousLastId = _messages.Count == 0 ? null : _messages[^1].Id;

            _messages = messages;

            Relayout();

            Guid? lastId = messages.Count == 0 ? null : messages[^1].Id;

            if (!_hasRendered || wasAtBottom || lastId != previousLastId)
            {
                ScrollToEnd();
            }

            _hasRendered = true;

            Invalidate();
        }

        /// <summary>Verilen piksel kadar yukarı/aşağı kaydırır.</summary>
        public void ScrollBy(int pixels)
        {
            if (pixels == 0 || _contentHeight <= ClientSize.Height)
            {
                return;
            }

            int maxScroll = _contentHeight - ClientSize.Height;
            int current = -AutoScrollPosition.Y;
            int target = Math.Clamp(current - pixels, 0, maxScroll);

            if (target != current)
            {
                AutoScrollPosition = new Point(0, target);
            }
        }

        // ----------------------------------------------------------------
        // Yerleşim
        // ----------------------------------------------------------------

        private bool IsAtBottom()
        {
            if (_contentHeight <= ClientSize.Height)
            {
                return true;
            }

            int top = -AutoScrollPosition.Y;

            // Birkaç piksellik pay: kaydırma çubuğu kenarına tam yapışmamış
            // olmak "en altta" sayılır, aksi halde tazeleme anında gereksiz
            // alta sıçrama olur.
            return top + ClientSize.Height >= _contentHeight - 48;
        }

        private void ScrollToEnd()
        {
            if (_contentHeight <= ClientSize.Height)
            {
                return;
            }

            AutoScrollPosition = new Point(0, _contentHeight - ClientSize.Height);
        }

        /// <summary>
        /// Baloncuk yerleşimini yeniden hesaplar ve kaydırma menzilini kurar.
        /// </summary>
        /// <remarks>
        /// Dikey kaydırma çubuğu içeriğe sığmadığında görünür olur ve genişliği
        /// düşürür; bu da baloncukları yeniden daraltmayı gerektirir. Bu yüzden
        /// ölçüm gerektiğinde iki kez yapılır: önce tam genişlikle, sonra
        /// çubuk payı çıkarılmış genişlikle.
        /// </remarks>
        private void Relayout()
        {
            if (_inLayout || IsDisposed)
            {
                return;
            }

            _inLayout = true;

            try
            {
                int width = ClientSize.Width;

                if (width <= 0)
                {
                    return;
                }

                List<BubbleEntry> entries = BuildEntries(width, out int height);

                if (height > ClientSize.Height
                    && width > SystemInformation.VerticalScrollBarWidth + 160)
                {
                    width -= SystemInformation.VerticalScrollBarWidth;
                    entries = BuildEntries(width, out height);
                }

                _entries = entries;
                _contentHeight = height;

                AutoScrollMinSize = new Size(width, height);
            }
            finally
            {
                _inLayout = false;
            }
        }

        /// <summary>Verilen genişlikte baloncuk ve gün ayırıcılarının yerini hesaplar.</summary>
        private List<BubbleEntry> BuildEntries(int width, out int contentHeight)
        {
            var entries = new List<BubbleEntry>();

            // Baloncuk genişliği ekranın büyük bölümünü kaplamaz: uzun mesajlar
            // okunabilir satır uzunluğunda kalır, sohbet yüzeyi de görünür kalır.
            int maxWidth = Math.Clamp((int)((width - SideMargin * 2) * 0.72F), 180, 560);

            int y = TopMargin;
            bool hasDay = false;
            DateTime currentDay = DateTime.MinValue;

            foreach (MessageDto message in _messages)
            {
                DateTime day = message.SentAt.ToLocalTime().Date;

                if (!hasDay || day != currentDay)
                {
                    hasDay = true;
                    currentDay = day;

                    string label = DayLabel(day);
                    Size daySize = TextRenderer.MeasureText(label, _dayFont);
                    int pillWidth = daySize.Width + 26;
                    int pillHeight = daySize.Height + 8;
                    int pillX = Math.Max(SideMargin, (width - pillWidth) / 2);

                    entries.Add(new BubbleEntry
                    {
                        DayLabel = label,
                        Bounds = new Rectangle(pillX, y, pillWidth, pillHeight)
                    });

                    y += pillHeight + BubbleGap;
                }

                string? subject = string.IsNullOrWhiteSpace(message.Subject)
                    ? null
                    : message.Subject;

                Size bodySize = TextRenderer.MeasureText(
                    message.Body, _bodyFont, new Size(maxWidth, int.MaxValue), TextFormatFlags.WordBreak);

                Size subjectSize = subject is null
                    ? Size.Empty
                    : TextRenderer.MeasureText(
                        subject, _subjectFont, new Size(maxWidth, int.MaxValue), TextFormatFlags.WordBreak);

                string time = TimeOf(message);
                string tick = TickOf(message);

                Size timeSize = TextRenderer.MeasureText(time, _metaFont);
                Size tickSize = tick.Length == 0
                    ? Size.Empty
                    : TextRenderer.MeasureText(tick, _metaFont);

                int metaWidth = timeSize.Width
                    + (tick.Length == 0 ? 0 : tickSize.Width + 4);

                int innerWidth = Math.Max(Math.Max(bodySize.Width, subjectSize.Width), metaWidth);
                int bubbleWidth = innerWidth + BubblePadX * 2;
                int bubbleHeight = BubblePadY
                    + (subject is null ? 0 : subjectSize.Height + 3)
                    + bodySize.Height
                    + MetaGap
                    + _metaFont.Height
                    + BubblePadY;

                int x = message.SentByCurrentUser
                    ? width - SideMargin - bubbleWidth
                    : SideMargin;

                var bounds = new Rectangle(x, y, bubbleWidth, bubbleHeight);

                int textX = x + BubblePadX;
                int textWidth = bubbleWidth - BubblePadX * 2;
                int cursorY = y + BubblePadY;

                var subjectRect = Rectangle.Empty;

                if (subject is not null)
                {
                    subjectRect = new Rectangle(textX, cursorY, textWidth, subjectSize.Height);
                    cursorY += subjectSize.Height + 3;
                }

                var bodyRect = new Rectangle(textX, cursorY, textWidth, bodySize.Height);
                cursorY += bodySize.Height + MetaGap;

                var metaRect = new Rectangle(textX, cursorY, textWidth, _metaFont.Height);

                entries.Add(new BubbleEntry
                {
                    Message = message,
                    Bounds = bounds,
                    SubjectRectangle = subjectRect,
                    BodyRectangle = bodyRect,
                    MetaRectangle = metaRect
                });

                y += bubbleHeight + BubbleGap;
            }

            contentHeight = Math.Max(y - BubbleGap + BottomMargin, 48);

            return entries;
        }

        // ----------------------------------------------------------------
        // Boyama
        // ----------------------------------------------------------------

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            Palette palette = ResolvePalette();

            e.Graphics.Clear(palette.Canvas);

            if (_messages.Count == 0)
            {
                DrawEmptyState(e.Graphics, palette);
                return;
            }

            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            e.Graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            Point offset = AutoScrollPosition;

            foreach (BubbleEntry entry in _entries)
            {
                Rectangle bounds = entry.Bounds;
                bounds.Offset(offset);

                // Ekranda olmayan baloncuk çizilmez; uzun konuşmalarda bu,
                // boyama süresini belirgin biçimde düşürür.
                if (bounds.Bottom < 0 || bounds.Top > ClientSize.Height)
                {
                    continue;
                }

                if (entry.DayLabel is not null)
                {
                    DrawDayPill(e.Graphics, entry, palette, offset);
                }
                else
                {
                    DrawBubble(e.Graphics, entry, palette, offset);
                }
            }
        }

        private void DrawEmptyState(Graphics graphics, Palette palette)
        {
            const string Text = "Henüz mesaj yok. İlk mesajı siz yazın.";

            Size size = TextRenderer.MeasureText(Text, _emptyFont);

            TextRenderer.DrawText(
                graphics,
                Text,
                _emptyFont,
                new Point((ClientSize.Width - size.Width) / 2, (ClientSize.Height - size.Height) / 2),
                palette.EmptyText);
        }

        private void DrawDayPill(Graphics graphics, BubbleEntry entry, Palette palette, Point offset)
        {
            Rectangle bounds = entry.Bounds;
            bounds.Offset(offset);

            using GraphicsPath path = Rounded(bounds, BubbleRadius);
            using var fill = new SolidBrush(palette.DayPill);
            graphics.FillPath(fill, path);

            Size size = TextRenderer.MeasureText(entry.DayLabel!, _dayFont);
            var location = new Point(
                bounds.X + ((bounds.Width - size.Width) / 2),
                bounds.Y + ((bounds.Height - size.Height) / 2));

            TextRenderer.DrawText(
                graphics, entry.DayLabel!, _dayFont, location, palette.DayText, TextFormatFlags.NoPadding);
        }

        private void DrawBubble(Graphics graphics, BubbleEntry entry, Palette palette, Point offset)
        {
            MessageDto message = entry.Message!;
            bool mine = message.SentByCurrentUser;

            Rectangle bounds = entry.Bounds;
            bounds.Offset(offset);

            Color bubbleColor = mine ? palette.OwnBubble : palette.OtherBubble;
            Color textColor = mine ? palette.OwnText : palette.OtherText;
            Color metaColor = mine ? palette.OwnMeta : palette.OtherMeta;
            Color borderColor = SkinTheme.Blend(bubbleColor, textColor, 0.12F);

            using GraphicsPath path = Rounded(bounds, BubbleRadius);
            using var fill = new SolidBrush(bubbleColor);
            graphics.FillPath(fill, path);

            using var border = new Pen(borderColor, 1F);
            graphics.DrawPath(border, path);

            if (entry.SubjectRectangle != Rectangle.Empty)
            {
                Rectangle subjectRect = entry.SubjectRectangle;
                subjectRect.Offset(offset);

                TextRenderer.DrawText(
                    graphics,
                    message.Subject!,
                    _subjectFont,
                    subjectRect,
                    textColor,
                    TextFormatFlags.WordBreak | TextFormatFlags.EndEllipsis);
            }

            Rectangle bodyRect = entry.BodyRectangle;
            bodyRect.Offset(offset);

            TextRenderer.DrawText(
                graphics,
                message.Body,
                _bodyFont,
                bodyRect,
                textColor,
                TextFormatFlags.WordBreak);

            // Saat ve tik, baloncuğun sağ altına hizalanır. DrawString'in
            // hizalama bayrağı kullanılır: iki metnin genişliğini ayrı ayrı
            // ölçmek yerine dikdörtgenlere sağa dayamak daha güvenlidir.
            Rectangle metaRect = entry.MetaRectangle;
            metaRect.Offset(offset);

            string time = TimeOf(message);
            string tick = TickOf(message);
            Color tickColor = message.IsRead ? palette.TickRead : palette.TickMuted;

            int tickWidth = tick.Length == 0
                ? 0
                : (int)Math.Ceiling(graphics.MeasureString(tick, _metaFont).Width);

            using (var metaBrush = new SolidBrush(metaColor))
            using (var tickBrush = new SolidBrush(tickColor))
            using (var format = new StringFormat
            {
                Alignment = StringAlignment.Far,
                LineAlignment = StringAlignment.Center,
                FormatFlags = StringFormatFlags.NoWrap | StringFormatFlags.NoClip
            })
            {
                if (tick.Length > 0)
                {
                    var tickRect = new RectangleF(
                        metaRect.Right - tickWidth, metaRect.Top, tickWidth, metaRect.Height);

                    graphics.DrawString(tick, _metaFont, tickBrush, tickRect, format);
                }

                var timeRect = new RectangleF(
                    metaRect.X,
                    metaRect.Top,
                    metaRect.Width - tickWidth - (tick.Length == 0 ? 0 : 4),
                    metaRect.Height);

                graphics.DrawString(time, _metaFont, metaBrush, timeRect, format);
            }
        }

        // ----------------------------------------------------------------
        // Renkler
        // ----------------------------------------------------------------

        /// <summary>
        /// Aktif tema için sohbet paletini çözer. Her boyamada hesaplanır;
        /// böylece skin değiştiğinde ayrıca bir kayıt/güncelleme gerekmez.
        /// </summary>
        private Palette ResolvePalette()
        {
            Color surface = SkinTheme.SurfaceOf(this);
            bool dark = SkinTheme.IsDarkSkin;

            // WhatsApp'ın açık temasındaki kâğıt rengine, koyu temasında ise
            // koyu lacivert zemine yaklaşan bir sohbet zemini.
            Color canvas = dark
                ? SkinTheme.Blend(surface, Color.FromArgb(9, 17, 21), 0.60F)
                : SkinTheme.Blend(surface, Color.FromArgb(232, 224, 212), 0.75F);

            // Giden mesajlar yeşil değil, mavidir: uygulamanın vurgu rengi budur.
            // Okundu tiki de aynı mavi ailenin parçası olduğu için açık temada
            // açık mavi baloncuğun üzerinde, koyu temada ise orta mavi zeminin
            // üzerinde net biçimde ayırt edilir.
            Color ownAccent = Color.FromArgb(37, 99, 235);

            Color ownBubble = dark
                ? SkinTheme.Blend(ownAccent, surface, 0.40F)
                : SkinTheme.Blend(ownAccent, surface, 0.82F);

            Color otherBubble = dark
                ? SkinTheme.Blend(surface, Color.White, 0.08F)
                : SkinTheme.Blend(Color.White, surface, 0.40F);

            Color ownText = SkinTheme.GetContrastText(ownBubble);
            Color otherText = dark ? SkinTheme.Text : Color.FromArgb(32, 41, 55);

            Color dayPill = dark
                ? SkinTheme.Blend(canvas, Color.White, 0.12F)
                : SkinTheme.Blend(canvas, Color.White, 0.75F);

            return new Palette(
                Canvas: canvas,
                OwnBubble: ownBubble,
                OtherBubble: otherBubble,
                OwnText: ownText,
                OtherText: otherText,
                OwnMeta: SkinTheme.Blend(ownText, ownBubble, 0.40F),
                OtherMeta: SkinTheme.Blend(otherText, otherBubble, 0.45F),
                TickMuted: SkinTheme.EnsureReadable(SkinTheme.MutedText(ownBubble), ownBubble),
                TickRead: SkinTheme.EnsureReadable(Color.FromArgb(37, 119, 252), ownBubble),
                DayPill: dayPill,
                DayText: SkinTheme.EnsureReadable(SkinTheme.MutedText(dayPill), dayPill),
                EmptyText: SkinTheme.EnsureReadable(SkinTheme.MutedText(canvas), canvas));
        }

        // ----------------------------------------------------------------
        // Yardımcılar
        // ----------------------------------------------------------------

        /// <summary>
        /// Durum tiki. WhatsApp ile aynı anlamı taşır: tek tik gönderildi,
        /// çift tik teslim edildi, mavi çift tik okundu. Okunmuş mesajlarda
        /// tiklerin yanına küçük "Okundu" yazısı da eklenir; böylece durum
        /// yalnızca renkle değil, sözcükle de anlaşılır.
        /// </summary>
        private static string TickOf(MessageDto message)
        {
            if (!message.SentByCurrentUser)
            {
                return string.Empty;
            }

            if (message.IsRead)
            {
                return "\u2713\u2713 Okundu";
            }

            return message.IsDelivered
                ? "\u2713\u2713"
                : "\u2713";
        }

        private static string TimeOf(MessageDto message)
            => message.SentAt.ToLocalTime().ToString("HH:mm", CultureInfo.CurrentCulture);

        /// <summary>Gün ayırıcısının metni: "Bugün", "Dün" ya da tarih.</summary>
        private static string DayLabel(DateTime day)
        {
            DateTime today = DateTime.Today;

            if (day == today)
            {
                return "Bugün";
            }

            if (day == today.AddDays(-1))
            {
                return "Dün";
            }

            return day.ToString("d MMMM yyyy", CultureInfo.CurrentCulture);
        }

        private static GraphicsPath Rounded(Rectangle bounds, int radius)
        {
            int diameter = Math.Min(radius * 2, Math.Min(bounds.Width, bounds.Height));

            var path = new GraphicsPath();

            if (diameter <= 4)
            {
                path.AddRectangle(bounds);
                return path;
            }

            path.AddArc(bounds.X, bounds.Y, diameter, diameter, 180, 90);
            path.AddArc(bounds.Right - diameter, bounds.Y, diameter, diameter, 270, 90);
            path.AddArc(bounds.Right - diameter, bounds.Bottom - diameter, diameter, diameter, 0, 90);
            path.AddArc(bounds.X, bounds.Bottom - diameter, diameter, diameter, 90, 90);
            path.CloseFigure();

            return path;
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);

            if (_inLayout || !IsHandleCreated)
            {
                return;
            }

            bool wasAtBottom = IsAtBottom();

            Relayout();

            if (wasAtBottom)
            {
                ScrollToEnd();
            }

            Invalidate();
        }

        // ----------------------------------------------------------------
        // Tekerlek / kaydırma
        // ----------------------------------------------------------------

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);

            // "Application" adı, içinde bulunulan ad uzayındaki
            // Cost.Accounting.Automation.Application nedeniyle System.Windows.Forms
            // sınıfına değil, o ad uzayına çözülür; bu yüzden tam ad yazılır.
            System.Windows.Forms.Application.AddMessageFilter(this);

            // Kaydırma konumu tutamacı (handle) oluşurken sıfırlanabilir; ekran
            // açılırken sohbetin en son mesaja (alta) açıldığından emin olmak
            // için yerleşim ve konum burada bir kez daha kurulur.
            Relayout();
            ScrollToEnd();
            Invalidate();
        }

        protected override void OnHandleDestroyed(EventArgs e)
        {
            System.Windows.Forms.Application.RemoveMessageFilter(this);

            base.OnHandleDestroyed(e);
        }

        /// <summary>
        /// İmleç sohbet alanı üzerindeyken tekerlek kaydırmasını bu panel
        /// üstlenir; aksi halde odakta olan yazı kutusu kayar ve kullanıcı
        /// sohbeti kaydıramaz.
        /// </summary>
        public bool PreFilterMessage(ref Message m)
        {
            if (m.Msg != WmMouseWheel || !IsHandleCreated || !Visible || _contentHeight <= ClientSize.Height)
            {
                return false;
            }

            Point cursor = PointToClient(Cursor.Position);

            if (!ClientRectangle.Contains(cursor))
            {
                return false;
            }

            int delta = (short)(unchecked((long)m.WParam.ToInt64()) >> 16);

            if (delta == 0)
            {
                return false;
            }

            ScrollBy((delta * WheelStepPixels) / 120);

            return true;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _bodyFont.Dispose();
                _subjectFont.Dispose();
                _metaFont.Dispose();
                _dayFont.Dispose();
                _emptyFont.Dispose();
            }

            base.Dispose(disposing);
        }

        // ----------------------------------------------------------------
        // Yerleşim kayıtları
        // ----------------------------------------------------------------

        /// <summary>Yerleşimde hesaplanmış bir öğe: baloncuk ya da gün ayırıcı.</summary>
        private sealed class BubbleEntry
        {
            public MessageDto? Message { get; init; }

            /// <summary>Gün ayırıcı metni; baloncuklarda boştur.</summary>
            public string? DayLabel { get; init; }

            public Rectangle Bounds { get; init; }

            public Rectangle SubjectRectangle { get; init; }

            public Rectangle BodyRectangle { get; init; }

            public Rectangle MetaRectangle { get; init; }
        }

        private sealed record Palette(
            Color Canvas,
            Color OwnBubble,
            Color OtherBubble,
            Color OwnText,
            Color OtherText,
            Color OwnMeta,
            Color OtherMeta,
            Color TickMuted,
            Color TickRead,
            Color DayPill,
            Color DayText,
            Color EmptyText);
    }
}
