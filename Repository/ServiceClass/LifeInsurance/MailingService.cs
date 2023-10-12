using System.Net;
using System.Net.Mail;
using test0000001.Models.LifeInsurance;

namespace test0000001.Repository.ServiceClass.LifeInsurance
{
    public class MailingService
    {
        public MailingService() { }

        public void SendMail(MailingData model)
        {
            using (MailMessage mm = new MailMessage(model.From, model.To))
            {
                mm.Subject = model.Subject;
                mm.Body = model.Body;
                if (!string.IsNullOrEmpty(model.FilePath))
                {
                    string filePath = Path.Combine(Directory.GetCurrentDirectory(), @"wwwroot/", model.FilePath);
                    FileInfo file = new FileInfo(filePath);
                    if(file.Exists) mm.Attachments.Add(new Attachment(filePath));
                }
                mm.IsBodyHtml = true;
                using (SmtpClient smtp = new SmtpClient())
                {
                    smtp.Host = "smtp.gmail.com";
                    smtp.EnableSsl = true;
                    NetworkCredential NetworkCred = new NetworkCredential(model?.From, model?.Password);
                    smtp.Credentials = NetworkCred;
                    smtp.Port = 587;
                    smtp.Send(mm);
                }
            }
        }
    }
}
