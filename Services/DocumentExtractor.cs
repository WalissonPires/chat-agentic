using System.Text;
using DocumentFormat.OpenXml.Packaging;
using UglyToad.PdfPig;

namespace ChatAgentic.Services
{
    public class DocumentExtractor
    {
        public async Task<string> ExtractTextAsync(string filename, Stream file)
        {
            var ext = Path.GetExtension(filename).ToLowerInvariant();

            return ext switch
            {
                ".txt" => await ExtractTxt(file),
                ".csv" => await ExtractTxt(file),
                ".md" => await ExtractTxt(file),
                ".reg1" => await ExtractTxt(file),
                ".json" => await ExtractTxt(file),
                ".pdf" => await ExtractPdf(file),
                ".docx" => await ExtractDocx(file),
                _ => throw new Exception($"Formato não suportado: {ext}")
            };
        }

        private async Task<string> ExtractTxt(Stream file)
        {
            using var reader = new StreamReader(file);
            return await reader.ReadToEndAsync();
        }

        private async Task<string> ExtractPdf(Stream file)
        {
            var sb = new StringBuilder();

            using var pdf = PdfDocument.Open(file);
            foreach (var page in pdf.GetPages())
            {
                sb.AppendLine(page.Text);
            }

            return sb.ToString();
        }

        private async Task<string> ExtractDocx(Stream file)
        {
            using var doc = WordprocessingDocument.Open(file, false);
            return doc.MainDocumentPart?.Document?.Body?.InnerText ?? string.Empty;
        }
    }
}