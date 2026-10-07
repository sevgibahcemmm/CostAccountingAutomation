using System.Drawing.Drawing2D;
using System.Globalization;
using System.IO;
using Cost.Accounting.Automation.Application.Messages;
using Cost.Accounting.Automation.WinFormsApp.Utils;
using DevExpress.Utils.Menu;

namespace Cost.Accounting.Automation.WinFormsApp.Forms.MessageForms
{

    internal sealed class ChatBubblePanel : Panel, IMessageFilter
    {
        private const int WmMouseWheel = 0x020A;

        private const int SideMargin = 12;
        private const int TopMargin = 12;
        private const int BottomMargin = 12;
        private const int BlockGap = 12;
        private const int BlockPadX = 8;
        private const int BlockPadY = 5;
        private const int SectionGap = 4;
        private const int BlockRadius = 8;
        private const int DayRadius = 10;
        private const int TickGap = 5;

        /// <summary>Resim ekinin içindeki en büyük yükseklik.</summary>
        private const int ImageMaxHeight = 200;

        /// <summary>Dosya çipinin (ad + boyut) yatay/dikey iç payı.</summary>
        private const int ChipPadX = 10;
        private const int ChipPadY = 6;

        /// <summary>Bir sohbet için bellekte tutulan en fazla önizleme sayısı.</summary>
        private const int MaxCachedImages = 64;

        /// <summary>Her tekerlek tıklamasında kaydırılacak piksel.</summary>
        private const int WheelStepPixels = 48;

        private readonly Font _bodyFont = new("Segoe UI", 9.5F);
        private readonly Font _subjectFont = new("Segoe UI", 9.5F, FontStyle.Bold);
        private readonly Font _metaFont = new("Segoe UI", 7.75F);
        private readonly Font _tickFont = new("Segoe UI", 9F);
        private readonly Font _dayFont = new("Segoe UI", 8.25F, FontStyle.Bold);
        private readonly Font _emptyFont = new("Segoe UI", 10F);
        private readonly Font _chipFont = new("Segoe UI", 9F);

        private IReadOnlyList<MessageDto> _messages = [];
        private List<BubbleEntry> _entries = [];

