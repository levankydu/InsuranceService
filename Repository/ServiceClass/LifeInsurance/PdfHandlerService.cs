using PdfSharpCore.Pdf.IO;
using PdfSharpCore.Pdf.Security;
using PuppeteerSharp;
using PuppeteerSharp.Media;
using test0000001.Models.DTO.LifeInsurance;

namespace test0000001.Repository.ServiceClass.LifeInsurance
{
    public class PdfHandlerService
    {
        public PdfHandlerService()
        {

        }

        public async Task<string?> HtmlToPdf(ContractContent pdfContent, string fileName)
        {
            try
            {
                var pdfOptions = new PdfOptions()
                {
                    Format = PaperFormat.A4,
                    PrintBackground = true,
                    DisplayHeaderFooter = true,
                    HeaderTemplate = pdfContent.Header,
                    FooterTemplate = pdfContent.Footer,
                    MarginOptions = new MarginOptions
                    {
                        Top = "100px",
                        Bottom = "100px"
                    }
                };

                string outputPath = Path.Combine(Directory.GetCurrentDirectory(), @"wwwroot/", fileName);
                List<string> cssFiles = new List<string>
                {
                    "HomeLibrary/css/bootstrap.min.css",
                    "HomeLibrary/css/style.css"
                };
                var cssPaths = cssFiles.Select(m => Path.Combine(Directory.GetCurrentDirectory(), @"wwwroot/", m));

                using (var browser = await Puppeteer.LaunchAsync(new LaunchOptions
                {
                    Headless = true,
                    //ExecutablePath = "C:/Program Files/Google/Chrome/Application/chrome.exe"
                    ExecutablePath = "C:/Program Files (x86)/Microsoft/Edge/Application/msedge.exe"
                }))
                {
                    using (var page = await browser.NewPageAsync())
                    {

                        await page.SetContentAsync(pdfContent.Body);
                        /*var ele = await page.QuerySelectorAsync(".my-page-cover-img");
                        await ele.EvaluateFunctionAsync<string>($"ele => ele.setAttribute('src', 'https://imgur.com/Xa5ttrV.jpg')");*/
                        foreach (var css in cssPaths)
                        {
                            await page.AddStyleTagAsync(new AddTagOptions { Path = css });
                        }

                        await page.PdfAsync(outputPath, pdfOptions);

                        return outputPath;
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public string AddPassword(string filePath, string password)
        {
            try
            {
                // Open pdf document
                var doc = PdfReader.Open(filePath);
                var securities = doc.SecuritySettings;

                // set document passwords
                securities.OwnerPassword = "InsuranceService@321";
                securities.UserPassword = password;

                //set document permissions
                securities.PermitAccessibilityExtractContent = false;
                securities.PermitAnnotations = false;
                securities.PermitAssembleDocument = false;
                securities.PermitExtractContent = false;
                securities.PermitFormsFill = true;
                securities.PermitFullQualityPrint = true;
                securities.PermitModifyDocument = false;
                securities.PermitPrint = true;

                // save pdf document and close
                var newFilePath = filePath.Replace(".pdf", "_approved.pdf");
                doc.Save(newFilePath);
                doc.Close();

                // Remove old pdf file
                FileInfo file = new FileInfo(filePath);
                if (file.Exists) file.Delete();
                else throw new Exception("File Not found.");

                return newFilePath;
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
