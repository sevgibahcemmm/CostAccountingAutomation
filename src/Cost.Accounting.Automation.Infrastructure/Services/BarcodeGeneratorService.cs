#pragma warning disable CA1416
using Cost.Accounting.Automation.Application.Services;
using System.Drawing;
using System.Drawing.Imaging;
using ZXing;
using ZXing.Common;

namespace Cost.Accounting.Automation.Infrastructure.Services;

internal sealed class BarcodeGeneratorService : IBarcodeGeneratorService
{
    public byte[] GenerateEan13Barcode(string gtin)
    {
        ArgumentNullException.ThrowIfNull(gtin);

        if (gtin.Length != 13 || !gtin.All(char.IsDigit))
            throw new ArgumentException("GTIN 13 basamaklı sayısal bir değer olmalıdır.", nameof(gtin));

        var hints = new Dictionary<EncodeHintType, object>
        {
            { EncodeHintType.MARGIN, 2 }
        };

        BitMatrix matrix = new MultiFormatWriter().encode(gtin, BarcodeFormat.EAN_13, 350, 160, hints);

        string humanReadable = $"{gtin[0]} {gtin.Substring(1, 6)} {gtin.Substring(7, 6)}";
        return BarcodeMatrixToPngBytesWithText(matrix, humanReadable);
    }

    public byte[] GenerateQrCode(string data)
    {
        ArgumentNullException.ThrowIfNull(data);

        var hints = new Dictionary<EncodeHintType, object>
        {
            { EncodeHintType.CHARACTER_SET, "UTF-8" },
            { EncodeHintType.MARGIN, 1 }
        };

        BitMatrix matrix = new MultiFormatWriter().encode(data, BarcodeFormat.QR_CODE, 350, 350, hints);
        return MatrixToPngBytes(matrix);
    }

    public string GenerateGtin(string companyPrefix, string productCode)
    {
        ArgumentNullException.ThrowIfNull(companyPrefix);
        ArgumentNullException.ThrowIfNull(productCode);

        // Sadece sayısal haneler kullanılır
        string prefixDigits = new(companyPrefix.Where(char.IsDigit).ToArray());
        string productDigits = new(productCode.Where(char.IsDigit).ToArray());

        if (productDigits.Length == 0)
            throw new ArgumentException("Ürün kodunda sayısal karakter bulunamadı.", nameof(productCode));

        // EAN-13 yapısı: Ülke kodu + Firma kodu (CompanyPrefix) + Ürün kodu + Kontrol rakamı
        // Kontrol rakamı hariç toplam 12 hane olmalıdır.
        if (prefixDigits.Length >= 12)
        {
            string truncated = prefixDigits[^12..];
            return truncated + CalculateEan13CheckDigit(truncated);
        }

        int availableSlots = 12 - prefixDigits.Length;

        string itemReference = productDigits.Length <= availableSlots
            ? productDigits.PadLeft(availableSlots, '0')
            : productDigits[^availableSlots..];

        string base12 = prefixDigits + itemReference;
        return base12 + CalculateEan13CheckDigit(base12);
    }

    private static int CalculateEan13CheckDigit(string code12)
    {
        if (code12.Length != 12)
            throw new ArgumentException("Kod 12 basamaklı olmalıdır.", nameof(code12));

        int sum = 0;
        for (int i = 0; i < 12; i++)
        {
            int digit = code12[i] - '0';
            sum += i % 2 == 0 ? digit : digit * 3;
        }

        return (10 - sum % 10) % 10;
    }

    private static byte[] BarcodeMatrixToPngBytesWithText(BitMatrix matrix, string humanReadableText)
    {
        int barWidth = matrix.Width;
        int barHeight = matrix.Height;
        const int textAreaHeight = 28;

        using Bitmap bitmap = new(barWidth, barHeight + textAreaHeight);
        using Graphics g = Graphics.FromImage(bitmap);
        g.Clear(Color.White);

        for (int x = 0; x < barWidth; x++)
            for (int y = 0; y < barHeight; y++)
                if (matrix[x, y])
                    bitmap.SetPixel(x, y, Color.Black);

        using Font font = new("Consolas", 14f, FontStyle.Regular, GraphicsUnit.Pixel);
        using SolidBrush brush = new(Color.Black);
        SizeF textSize = g.MeasureString(humanReadableText, font);
        float textX = Math.Max(0, (barWidth - textSize.Width) / 2f);
        float textY = barHeight + (textAreaHeight - textSize.Height) / 2f;
        g.DrawString(humanReadableText, font, brush, textX, textY);

        using var ms = new MemoryStream();
        bitmap.Save(ms, ImageFormat.Png);
        return ms.ToArray();
    }

    private static byte[] MatrixToPngBytes(BitMatrix matrix)
    {
        int width = matrix.Width;
        int height = matrix.Height;
        Bitmap bitmap = new(width, height);
        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                bitmap.SetPixel(x, y, matrix[x, y] ? Color.Black : Color.White);

        using var ms = new MemoryStream();
        bitmap.Save(ms, ImageFormat.Png);
        return ms.ToArray();
    }
}