        /// <summary>
        /// Resim ekleri için blok başına önbellek. Anahtar göreli depo
        /// yoludur; değer <c>null</c> ise dosya bulunamadı/açılamadı ve çip
        /// çizilir. Önbellek panelle birlikte ölür.
        /// </summary>
        private readonly Dictionary<string, Bitmap?> _attachmentImages = new(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Kullanıcı bir eke (resme ya da dosya çipine) tıkladığında çağrılır;
        /// dosyayı açma işlemi sahibi formun sorumluluğudur.
        /// </summary>
        public event Action<MessageDto>? AttachmentOpenRequested;

        /// <summary>Kullanıcı kendi mesajını düzenlemek istediğinde çağrılır.</summary>
        public event Action<MessageDto>? EditRequested;

        /// <summary>Kullanıcı mesajı silmek istediğinde çağrılır.</summary>
        public event Action<MessageDto>? DeleteRequested;

        /// <summary>İçeriğin toplam yüksekliği (kaydırma menzili).</summary>
        private int _contentHeight;

        /// <summary>İmlecin üzerinde durduğu mesaj (sağ tık menüsü ve vurgu için).</summary>
        private Guid? _hoveredId;

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
        /// Mesajları yerleştirir ve görünümü tazeler.
        /// </summary>
        /// <remarks>
        /// Kullanıcı geçmişe kaydırmışsa ve yeni mesaj gelmediyse konumu
        /// korunur; yalnızca yeni bir mesaj geldiğinde ya da en alttaysa
        /// en alta düşülür. Böylece on saniyelik tazeleme okuma yaptığı
        /// yeri sıfırlamaz.
        /// </remarks>
        public void SetMessages(IReadOnlyList<MessageDto> messages)
            => SetMessages(messages, preserveTopAnchor: false);

        /// <summary>
        /// Mesajları yerleştirir; <paramref name="preserveTopAnchor"/>
        /// <c>true</c> ise eski mesajlar üstten eklendikten sonra daha önce
        /// en üstte görünen mesaj aynı ekran yerinde kalır (kullanıcı
        /// "daha eskilerini yüklerken" konumu kaybolmaz).
        /// </summary>
        public void SetMessages(IReadOnlyList<MessageDto> messages, bool preserveTopAnchor)
        {
            Guid? anchorId = null;
            int anchorScreenTop = 0;

            if (preserveTopAnchor && _entries.Count > 0)
            {
                int scrollBefore = -AutoScrollPosition.Y;

                foreach (BubbleEntry entry in _entries)
                {
                    if (entry.Message is not null && entry.Bounds.Bottom > scrollBefore)
                    {
                        anchorId = entry.Message.Id;
                        anchorScreenTop = entry.Bounds.Top - scrollBefore;
                        break;
                    }
                }
            }

            bool wasAtBottom = IsAtBottom();
            Guid? previousLastId = _messages.Count == 0 ? null : _messages[^1].Id;

            _messages = messages;
            _hoveredId = null;

            Relayout();

            Guid? lastId = messages.Count == 0 ? null : messages[^1].Id;

            if (anchorId is { } id && FindEntry(id) is { } anchor)
            {
                int target = Math.Clamp(
                    anchor.Bounds.Top - anchorScreenTop,
                    0,
                    Math.Max(0, _contentHeight - ClientSize.Height));

                AutoScrollPosition = new Point(0, target);
            }
            else if (!_hasRendered || wasAtBottom || lastId != previousLastId)
            {
                ScrollToEnd();
            }

            _hasRendered = true;

            Invalidate();
        }

        /// <summary>Verilen mesaja karşılık gelen yerleşim kaydı.</summary>
        private BubbleEntry? FindEntry(Guid messageId)
        {
            foreach (BubbleEntry entry in _entries)
            {
                if (entry.Message is not null && entry.Message.Id == messageId)
                {
                    return entry;
                }
            }

            return null;
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
        /// Mesaj bloklarının yerleşimini yeniden hesaplar ve kaydırma
        /// menzilini kurar.
        /// </summary>
        /// <remarks>
        /// Dikey kaydırma çubuğu içeriğe sığmadığında görünür olur ve genişliği
        /// düşürür; bu da blokları yeniden daraltmayı gerektirir. Bu yüzden
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

        /// <summary>Verilen genişlikte mesaj bloklarının ve gün ayırıcılarının yerini hesaplar.</summary>
        private List<BubbleEntry> BuildEntries(int width, out int contentHeight)
        {
            var entries = new List<BubbleEntry>();

            // Blok genişliği ekranın büyük bölümünü kaplamaz: uzun mesajlar
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

                    y += pillHeight + BlockGap;
                }

                string? subject = string.IsNullOrWhiteSpace(message.Subject)
                    ? null
                    : message.Subject;

                List<string> subjectLines = subject is null
                    ? []
                    : WrapLines(subject, _subjectFont, maxWidth);

                List<string> bodyLines = message.Body.Length == 0
                    ? []
                    : WrapLines(message.Body, _bodyFont, maxWidth);

                // Ek: resimse ölçeklenmiş önizleme, diğer dosyalar ad + boyut
                // çipi. Her iki durumda da blok metin bloğu gibi sola dayanır.
                Bitmap? attachmentImage = null;
                Size attachmentSize = Size.Empty;
                List<string> chipLines = [];

                if (message.AttachmentPath is not null)
                {
                    if (message.AttachmentIsImage)
                    {
                        attachmentImage = GetAttachmentImage(message);
                    }

                    if (attachmentImage is not null)
                    {
                        double scale = Math.Min(
                            1.0,
                            Math.Min(
                                (double)maxWidth / attachmentImage.Width,
                                (double)ImageMaxHeight / attachmentImage.Height));

                        attachmentSize = new Size(
                            Math.Max(1, (int)(attachmentImage.Width * scale)),
                            Math.Max(1, (int)(attachmentImage.Height * scale)));
                    }
                    else
                    {
                        chipLines = WrapLines(ChipTextOf(message), _chipFont, Math.Max(80, maxWidth - ChipPadX * 2));

                        int chipWidth = 0;

                        foreach (string line in chipLines)
                        {
                            chipWidth = Math.Max(chipWidth, Measure(line, _chipFont));
                        }

                        attachmentSize = new Size(
                            Math.Min(maxWidth, chipWidth + ChipPadX * 2),
                            chipLines.Count * _chipFont.Height + ChipPadY * 2);
                    }
                }

                string tick = TickOf(message);
                int tickWidth = tick.Length == 0 ? 0 : Measure(tick, _tickFont);

                List<(string Text, bool Soft)> metaParts = MetaParts(message);
                int metaWidth = 0;

                foreach ((string text, bool soft) in metaParts)
                {
                    metaWidth += Measure(text, _metaFont);
                }

                int innerWidth = metaWidth;
                int anchorWidth = 0;
                bool hasAnchor = false;

                foreach (string line in subjectLines)
                {
                    innerWidth = Math.Max(innerWidth, Measure(line, _subjectFont));
                }

                foreach (string line in bodyLines)
                {
                    innerWidth = Math.Max(innerWidth, Measure(line, _bodyFont));
                }

                innerWidth = Math.Max(innerWidth, attachmentSize.Width);

                // Tik, son yazının hemen sağında durur; blok genişliği buna
                // göre genişletilir ki tik kenardan taşmasın.
                if (bodyLines.Count > 0)
                {
                    anchorWidth = Measure(bodyLines[^1], _bodyFont);
                    hasAnchor = true;
                }
                else if (!attachmentSize.IsEmpty)
                {
                    anchorWidth = attachmentSize.Width;
                    hasAnchor = true;
                }
                else if (subjectLines.Count > 0)
                {
                    anchorWidth = Measure(subjectLines[^1], _subjectFont);
                    hasAnchor = true;
                }

                if (tickWidth > 0 && hasAnchor)
                {
                    innerWidth = Math.Max(innerWidth, anchorWidth + TickGap + tickWidth);
                }

                int blockWidth = innerWidth + BlockPadX * 2;
                int blockTop = y;
                int x = message.SentByCurrentUser
                    ? width - SideMargin - blockWidth
                    : SideMargin;
                int contentX = x + BlockPadX;
                int contentY = blockTop + BlockPadY;
                int cursor = contentY;

                int subjectTop = cursor;

                if (subjectLines.Count > 0)
                {
                    cursor += subjectLines.Count * _subjectFont.Height + SectionGap;
                }

                var attachmentRect = Rectangle.Empty;

                if (!attachmentSize.IsEmpty)
                {
                    attachmentRect = new Rectangle(contentX, cursor, attachmentSize.Width, attachmentSize.Height);
                    cursor += attachmentSize.Height + SectionGap;
                }

                int bodyTop = cursor;

                if (bodyLines.Count > 0)
                {
                    cursor += bodyLines.Count * _bodyFont.Height + SectionGap;
                }

                int metaTop = cursor;
                cursor += _metaFont.Height;

                int blockHeight = (cursor - contentY) + BlockPadY * 2;

                var tickRect = Rectangle.Empty;

                if (tickWidth > 0 && hasAnchor)
                {
                    if (bodyLines.Count > 0)
                    {
                        int lastLineTop = bodyTop + (bodyLines.Count - 1) * _bodyFont.Height;
                        tickRect = new Rectangle(
                            contentX + anchorWidth + TickGap,
                            lastLineTop + Math.Max(0, (_bodyFont.Height - _tickFont.Height) / 2),
                            tickWidth,
                            _tickFont.Height);
                    }
                    else if (!attachmentSize.IsEmpty)
                    {
                        tickRect = new Rectangle(
                            attachmentRect.Right + TickGap,
                            attachmentRect.Bottom - _tickFont.Height,
                            tickWidth,
                            _tickFont.Height);
                    }
                    else
                    {
                        int lastLineTop = subjectTop + (subjectLines.Count - 1) * _subjectFont.Height;
                        tickRect = new Rectangle(
                            contentX + anchorWidth + TickGap,
                            lastLineTop + Math.Max(0, (_subjectFont.Height - _tickFont.Height) / 2),
                            tickWidth,
                            _tickFont.Height);
                    }
                }

                entries.Add(new BubbleEntry
                {
                    Message = message,
                    Bounds = new Rectangle(x, blockTop, blockWidth, blockHeight),
                    SubjectLines = subjectLines,
                    SubjectTop = subjectTop,
                    AttachmentRectangle = attachmentRect,
                    AttachmentLines = chipLines,
                    BodyLines = bodyLines,
                    BodyTop = bodyTop,
                    TickRectangle = tickRect,
                    MetaParts = metaParts,
                    MetaRectangle = new Rectangle(contentX, metaTop, innerWidth, _metaFont.Height)
                });

                y += blockHeight + BlockGap;
            }

            contentHeight = Math.Max(y - BlockGap + BottomMargin, 48);

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

                // Ekranda olmayan blok çizilmez; uzun konuşmalarda bu,
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
                    DrawMessage(e.Graphics, entry, palette, offset);
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

            using GraphicsPath path = Rounded(bounds, DayRadius);
            using var fill = new SolidBrush(palette.DayPill);
            graphics.FillPath(fill, path);

            Size size = TextRenderer.MeasureText(entry.DayLabel!, _dayFont);
            var location = new Point(
                bounds.X + ((bounds.Width - size.Width) / 2),
                bounds.Y + ((bounds.Height - size.Height) / 2));

            TextRenderer.DrawText(
                graphics, entry.DayLabel!, _dayFont, location, palette.DayText, TextFormatFlags.NoPadding);
        }

