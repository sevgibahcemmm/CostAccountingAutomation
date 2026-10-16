using DevExpress.Utils;
using DevExpress.XtraBars;
using DevExpress.XtraEditors;
using DevExpress.XtraTab;
using DevExpress.XtraTab.ViewInfo;
using DevExpress.XtraTabbedMdi;
using Cost.Accounting.Automation.WinFormsApp.Tools;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.Linq;

namespace Cost.Accounting.Automation.WinFormsApp.Utils
{
    /// <summary>
    /// MDI sekmesinde kapatma (×) düğmesi gösterilmeyecek formlar bu arayüzü
    /// uygular. Örneğin ana Dashboard sekmesi kapatılamaz olmalıdır.
    /// </summary>
    public interface IMdiTabCloseDisabled
    {
    }

    public sealed class MdiFormManager
    {
        private XtraTabbedMdiManager? _mdiManager;

        private static bool _drawingTabHeader;

        private MdiFormManager()
        {
        }

        public static MdiFormManager Instance { get; } = new();

        public void Initialize(XtraTabbedMdiManager mdiManager)
        {
            if (_mdiManager is not null)
            {
                _mdiManager.PageAdded -= OnPageAdded;
                _mdiManager.CustomDrawTabHeader -= OnCustomDrawTabHeader;
            }

            _mdiManager = mdiManager;
            _mdiManager.PageAdded += OnPageAdded;
            _mdiManager.CustomDrawTabHeader += OnCustomDrawTabHeader;
            ConfigureTabHeader();
        }

        /// <summary>
        /// Sekme başlıklarının yazı tipini ve ikon boyutunu büyütür.
        /// Varsayılan başlık çok küçük kaldığı için okunurluk artırılır.
        /// </summary>
        private void ConfigureTabHeader()
        {
            if (_mdiManager is null)
            {
                return;
            }

            var header = _mdiManager.AppearancePage.Header;
            header.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            header.Options.UseFont = true;

            var headerActive = _mdiManager.AppearancePage.HeaderActive;
            headerActive.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            headerActive.Options.UseFont = true;
        }

        private static void OnPageAdded(object? sender, MdiTabPageEventArgs e)
        {
            if (e.Page.MdiChild is not XtraForm form)
            {
                return;
            }

            // Dashboard gibi kapatılamaz sekmelerde × düğmesi gizlenir; diğer
            // tüm sekmelerde kapatma yalnızca başlıktan yapılır.
            e.Page.ShowCloseButton = form is IMdiTabCloseDisabled
                ? DefaultBoolean.False
                : DefaultBoolean.True;

            CopyIconToPage(form, e.Page);
            void copyHandler(object? _, EventArgs __) => CopyIconToPage(form, e.Page);
            form.Load += copyHandler;
            form.FormClosed += (_, _) => form.Load -= copyHandler;
        }

        /// <summary>
        /// Sekme başlıklarındaki kapatma çarpısını daha büyük ve kutusuz
        /// çizmek için varsayılan çizim devre dışı bırakılıp yerine özel çarpı
        /// çizilir. Böylece buton görünümü kaybolur ve ikon büyür.
        /// </summary>
        private static void OnCustomDrawTabHeader(object? sender, TabHeaderCustomDrawEventArgs e)
        {
            if (_drawingTabHeader || e.TabHeaderInfo is not { } info)
            {
                return;
            }

            _drawingTabHeader = true;
            bool drewHeader;

            try
            {
                info.DisableDrawCloseButton = true;
                e.Painter.Draw(e.ControlInfo);
                drewHeader = true;
            }
            catch (Exception ex)
            {
                CrashLog.WriteException("MdiTabHeader.CustomDraw", ex);
                drewHeader = false;
            }
            finally
            {
                _drawingTabHeader = false;
            }

            if (!drewHeader)
            {
                // Varsayılan çizime bırak (özel çarpı çizilmez).
                return;
            }

            bool showClose = info.Page is not XtraMdiTabPage page
                || page.ShowCloseButton != DefaultBoolean.False;

            if (showClose)
            {
                DrawCloseGlyph(e.Graphics, info);
            }

            e.Handled = true;
        }

