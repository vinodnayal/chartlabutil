using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Mail;

namespace ChartLabFinCalculation.UTIL
{
    public class MailUtility
    {
        static log4net.ILog log = log4net.LogManager.GetLogger(typeof(MailUtility));

        internal static void SendMail(string Subject, string Body, string From, string To)
        {
            bool retry = true;
            MailMessage mail = new MailMessage();
            SmtpClient SmtpServer = new SmtpClient(Constants.SmtpServer);
            SmtpServer.Port = Constants.SmtpPort;
            SmtpServer.Credentials = new System.Net.NetworkCredential(Constants.AdminEmail, Constants.AdminMailPassword);
            SmtpServer.EnableSsl = true;
           try
            {
                mail.Subject = Subject;
                mail.From = new MailAddress(From);
                mail.To.Add(To);
                mail.Body = Body;
                mail.IsBodyHtml = true;
                SmtpServer.Send(mail);
            }
            catch (Exception ex)
           {
               try
               {
                   if (retry)
                   {
                       SmtpServer.Send(mail);
                       retry = false;
                   }
               }
               catch (Exception ex1)
               {
                   log.Error(ex1);
               }
                
                log.Error("Error in Sending  email to email id: " + To + " subject : " + Subject);
                log.Error(ex);
            }
        }

    }
}

