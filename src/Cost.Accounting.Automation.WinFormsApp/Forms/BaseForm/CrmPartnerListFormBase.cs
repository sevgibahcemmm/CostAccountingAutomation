using Cost.Accounting.Automation.Domain.Abstractions;
using Cost.Accounting.Automation.WinFormsApp.Utils;
using DevExpress.XtraEditors;
using TS.MediatR;
using TS.Result;

namespace Cost.Accounting.Automation.WinFormsApp.Forms.BaseForm
{
    /// <summary>
    /// Müşteri ve tedarikçi listelerinin ortak tabanıdır.
    ///
    /// <para>
    /// Cari hareket denetimi kayıt başına ayrı sorgu atmak yerine seçimin
    /// tamamını tek sorguda yapar. Seçim büyüklüğü arttıkça sorgu sayısı
    /// değişmez.
    /// </para>
    /// </summary>
    public abstract partial class CrmPartnerListFormBase<TListQuery, TDto, TEditForm>
        : CrudListFormBase<TListQuery, TDto, TEditForm>
        where TListQuery : class, IRequest<IQueryable<TDto>>, new()
        where TDto : EntityDto
        where TEditForm : XtraForm
    {
        protected CrmPartnerListFormBase(string formTitle) : base(formTitle)
        {
        }

        protected override void ConfigureColumns() => AddColumnsFromAttributes();

        protected override bool SupportsRestore => true;

        /// <summary>
        /// Seçili partner kimlikleri için cari hareket görmüş olanları döndürür.
        /// Uygulama tek sorguda çalışır.
        /// </summary>
        protected abstract Task<HashSet<Guid>> GetMovementBlockedIdsAsync(
            IReadOnlyCollection<Guid> partnerIds,
            CancellationToken cancellationToken);

        protected override async Task<List<TDto>> GetUndeletableAsync(
            List<TDto> selected, CancellationToken cancellationToken)
        {
            if (selected.Count == 0)
            {
                return [];
            }

            HashSet<Guid> partnerIds = selected.Select(item => item.Id).ToHashSet();
            HashSet<Guid> blockedIds = await GetMovementBlockedIdsAsync(partnerIds, cancellationToken);

            if (blockedIds.Count == 0)
            {
                return [];
            }

            return selected.Where(item => blockedIds.Contains(item.Id)).ToList();
        }
    }
}