        private static void DrawCloseGlyph(Graphics graphics, BaseTabPageViewInfo info)
        {
            Rectangle box = info.ControlBox;
            if (box.Width < 6 || box.Height < 6)
            {
                return;
            }

            Color color = info.PaintAppearance.GetForeColor();
            if (color.IsEmpty)
            {
                color = SystemColors.ControlText;
            }

            int half = Math.Max(3, (Math.Min(box.Width, box.Height) - 3) / 2);
            int cx = box.Left + (box.Width / 2);
            int cy = box.Top + (box.Height / 2);

            SmoothingMode oldMode = graphics.SmoothingMode;
            graphics.SmoothingMode = SmoothingMode.AntiAlias;
            using Pen pen = new(color, 1.8f);
            graphics.DrawLine(pen, cx - half, cy - half, cx + half, cy + half);
            graphics.DrawLine(pen, cx - half, cy + half, cx + half, cy - half);
            graphics.SmoothingMode = oldMode;
        }

        private static void CopyIconToPage(XtraForm form, XtraMdiTabPage page)
        {
            if (form.IconOptions.SvgImage is { } svg)
            {
                page.ImageOptions.SvgImage = svg;
                page.ImageOptions.SvgImageSize = new Size(24, 24);
            }
        }

        /// <summary>
        /// Sekme eklenirken uygulanan görsel ayarları (ikon + kapatma düğmesi)
        /// tekrar uygular. DevExpress bazı durumlarda bu değerleri sekme
        /// oluşturulurken sıfırlayabildiği için form gösterildikten sonra da
        /// güvenceye alınır.
        /// </summary>
        private void ApplyPageChrome(XtraForm form)
        {
            if (_mdiManager is null || form.IsDisposed)
            {
                return;
            }

            XtraMdiTabPage? page = _mdiManager.Pages[form];
            if (page is null)
            {
                return;
            }

            page.ShowCloseButton = form is IMdiTabCloseDisabled
                ? DefaultBoolean.False
                : DefaultBoolean.True;
            CopyIconToPage(form, page);
        }

        public TForm OpenForm<TForm>(XtraForm parentForm, string? formTitle, Func<TForm>? factory = null)
            where TForm : XtraForm
            => OpenForm<TForm>(parentForm, formTitle, factory, reuseExisting: true);

        /// <summary>
        /// MDI sekmesi açar.
        /// </summary>
        /// <param name="parentForm">Asıl MDI kapsayıcısı.</param>
        /// <param name="formTitle">Sekme başlığı; null ise formun kendi başlığı.</param>
        /// <param name="factory">Formu üreten fabrika; null ise varsayılan yapıcı.</param>
        /// <param name="reuseExisting">
        /// <c>true</c> ise aynı tür ve başlığa sahip açık bir sekme varsa yenisi
        /// oluşturulmaz, mevcut sekme öne getirilir. <c>false</c> ise her çağrı
        /// yeni bir sekme üretir.
        /// </param>
        /// <remarks>
        /// Raporlar <c>false</c> ile açılır: aynı rapor iki kez açılabilmeli
        /// (iki dönem karşılaştırmak için) ve sekme başlıkları ayırt edilebilir
        /// kalmalıdır. Liste ekranları <c>true</c> kullanır: aynı kayıt listesi
        /// iki kez açılmamalıdır.
        /// </remarks>
        public TForm OpenForm<TForm>(
            XtraForm parentForm,
            string? formTitle,
            Func<TForm>? factory,
            bool reuseExisting)
            where TForm : XtraForm
        {
            if (_mdiManager is null)
            {
                throw new InvalidOperationException("MdiFormManager.Initialize çağrılmadı.");
            }

            string key = formTitle ?? GetDefaultTitle<TForm>();

            if (reuseExisting)
            {
                TForm? existing = parentForm.MdiChildren.OfType<TForm>()
                    .FirstOrDefault(f => !f.IsDisposed && string.Equals(f.Text, key, StringComparison.Ordinal));
                if (existing is not null)
                {
                    ActivateForm(existing);
                    return existing;
                }
            }

            TForm form = factory?.Invoke() ?? Activator.CreateInstance<TForm>()!;
            ApplyMdiMetadata(form);
            form.MdiParent = parentForm;
            form.Show();
            ApplyPageChrome(form);

            return form;
        }

public TForm OpenForm<TForm>(XtraForm parentForm, Func<TForm>? factory = null)
            where TForm : XtraForm
        {
            return OpenForm<TForm>(parentForm, null, factory);
        }

