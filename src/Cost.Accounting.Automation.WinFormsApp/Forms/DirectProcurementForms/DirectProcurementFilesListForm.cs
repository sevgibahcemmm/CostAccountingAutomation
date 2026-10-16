using System.Windows.Forms;
using Cost.Accounting.Automation.Application.DirectProcurement;
using Cost.Accounting.Automation.Application.Services;
using Cost.Accounting.Automation.Domain.Abstractions;
using Cost.Accounting.Automation.Domain.DirectProcurement;
using Cost.Accounting.Automation.WinFormsApp.Forms.BaseForm;
using Cost.Accounting.Automation.WinFormsApp.Utils;
using DevExpress.Utils.Svg;
using Microsoft.Extensions.DependencyInjection;
using TS.MediatR;
using TS.Result;

namespace Cost.Accounting.Automation.WinFormsApp.Forms.DirectProcurementForms
{
    /// <summary>
    /// Doğrudan temin dosyalarının ana listesi. Kaynak sistemdeki "Doğrudan
    /// Temin Listesi" ekranının karşılığıdır.
    /// </summary>
    public sealed partial class DirectProcurementFilesListForm
        : CrudListFormBase<DirectProcurementFileGetAllQuery, DirectProcurementFileDto, DirectProcurementFileEditForm>
    {
        public DirectProcurementFilesListForm() : base("Doğrudan Temin Dosyaları")
        {
        }

        protected override SvgImage ModuleIcon => DxIcon.Contract;

        protected override bool SupportsRestore => true;

        protected override void ConfigureColumns() => AddColumnsFromAttributes();

        protected override string DeleteItemLabel => "doğrudan temin dosyası";

        protected override string PendingItemLabel => "dosya";

        protected override string[] SearchFieldNames =>
        [
            nameof(DirectProcurementFileDto.FileNumber),
            nameof(DirectProcurementFileDto.WorkName),
            nameof(DirectProcurementFileDto.SupplierName),
            nameof(DirectProcurementFileDto.ProjectNumber),
            nameof(DirectProcurementFileDto.DecisionNumber)
        ];

        protected override DirectProcurementFileGetAllQuery BuildListQuery()
            => new(OnlyDeleted: ShowDeleted);

        /// <summary>
        /// Taban sınıf düzenleme penceresini <c>Activator</c> ile, yalnızca DTO
        /// alarak kurar. Giriş penceresindeki "Birimi" kutusu Master DB'den
        /// doldurulduğu için arama servisi burada elle geçirilir; aksi hâlde
        /// liste ekranından düzenleme yapıldığında kutu boş gelirdi.
        /// </summary>
        protected override DirectProcurementFileEditForm CreateEditEditor(DirectProcurementFileDto item)
        {
            using var scope = Program.Services.CreateScope();
            IMasterCompanyNameLookup? lookup = scope.ServiceProvider.GetService<IMasterCompanyNameLookup>();
            return new DirectProcurementFileEditForm(item, lookup);
        }

        protected override IRequest<Result<string>> BuildDeleteCommand(DirectProcurementFileDto item)
            => new DirectProcurementFileDeleteCommand(item.Id);

        /// <summary>Seçili dosyalar tek transaction'da silinir.</summary>
        protected override IRequest<Result<string>>? BuildBulkDeleteCommand(IReadOnlyList<DirectProcurementFileDto> items)
            => new BulkDeleteDirectProcurementFilesCommand(items.Select(item => item.Id).ToList());

        protected override IRequest<Result<string>> BuildRestoreCommand(DirectProcurementFileDto item)
            => new DirectProcurementFileRestoreCommand(item.Id);

        protected override string GetDeleteSummary(DirectProcurementFileDto item)
            => $"{item.FileNumber} - {item.WorkName}";

        /// <summary>
        /// Arşivlenen dosya düzenlenemez. Kaynak sistemde de arşiv bayrağı
        /// dosyayı kilitlerdi; liste ekranında bu kuralı görünür kılmak
        /// "Düzenle" düğmesini tek tek devre dışı bırakmaktan daha anlaşılırdır.
        /// </summary>
        protected override bool AllowsEdit(DirectProcurementFileDto item) => !item.IsArchived;

        /// <summary>
        /// Teklif veya kazanan firma kaydı bulunan dosyalar silinemez: bu kayıtlar
        /// dosyanın mali izidir. Denetim tek sorguda yapılır.
        /// </summary>
        protected override async Task<List<DirectProcurementFileDto>> GetUndeletableAsync(
            List<DirectProcurementFileDto> selected,
            CancellationToken cancellationToken)
        {
            if (selected.Count == 0)
            {
                return [];
            }

            using var scope = Program.Services.CreateScope();
            IDirectProcurementFileRepository repository =
                scope.ServiceProvider.GetRequiredService<IDirectProcurementFileRepository>();

            DeletionCheck check = await repository.GetDeletionCheckAsync(
                selected.Select(item => item.Id).ToList(),
                cancellationToken);

            if (check.HasMovement)
            {
                return selected.Where(item => check.MovementIds.Contains(item.Id)).ToList();
            }

            return [];
        }
    }
}