using System.Text.RegularExpressions;

namespace test0000001.Repository.ServiceClass.LifeInsurance
{
    public class FileHandlerService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        
        public FileHandlerService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<string> UploadFile(IFormFile file, string url, bool isTemporary = false)
        {
            string fileName = FileNameBuilder(file);
            string fileUrl = isTemporary ? "temp/" + url : url;
            string filePath = Path.Combine(Directory.GetCurrentDirectory(), @"wwwroot/", fileUrl, fileName);
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }
            return fileUrl + fileName;
        }

        public string? MoveTempFile(string fileName)
        {
            try
            {
                string filePath = Path.Combine(Directory.GetCurrentDirectory(), @"wwwroot/", fileName);                
                FileInfo file = new FileInfo(filePath);
                if (!file.Exists) return null;
                string newFileName = Regex.Replace(fileName, @"^([^\/]*\/)", "");
                file.MoveTo(Path.Combine(Directory.GetCurrentDirectory(), @"wwwroot/", newFileName));
                return newFileName;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public void RemoveFile(string fileName)
        {
            try
            {
                string filePath = Path.Combine(Directory.GetCurrentDirectory(), @"wwwroot/", fileName);
                FileInfo file = new FileInfo(filePath);
                if (file.Exists) file.Delete();
                else throw new Exception("File Not found.");
            }
            catch (Exception)
            {
                throw;
            }
        }

        private string FileNameBuilder(IFormFile file)
        {
            DateTime currentTime = DateTime.UtcNow;
            long unixTime = ((DateTimeOffset)currentTime).ToUnixTimeSeconds();
            int rand = new Random().Next(1, 10000);
            string ext = Path.GetExtension(file.FileName);
            return unixTime.ToString() + rand.ToString() + ext;
        }

    }
}
