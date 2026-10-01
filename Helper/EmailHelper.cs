using System.Net.Mail;
using System.Net;

namespace HCMPortal.Helper
{
    public static class EmailHelper
    {
public static void sendEmail(string ToEmail,string body, string subject,byte[] attachmentData = null, string attachmentFileName = null)    // New parameter for PDF file name
    {

        string fromEmail = "thecloudtree.developer@gmail.com";
        string password = "jdndluquofjmwzjl";
        //EmailServerDetailsViewModel econfig = new EmailServerDetailsViewModel();
        //econfig = commonService.GetEmailServerDetails(companyid).Result;

        // Ensure the subject is used, currently it's hardcoded to "HCM Purchase"
        string finalSubject = string.IsNullOrEmpty(subject) ? "HCM Purchase" : subject;

        try
        {
            using (MailMessage msg = new MailMessage())
            {
                msg.From = new MailAddress(fromEmail ?? String.Empty);
                msg.To.Add(ToEmail);
                msg.Subject = finalSubject; // Use the passed subject
                msg.Body = body;
                msg.IsBodyHtml = true;

                // --- ATTACHMENT LOGIC ---
                if (attachmentData != null && !string.IsNullOrEmpty(attachmentFileName))
                {
                    // Create a MemoryStream from the PDF byte array
                    MemoryStream stream = new MemoryStream(attachmentData);

                    // Create the Attachment object. We use "application/pdf" for the MIME type.
                    Attachment attachment = new Attachment(stream, attachmentFileName, "application/pdf");

                    msg.Attachments.Add(attachment);
                }
                // ------------------------

                using (SmtpClient smt = new SmtpClient())
                {
                    smt.Host = "smtp.gmail.com" ?? String.Empty;
                    System.Net.NetworkCredential ntwd = new NetworkCredential();
                    ntwd.UserName = fromEmail;
                    ntwd.Password = password;
                    smt.UseDefaultCredentials = false;
                    smt.Credentials = ntwd;
                    smt.Port = 587;
                    smt.DeliveryMethod = SmtpDeliveryMethod.Network;
                    smt.EnableSsl = true;
                    smt.Timeout = 300000000;
                    smt.Send(msg);
                }
            }
        }
        catch (Exception ex)
        {
            // Log the exception here instead of just re-throwing
            throw ex;
        }
    }


    //public static void sendEmail(string ToEmail, string body,string subject)
    //{

    //    string fromEmail = "thecloudtree.developer@gmail.com";
    //    string password = "jdndluquofjmwzjl";
    //    //EmailServerDetailsViewModel econfig = new EmailServerDetailsViewModel();
    //    //econfig = commonService.GetEmailServerDetails(companyid).Result;
    //    try
    //    {
    //        MailMessage msg = new MailMessage();
    //        msg.From = new MailAddress(fromEmail ?? String.Empty);
    //        msg.To.Add(ToEmail);
    //        msg.Subject = "HCM Purchase";
    //        msg.Body = body;
    //        msg.IsBodyHtml = true;
    //        SmtpClient smt = new SmtpClient();
    //        smt.Host = "smtp.gmail.com" ?? String.Empty;
    //        System.Net.NetworkCredential ntwd = new NetworkCredential();
    //        ntwd.UserName = fromEmail;
    //        ntwd.Password = password;
    //        smt.UseDefaultCredentials = false;
    //        smt.Credentials = ntwd;
    //        smt.Port = 587;
    //        smt.DeliveryMethod = SmtpDeliveryMethod.Network;
    //        smt.EnableSsl = true;
    //        smt.Timeout = 300000000;
    //        smt.Send(msg);
    //    }
    //    catch (Exception ex)
    //    {
    //        throw ex;
    //    }
    //}
}
}