        /// <summary>
        /// Tek bir mesajı kutusuz olarak çizer: zemin dolgusu yok, metin
        /// doğrudan sohbet zemininin üzerinde durur. Yalnızca imleç
        /// mesajın üzerindeyken arkasında hafif bir vurgu belirir.
        /// </summary>
        private void DrawMessage(Graphics graphics, BubbleEntry entry, Palette palette, Point offset)
        {
            MessageDto message = entry.Message!;

            Rectangle bounds = entry.Bounds;
            bounds.Offset(offset);

            if (_hoveredId == message.Id)
            {
                using GraphicsPath hoverPath = Rounded(bounds, BlockRadius);
                using var hoverBrush = new SolidBrush(palette.Hover);
                graphics.FillPath(hoverBrush, hoverPath);
            }

            int contentX = bounds.X + BlockPadX;

            int lineY = entry.SubjectTop + offset.Y;

            foreach (string line in entry.SubjectLines)
            {
                DrawLine(graphics, line, _subjectFont, contentX, lineY, palette.SubjectText);
                lineY += _subjectFont.Height;
            }

            if (entry.AttachmentRectangle != Rectangle.Empty)
            {
                Rectangle attachmentRect = entry.AttachmentRectangle;
                attachmentRect.Offset(offset);

                DrawAttachment(graphics, entry, palette, attachmentRect);
            }

            lineY = entry.BodyTop + offset.Y;

            foreach (string line in entry.BodyLines)
            {
                DrawLine(graphics, line, _bodyFont, contentX, lineY, palette.BodyText);
                lineY += _bodyFont.Height;
            }

            if (entry.TickRectangle != Rectangle.Empty)
            {
                Rectangle tickRect = entry.TickRectangle;
                tickRect.Offset(offset);

                TextRenderer.DrawText(
                    graphics,
                    TickOf(message),
                    _tickFont,
                    tickRect.Location,
                    message.IsRead ? palette.TickRead : palette.TickMuted,
                    TextFormatFlags.NoPadding | TextFormatFlags.SingleLine | TextFormatFlags.NoPrefix);
            }

            Rectangle metaRect = entry.MetaRectangle;
            metaRect.Offset(offset);

            DrawMeta(graphics, entry, palette, metaRect);
        }

