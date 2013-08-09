using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Mail;
using System.Configuration;

namespace ChartLabFinCalculation.UTIL
{
    public class MailUtility
    {
        static log4net.ILog log = log4net.LogManager.GetLogger(typeof(MailUtility));

        static  String _SMTPServer = ConfigurationManager.AppSettings["SMTPServer"];
        static String _adminEmail = ConfigurationManager.AppSettings["AdminEmail"];
        static String _adminMailPassword = ConfigurationManager.AppSettings["AdminPassword"];
        static int _SmtpPort = Convert.ToInt32(ConfigurationManager.AppSettings["SMTPPort"]);

        internal static void SendMail(string Subject, string Body, string From, string To)
        {
           

            bool retry = true;
            MailMessage mail = new MailMessage();
            SmtpClient SmtpServer = new SmtpClient(_SMTPServer);
            SmtpServer.Port = _SmtpPort;
            SmtpServer.Credentials = new System.Net.NetworkCredential(_adminEmail, _adminMailPassword);
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
        internal static void SendMail(string Subject, string Body, string From, List<string> usersEmailsList)
        {

            bool retry = true;
            MailMessage mail = new MailMessage();
            SmtpClient SmtpServer = new SmtpClient(_SMTPServer);
            SmtpServer.Port = _SmtpPort;
            SmtpServer.Credentials = new System.Net.NetworkCredential(_adminEmail, _adminMailPassword);
            SmtpServer.EnableSsl = true;
            try
            {
                if (Body != "")
                {
                    mail.Subject = Subject;
                    mail.From = new MailAddress(From);
                    foreach (String mailId in usersEmailsList)
                    {
                        if (mailId != "")
                        {
                            mail.Bcc.Add(mailId);
                        }
                    }

                    mail.Body = Body;
                    mail.IsBodyHtml = true;
                    SmtpServer.Send(mail);
                }
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

                log.Error("Error in Sending  email subject " + Subject);
                log.Error(ex);
            }
        }
    }
}

