using System.Drawing;
using Cost.Accounting.Automation.Domain.Abstractions;
using DevExpress.XtraGrid.Views.Base;
using DevExpress.XtraGrid.Views.Grid;

namespace Cost.Accounting.Automation.WinFormsApp.Utils;

/// <summary>
/// Taslak/onaylı satır boyamasının tek uygulaması.
///
/// <para>
/// Liste ekranlarındaki renk kuralı burada toplanır; renklerin skin'e uyumu,
/// okunabilirlik ve temaya bağlı seçeneklerin (Options) doğru ayarlanması tek
/// yerden denetlenir. Ekran kodu yalnızca <see cref="Apply"/> çağırır.
/// </para>
/// </summary>
public static class StatusRowPainter
{
    /// <summary>
    /// Onaylı satır zemini için aksan rengi karışım oranı. Düşük tutulur:
    /// onaylı kayıt normal akışın bir parçasıdır, dikkat çekmesi gerekmez.
    /// </summary>
    public const float ApprovedTint = 0.10F;

    /// <summary>
    /// Taslak satır zemini için aksan rengi karışım oranı. Onaylıdan yüksek
    /// tutulur: gözden geçirilecek kayıt ilk bakışta ayırt edilmelidir.
    /// </summary>
    public const float DraftTint = 0.20F;

    private static readonly Font ApprovedFont = new("Segoe UI", 9F);
    private static readonly Font DraftFont = new("Segoe UI", 9F, FontStyle.Italic);

    /// <summary>Onaylı kayıt satırının zemin rengi.</summary>
    public static Color ApprovedRowColor(Color surface)
        => SkinTheme.Blend(surface, SkinTheme.Success, ApprovedTint);

    /// <summary>Taslak kayıt satırının zemin rengi.</summary>
    public static Color DraftRowColor(Color surface)
        => SkinTheme.Blend(surface, SkinTheme.Warning, DraftTint);

    /// <summary>Duruma göre satır fontu (taslak italiktir).</summary>
    public static Font FontFor(bool isApproved) => isApproved ? ApprovedFont : DraftFont;

    /// <summary>
    /// Satırın görünümüne durum rengini ve fontunu uygular.
    /// </summary>
    public static void Apply(RowStyleEventArgs e, IApprovalStatusDto status, Color approvedColor, Color draftColor)
    {
        Color backColor = status.IsApproved ? approvedColor : draftColor;

        e.Appearance.BackColor = backColor;

        // EnableAppearanceEvenRow/OddRow açık olduğu için şeritli görünüm
        // bozulmasın diye ikisi de yazılmalıdır.
        e.Appearance.BackColor2 = backColor;

        // KRİTİK: DevExpress'te bir Appearance değeri atamak tek başına
        // yeterli değildir; hangi özelliklerin kullanılacağını belirleyen
        // Options bayrakları da açılmalıdır. Aksi halde atanan renk ve font
        // cizilirken yok sayılır ve satır tema renginde kalır.
        e.Appearance.Options.UseBackColor = true;
        e.Appearance.Options.UseFont = true;

        e.Appearance.Font = FontFor(status.IsApproved);

        // Metin rengi bilinçli olarak değiştirilmez: tema metin rengi
        // harmanlanmış zeminde zaten okunaklıdır ve koyu temada elle verilen
        // bir renk kaybolabilir.
    }
}