        private static void DrawLine(Graphics graphics, string text, Font font, int x, int y, Color color)
        {
            if (text.Length == 0)
            {
                return;
            }

            TextRenderer.DrawText(
                graphics,
                text,
                font,
                new Point(x, y),
                color,
                TextFormatFlags.NoPadding | TextFormatFlags.SingleLine | TextFormatFlags.NoPrefix);
        }

        /// <summary>
        /// Saat ile durum satırını bloğun sağ kenarına dayayarak çizer.
        /// Saat normal, "Okundu" ve "düzenlendi" bilgisi bilerek daha soluk
        /// renkte yazılır; satır okunurken öne çıkan tek şey mesajın kendisi
        /// olur.
        /// </summary>
        private void DrawMeta(Graphics graphics, BubbleEntry entry, Palette palette, Rectangle metaRect)
        {
            int total = 0;

            foreach ((string text, bool soft) in entry.MetaParts!)
            {
                total += Measure(text, _metaFont);
            }

            int x = metaRect.Right - total;

            foreach ((string text, bool soft) in entry.MetaParts)
            {
                int partWidth = Measure(text, _metaFont);

                TextRenderer.DrawText(
                    graphics,
                    text,
                    _metaFont,
                    new Point(x, metaRect.Y),
                    soft ? palette.ReadText : palette.MetaText,
                    TextFormatFlags.NoPadding | TextFormatFlags.SingleLine | TextFormatFlags.NoPrefix);

                x += partWidth;
            }
        }

