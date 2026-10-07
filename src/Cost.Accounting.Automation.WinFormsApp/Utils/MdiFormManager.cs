using DevExpress.Utils;
using DevExpress.XtraBars;
using DevExpress.XtraEditors;
using DevExpress.XtraTab;
using DevExpress.XtraTab.ViewInfo;
using DevExpress.XtraTabbedMdi;
using Cost.Accounting.Automation.WinFormsApp.Tools;
using System.Drawing;
using System.Drawing.Drawing2D;
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
        {
            if (_mdiManager is null)
            {
                throw new InvalidOperationException("MdiFormManager.Initialize çağrılmadı.");
            }

            string key = formTitle ?? GetDefaultTitle<TForm>();

            TForm? existing = parentForm.MdiChildren.OfType<TForm>()
                .FirstOrDefault(f => !f.IsDisposed && string.Equals(f.Text, key, StringComparison.Ordinal));
            if (existing is not null)
            {
                ActivateForm(existing);
                return existing;
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