        /// <summary>
        /// Onay ekranı gibi yalnızca çalışma zamanında bilinen form türlerini açmak
        /// için kullanılır. Aynı pencere zaten açıksa yenisi oluşturulmaz, mevcut
        /// sekme öne getirilir.
        /// </summary>
        public XtraForm OpenForm(XtraForm mdiContainer, Type formType, string? formTitle = null)
        {
            ArgumentNullException.ThrowIfNull(formType);

            if (!typeof(XtraForm).IsAssignableFrom(formType))
            {
                throw new ArgumentException($"{formType.Name} bir XtraForm türü olmalıdır.", nameof(formType));
            }

            if (_mdiManager is null)
            {
                throw new InvalidOperationException("MdiFormManager.Initialize çağrılmadı.");
            }

            string key = formTitle ?? GetDefaultTitle(formType);

            XtraForm? existing = mdiContainer.MdiChildren
                .OfType<XtraForm>()
                .FirstOrDefault(f => !f.IsDisposed
                    && f.GetType() == formType
                    && string.Equals(f.Text, key, StringComparison.Ordinal));
            if (existing is not null)
            {
                ActivateForm(existing);
                return existing;
            }

            XtraForm form = (XtraForm)Activator.CreateInstance(formType)!;
            ApplyMdiMetadata(form);

            // MDI çocuk form başka bir MDI çocuğu doğuramaz; bu yüzden daima asıl
            // MDI kapsayıcı hedeflenir.
            form.MdiParent = mdiContainer;
            form.Show();
            ApplyPageChrome(form);

            return form;
        }

        /// <summary>
        /// Zaten oluşturulmuş bir formu MDI sekmesi olarak açar.
        /// </summary>
        /// <param name="form">
        /// Açılacak form. DevExpress'in kendi pencere sınıfları
        /// (<c>PrintPreviewRibbonForm</c> gibi) yalnızca kendi yazdırma aracı
        /// üzerinden üretilebildiği için bu aşama zorunludur.
        /// </param>
        /// <param name="container">Asıl MDI kapsayıcısı.</param>
        /// <param name="title">Sekme başlığı.</param>
        /// <remarks>
        /// Rapor önizlemesi bu yolu kullanır: kullanıcı, <c>ShowRibbonPreviewDialog</c>
        /// ile gördüğü pencerenin aynısını sekme olarak görür — tüm Türkçe
        /// menüler, kaydetme, yakınlaştırma ve yazdırma komutları aynen çalışır.
        /// </remarks>
        public TForm OpenExistingForm<TForm>(TForm form, XtraForm container, string title)
            where TForm : XtraForm
        {
            ArgumentNullException.ThrowIfNull(form);
            ArgumentNullException.ThrowIfNull(container);

            if (_mdiManager is null)
            {
                throw new InvalidOperationException("MdiFormManager.Initialize çağrılmadı.");
            }

            form.Text = title;

            ApplyMdiMetadata(form);

            // MDI çocuk form başka bir MDI çocuğu doğuramaz; bu yüzden daima asıl
            // MDI kapsayıcı hedeflenir.
            form.MdiParent = container;
            form.Show();
            ApplyPageChrome(form);

            return form;
        }