        /// <summary>
        /// Ek dosyayı çizer: resimse yuvarlatılmış köşeli, ince çerçeveli
        /// önizleme, değilse ad + boyut çipi. Her iki durumda da alana
        /// tıklanınca dosya açılır (bkz. <see cref="AttachmentOpenRequested"/>).
        /// </summary>
        private void DrawAttachment(
            Graphics graphics,
            BubbleEntry entry,
            Palette palette,
            Rectangle attachmentRect)
        {
            MessageDto message = entry.Message!;

            Bitmap? image = message.AttachmentIsImage ? GetAttachmentImage(message) : null;

            if (image is not null)
            {
                using GraphicsPath clip = Rounded(attachmentRect, BlockRadius);
                GraphicsState state = graphics.Save();

                graphics.SetClip(clip);
                graphics.DrawImage(image, attachmentRect);
                graphics.Restore(state);

                using var border = new Pen(palette.AttachmentBorder, 1F);
                graphics.DrawPath(border, clip);
                return;
            }

            // Resim değil ya da açılamadı: dosya adı + boyut çipi.
            Rectangle chipBounds = attachmentRect;
            int radius = Math.Min(BlockRadius, Math.Max(4, Math.Min(chipBounds.Width, chipBounds.Height) / 2 - 1));

            using GraphicsPath chipPath = Rounded(chipBounds, radius);

            using (var chipBrush = new SolidBrush(palette.ChipFill))
            using (var chipBorder = new Pen(palette.AttachmentBorder, 1F))
            {
                graphics.FillPath(chipBrush, chipPath);
                graphics.DrawPath(chipBorder, chipPath);
            }

            IReadOnlyList<string> lines = entry.AttachmentLines;
            int lineY = chipBounds.Y + ChipPadY;

            foreach (string line in lines)
            {
                DrawLine(graphics, line, _chipFont, chipBounds.X + ChipPadX, lineY, palette.ChipText);
                lineY += _chipFont.Height;

                if (lineY >= chipBounds.Bottom - ChipPadY)
                {
                    break;
                }
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

            Color bodyText = SkinTheme.EnsureReadable(
                dark ? Color.FromArgb(228, 233, 241) : Color.FromArgb(28, 36, 48),
                canvas);

            Color metaText = SkinTheme.EnsureReadable(
                SkinTheme.Blend(bodyText, canvas, dark ? 0.34F : 0.44F),
                canvas,
                3.6);

            Color dayPill = dark
                ? SkinTheme.Blend(canvas, Color.White, 0.12F)
                : SkinTheme.Blend(canvas, Color.White, 0.75F);

            return new Palette(
                Canvas: canvas,
                BodyText: bodyText,
                SubjectText: bodyText,
                MetaText: metaText,
                ReadText: SkinTheme.Blend(metaText, canvas, 0.55F),
                TickMuted: SkinTheme.EnsureReadable(
                    SkinTheme.Blend(bodyText, canvas, dark ? 0.34F : 0.44F),
                    canvas,
                    3.6),
                TickRead: SkinTheme.EnsureReadable(Color.FromArgb(37, 119, 252), canvas, 3.6),
                AttachmentBorder: SkinTheme.Blend(bodyText, canvas, dark ? 0.55F : 0.66F),
                ChipFill: SkinTheme.Blend(canvas, bodyText, dark ? 0.10F : 0.06F),
                ChipText: metaText,
                Hover: SkinTheme.Blend(canvas, bodyText, dark ? 0.14F : 0.07F),
                DayPill: dayPill,
                DayText: SkinTheme.EnsureReadable(SkinTheme.MutedText(dayPill), dayPill),
                EmptyText: SkinTheme.EnsureReadable(SkinTheme.MutedText(canvas), canvas));
        }

        // ----------------------------------------------------------------
        // Yardımcılar
        // ----------------------------------------------------------------

        /// <summary>
        /// Durum tiki. WhatsApp ile aynı anlamı taşır: tek tik gönderildi,
        /// çift tik teslim edildi/okundu. Tiksiz mesajlarda (karşı taraftan
        /// gelen) durum satırı yalnızca saati taşır.
        /// </summary>
        private static string TickOf(MessageDto message)
        {
            if (!message.SentByCurrentUser)
            {
                return string.Empty;
            }

            if (message.IsRead)
            {
                return "\u2713\u2713";
            }

            return message.IsDelivered
                ? "\u2713\u2713"
                : "\u2713";
        }

        /// <summary>
        /// Alt durum satırının parçaları: saat her zaman, "Okundu" yalnızca
        /// okunmuş kendi mesajlarında, "düzenlendi" ise düzenleme damgası
        /// taşıyan mesajlarda görünür.
        /// </summary>
        private static List<(string Text, bool Soft)> MetaParts(MessageDto message)
        {
            var parts = new List<(string Text, bool Soft)>
            {
                (TimeOf(message), false)
            };

            if (message.SentByCurrentUser && message.IsRead)
            {
                parts.Add((" \u00b7 Okundu", true));
            }

            if (message.EditedAt is not null)
            {
                parts.Add((" \u00b7 d\u00fczenlendi", true));
            }

            return parts;
        }

        private static string TimeOf(MessageDto message)
            => message.SentAt.ToLocalTime().ToString("HH:mm", CultureInfo.CurrentCulture);

        /// <summary>Metni verilen genişliğe göre satırlara böler.</summary>
        private static List<string> WrapLines(string text, Font font, int maxWidth)
        {
            var lines = new List<string>();

            foreach (string paragraph in text.Replace("\r\n", "\n").Split('\n'))
            {
                var words = new List<string>();

                foreach (string word in paragraph.Split(' ', StringSplitOptions.RemoveEmptyEntries))
                {
                    if (Measure(word, font) <= maxWidth)
                    {
                        words.Add(word);
                    }
                    else
                    {
                        words.AddRange(BreakWord(word, font, maxWidth));
                    }
                }

                if (words.Count == 0)
                {
                    lines.Add(string.Empty);
                    continue;
                }

                string current = words[0];

                for (int i = 1; i < words.Count; i++)
                {
                    string candidate = current + " " + words[i];

                    if (Measure(candidate, font) <= maxWidth)
                    {
                        current = candidate;
                    }
                    else
                    {
                        lines.Add(current);
                        current = words[i];
                    }
                }

                lines.Add(current);
            }

            return lines;
        }

        /// <summary>Sığmayan tek bir kelimeyi karakter karakter parçalara böler.</summary>
        private static List<string> BreakWord(string word, Font font, int maxWidth)
        {
            var chunks = new List<string>();
            string current = string.Empty;

            foreach (char c in word)
            {
                string candidate = current + c;

                if (current.Length > 0 && Measure(candidate, font) > maxWidth)
                {
                    chunks.Add(current);
                    current = c.ToString();
                }
                else
                {
                    current = candidate;
                }
            }

            if (current.Length > 0)
            {
                chunks.Add(current);
            }

            return chunks;
        }

        /// <summary>Tek satırın piksel genişliği (iç paylar hariç).</summary>
        private static int Measure(string text, Font font)
            => text.Length == 0
                ? 0
                : TextRenderer.MeasureText(
                    text,
                    font,
                    new Size(int.MaxValue, int.MaxValue),
                    TextFormatFlags.NoPadding | TextFormatFlags.SingleLine | TextFormatFlags.NoPrefix).Width;

        /// <summary>
        /// Resim ekini önbellekten okur. Dosya yok/açılamaz ise <c>null</c>
        /// döner ve çizerken çip'e düşülür. Önbellek panel ömrü kadardır ve
        /// belirli bir sayıyı aşınca boşaltılır.
        /// </summary>
        private Bitmap? GetAttachmentImage(MessageDto message)
        {
            string key = message.AttachmentPath ?? string.Empty;

            if (key.Length == 0)
            {
                return null;
            }

            if (_attachmentImages.TryGetValue(key, out Bitmap? cached))
            {
                return cached;
            }

            if (_attachmentImages.Count >= MaxCachedImages)
            {
                foreach (Bitmap? stale in _attachmentImages.Values)
                {
                    stale?.Dispose();
                }

                _attachmentImages.Clear();
            }

            Bitmap? bitmap = null;
            string? fullPath = message.AttachmentFullPath;

            if (!string.IsNullOrWhiteSpace(fullPath))
            {
                try
                {
                    if (File.Exists(fullPath))
                    {
                        // Image.FromFile dosyayı kilitler; kopyaya alınıp
                        // önbelleğe öyle konur ve kilit hemen bırakılır.
                        using Image original = Image.FromFile(fullPath);
                        bitmap = ScaleToFit(original, 960, 600);
                    }
                }
                catch
                {
                    // Bozuk ya da erişilemeyen resim uygulamayı düşürmez;
                    // çip olarak gösterilir.
                    bitmap = null;
                }
            }

            _attachmentImages[key] = bitmap;
            return bitmap;
        }

        /// <summary>
        /// Görseli verilen kutuya sığdırıp küçültülmüş bir kopya üretir;
        /// bellekte orijinal boyutla (25 MB'a kadar fotoğraf) durmaz.
        /// </summary>
        private static Bitmap ScaleToFit(Image image, int maxWidth, int maxHeight)
        {
            double scale = Math.Min(
                1.0,
                Math.Min((double)maxWidth / image.Width, (double)maxHeight / image.Height));

            int width = Math.Max(1, (int)(image.Width * scale));
            int height = Math.Max(1, (int)(image.Height * scale));

            var target = new Bitmap(width, height);

            using Graphics graphics = Graphics.FromImage(target);
            graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
            graphics.DrawImage(image, 0, 0, width, height);

            return target;
        }

        /// <summary>Çip metni: dosya adı ve boyutu, örn. "rapor.pdf (1,2 MB)".</summary>
        internal static string ChipTextOf(MessageDto message)
        {
            string name = string.IsNullOrWhiteSpace(message.AttachmentFileName)
                ? "Dosya"
                : message.AttachmentFileName;

            string size = message.AttachmentSize is { } bytes
                ? $" ({FormatSize(bytes)})"
                : string.Empty;

            return name + size;
        }

        /// <summary>İnsan okunur dosya boyutu (1,2 MB / 45,3 KB / 320 B).</summary>
        internal static string FormatSize(long bytes)
        {
            if (bytes >= 1024L * 1024)
            {
                return $"{(bytes / (1024d * 1024)).ToString("N1", CultureInfo.CurrentCulture)} MB";
            }

            if (bytes >= 1024)
            {
                return $"{(bytes / 1024d).ToString("N1", CultureInfo.CurrentCulture)} KB";
            }

            return $"{bytes} B";
        }

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

        /// <summary>
        /// Eke tıklanınca dosya açma isteği sahibi forma iletilir; panel
        /// kendisi dosya açmaz (yol çözümü, hata bildirimi ve yetki oradadır).
        /// </summary>
        protected override void OnMouseClick(MouseEventArgs e)
        {
            base.OnMouseClick(e);

            if (e.Button != MouseButtons.Left)
            {
                return;
            }

            if (HitAttachment(e.Location)?.Message is { } message)
            {
                AttachmentOpenRequested?.Invoke(message);
            }
        }

        /// <summary>Sağ tık, kendi mesaj üzerinde düzenle/sil menüsünü açar.</summary>
        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);

