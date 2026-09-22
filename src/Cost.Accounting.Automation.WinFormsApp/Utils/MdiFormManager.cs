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
            return OpenForm(parentForm, null, factory);
        }

        private static string GetDefaultTitle<TForm>() where TForm : XtraForm
        {
            try
            {
                using TForm temp = Activator.CreateInstance<TForm>()!;
                return string.IsNullOrEmpty(temp.Text) ? typeof(TForm).Name : temp.Text;
            }
            catch
            {
                return typeof(TForm).Name;
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