        /// <summary>
        /// Aynı belgeden ikinci kez açılışta sekme başlığına sıra numarası ekler.
        /// </summary>
        /// <param name="container">MDI kapsayıcısı.</param>
        /// <param name="baseTitle">Sırasız taban başlık.</param>
        /// <returns>
        /// Sırasız başlık varsa onun kendisi ("Liste"); yoksa en küçük
        /// <b>boş</b> numara ("Liste (2)", "Liste (3)"...).
        /// </returns>
        /// <remarks>
        /// <para>
        /// Aynı rapor defalarca açılabilir (iki dönem karşılaştırmak gibi).
        /// Sırasız başlıklar kullanıcıyı hangi sekmenin hangisi olduğunu
        /// ayırmakta zorlayacağı için numara verilir. İlk sekmede numara
        /// gösterilmez: her rapor ilk açılışta da "(1)" taşıması asıl belgeyi
        /// gizlerdi.
        /// </para>
        /// <para>
        /// <b>Neden "say + 1" değil:</b> Ortadaki bir sekme kapatılırsa
        /// numaralar boşluklu kalır. Örneğin açık sekmeler
        /// <c>[Liste, Liste (3)]</c> iken "say + 1" yöntemi yeni sekmeye yine
        /// "(3)" verir ve iki sekme aynı adı taşır. Kullanılmayan en küçük
        /// numara seçildiği için adlar her zaman benzersiz kalır.
        /// </para>
        /// </remarks>
        public static string NextTabTitle(XtraForm container, string baseTitle)
        {
            ArgumentNullException.ThrowIfNull(container);

            if (string.IsNullOrWhiteSpace(baseTitle))
            {
                return baseTitle;
            }

            bool hasPlain = false;
            var used = new HashSet<int>();

            foreach (Form child in container.MdiChildren)
            {
                if (child.IsDisposed
                    || !string.Equals(StripCounter(child.Text), baseTitle, StringComparison.Ordinal))
                {
                    continue;
                }

                if (TryReadCounter(child.Text, out int number))
                {
                    used.Add(number);
                }
                else
                {
                    hasPlain = true;
                }
            }

            // Bu belgeden hiç sekme yok: yeni sekme numarasız açılır.
            if (!hasPlain && used.Count == 0)
            {
                return baseTitle;
            }

            // En küçük boş numara seçilir. "say + 1" yerine bunu seçmek,
            // ortadaki bir sekme kapatıldığında iki sekmenin aynı adı taşımasını
            // (örn. [Liste, Liste (3)] iken yeni "(3)" olması) engeller.
            int candidate = 2;
            while (used.Contains(candidate))
            {
                candidate++;
            }

            return $"{baseTitle} ({candidate})";
        }

        /// <summary>
        /// Başlıktaki sıra numarası ekini kaldırır: "Liste (3)" -> "Liste".
        /// </summary>
        private static string StripCounter(string? text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return string.Empty;
            }

            if (!text.EndsWith(')'))
            {
                return text;
            }

            int open = text.LastIndexOf(" (", StringComparison.Ordinal);

            if (open < 0)
            {
                return text;
            }

            string digits = text[(open + 2)..^1];

            // Yalnızca tamamen rakam olan ekler soyulur. Belgenin kendi adında
            // parantez varsa ("Mamül Maliyet (2026) Baskı Önizleme") bozulmaz.
            return digits.Length > 0 && digits.All(char.IsAsciiDigit)
                ? text[..open]
                : text;
        }

        /// <summary>Başlıktaki sıra numarasını okur; yoksa <c>false</c> döner.</summary>
        private static bool TryReadCounter(string? text, out int number)
        {
            number = 0;

            if (string.IsNullOrEmpty(text) || !text.EndsWith(')'))
            {
                return false;
            }

            int open = text.LastIndexOf(" (", StringComparison.Ordinal);

            if (open < 0)
            {
                return false;
            }

            return int.TryParse(
                text.AsSpan(open + 2, text.Length - open - 3),
                NumberStyles.None,
                CultureInfo.InvariantCulture,
                out number);
        }

        private static string GetDefaultTitle<TForm>() where TForm : XtraForm
        {
            return GetDefaultTitle(typeof(TForm));
        }

        private static string GetDefaultTitle(Type formType)
        {
            try
            {
                using var temp = (XtraForm)Activator.CreateInstance(formType)!;
                return string.IsNullOrEmpty(temp.Text) ? formType.Name : temp.Text;
            }
            catch
            {
                return formType.Name;
            }
        }

        private static void ApplyMdiMetadata(XtraForm form)
        {
            // MDI çocuğun kendi kenarlığı/başlığı gösterilmez; sekme başlığı
            // XtraTabbedMdiManager tarafından çizilir.
            form.FormBorderStyle = FormBorderStyle.None;

            // ControlBox AÇIK kalmalıdır: sekme başlığındaki kapatma (×)
            // düğmesi, doğrudan MDI çocuğunun ControlBox'ına bağlıdır.
            // ControlBox = false yapıldığında DevExpress sekmedeki × düğmesini
            // hiç göstermez. Çocuğun kendi X'i zaten tabbed MDI'da çizilmez.
            form.ControlBox = true;
            form.MaximizeBox = false;
            form.MinimizeBox = false;
        }

        private void ActivateForm(XtraForm form)
        {
            XtraTabbedMdiManager? manager = _mdiManager;
            form.Activate();

            if (manager is null)
            {
                return;
            }

            foreach (DevExpress.XtraTabbedMdi.XtraMdiTabPage tabPage in manager.Pages)
            {
                if (ReferenceEquals(tabPage.MdiChild, form))
                {
                    manager.SelectedPage = tabPage;
                    break;
                }
            }
        }
    }
}