            if (e.Button != MouseButtons.Right)
            {
                return;
            }

            BubbleEntry? entry = HitMessage(e.Location);

            if (entry?.Message is not { } message)
            {
                return;
            }

            if (!message.SentByCurrentUser || message.IsAnnouncement)
            {
                return;
            }

            ShowMessageMenu(entry, e.Location);
        }

        /// <summary>İmleç ek üzerindeyken el imleci; tıklamanın açılabilir olduğunu gösterir.</summary>
        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            BubbleEntry? entry = HitMessage(e.Location);
            Guid? hovered = entry?.Message?.Id;

            if (hovered != _hoveredId)
            {
                _hoveredId = hovered;
                Invalidate();
            }

            Cursor = HitAttachment(e.Location) is not null ? Cursors.Hand : Cursors.Default;
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);

            if (_hoveredId is not null)
            {
                _hoveredId = null;
                Invalidate();
            }
        }

        /// <summary>Kendi mesajı için düzenle/sil bağlam menüsünü gösterir.</summary>
        private void ShowMessageMenu(BubbleEntry entry, Point location)
        {
            MessageDto message = entry.Message!;

            var menu = new DXPopupMenu();

            menu.Items.Add(new DXMenuItem(
                "Düzenle",
                (_, _) => EditRequested?.Invoke(message),
                DxIcon.Edit,
                DXMenuItemPriority.Normal));

            menu.Items.Add(new DXMenuItem(
                "Sil",
                (_, _) => DeleteRequested?.Invoke(message),
                DxIcon.Delete,
                DXMenuItemPriority.Normal));

            menu.CloseUp += (_, _) => DisposeMenu(menu);

            menu.ShowPopup(this, location);
        }

        /// <summary>
        /// Menü kapandıktan sonra serbest bırakılır; <c>CloseUp</c> olayı
        /// hâlâ kapanış sırasında tetiklendiği için iş bir sonraki mesaj
        /// döngüsüne ertelenir.
        /// </summary>
        private void DisposeMenu(DXPopupMenu menu)
        {
            try
            {
                if (IsHandleCreated && !IsDisposed)
                {
                    BeginInvoke(new Action(menu.Dispose));
                    return;
                }
            }
            catch (InvalidOperationException)
            {
                // Handle kapanmış olabilir; doğrudan bırakılır.
            }

            menu.Dispose();
        }

        /// <summary>Tıklama/imleç konumunun altında kalan ek kaydı (yoksa <c>null</c>).</summary>
        private BubbleEntry? HitAttachment(Point clientPoint)
        {
            // AutoScrollPosition negatiftir; içerik koordinatına çevirmek için
            // konumdan çıkarılır.
            var location = new Point(
                clientPoint.X - AutoScrollPosition.X,
                clientPoint.Y - AutoScrollPosition.Y);

            for (int i = _entries.Count - 1; i >= 0; i--)
            {
                BubbleEntry entry = _entries[i];

                if (entry.AttachmentRectangle != Rectangle.Empty
                    && entry.AttachmentRectangle.Contains(location))
                {
                    return entry;
                }
            }

            return null;
        }

        /// <summary>Tıklama/imleç konumunun altında kalan mesaj bloğu (yoksa <c>null</c>).</summary>
        private BubbleEntry? HitMessage(Point clientPoint)
        {
            var location = new Point(
                clientPoint.X - AutoScrollPosition.X,
                clientPoint.Y - AutoScrollPosition.Y);

            for (int i = _entries.Count - 1; i >= 0; i--)
            {
                BubbleEntry entry = _entries[i];

                if (entry.Message is not null && entry.Bounds.Contains(location))
                {
                    return entry;
                }
            }

            return null;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _bodyFont.Dispose();
                _subjectFont.Dispose();
                _metaFont.Dispose();
                _tickFont.Dispose();
                _dayFont.Dispose();
                _emptyFont.Dispose();
                _chipFont.Dispose();

                foreach (Bitmap? bitmap in _attachmentImages.Values)
                {
                    bitmap?.Dispose();
                }

                _attachmentImages.Clear();
            }

            base.Dispose(disposing);
        }

        // ----------------------------------------------------------------
        // Yerleşim kayıtları
        // ----------------------------------------------------------------

        /// <summary>Yerleşimde hesaplanmış bir öğe: mesaj bloğu ya da gün ayırıcı.</summary>
        private sealed class BubbleEntry
        {
            public MessageDto? Message { get; init; }

            /// <summary>Gün ayırıcı metni; mesaj bloklarında boştur.</summary>
            public string? DayLabel { get; init; }

            public Rectangle Bounds { get; init; }

            public IReadOnlyList<string> SubjectLines { get; init; } = [];

            public int SubjectTop { get; init; }

            /// <summary>Resim/çip bloğu; boş ise ek yoktur.</summary>
            public Rectangle AttachmentRectangle { get; init; }

            /// <summary>Dosya çipinin satırları; resim eklerinde boştur.</summary>
            public IReadOnlyList<string> AttachmentLines { get; init; } = [];

            public IReadOnlyList<string> BodyLines { get; init; } = [];

            public int BodyTop { get; init; }

            /// <summary>Yazının bitimindeki durum tiki; yoksa boş dikdörtgen.</summary>
            public Rectangle TickRectangle { get; init; }

            /// <summary>Alt satırın parçaları (saat, okundu, düzenlendi).</summary>
            public List<(string Text, bool Soft)>? MetaParts { get; init; }

            public Rectangle MetaRectangle { get; init; }
        }

        private sealed record Palette(
            Color Canvas,
            Color BodyText,
            Color SubjectText,
            Color MetaText,
            Color ReadText,
            Color TickMuted,
            Color TickRead,
            Color AttachmentBorder,
            Color ChipFill,
            Color ChipText,
            Color Hover,
            Color DayPill,
            Color DayText,
            Color EmptyText);
    }
}
