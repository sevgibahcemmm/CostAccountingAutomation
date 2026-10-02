using DevExpress.XtraBars;
using DevExpress.XtraEditors;
using DevExpress.XtraTabbedMdi;
using System.Linq;

namespace Cost.Accounting.Automation.WinFormsApp.Utils
{
    public sealed class MdiFormManager
    {
        private XtraTabbedMdiManager? _mdiManager;

        private MdiFormManager()
        {
        }

        public static MdiFormManager Instance { get; } = new();

        public void Initialize(XtraTabbedMdiManager mdiManager)
        {
            if (_mdiManager is not null)
            {
                _mdiManager.PageAdded -= OnPageAdded;
            }

            _mdiManager = mdiManager;
            _mdiManager.PageAdded += OnPageAdded;
        }

        private static void OnPageAdded(object? sender, MdiTabPageEventArgs e)
        {
            if (e.Page.MdiChild is not XtraForm form)
            {
                return;
            }

            CopyIconToPage(form, e.Page);
            void copyHandler(object? _, EventArgs __) => CopyIconToPage(form, e.Page);
            form.Load += copyHandler;
            form.FormClosed += (_, _) => form.Load -= copyHandler;
        }

        private static void CopyIconToPage(XtraForm form, XtraMdiTabPage page)
        {
            if (form.IconOptions.SvgImage is { } svg)
            {
                page.ImageOptions.SvgImage = svg;
            }
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
            form.FormBorderStyle = FormBorderStyle.None;
            form.ControlBox = false;
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