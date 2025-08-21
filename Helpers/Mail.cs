using Azure;
using Azure.Communication.Email;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Web;

namespace TimeTracker.Helpers
{
    public class Mail
    {
        
        public string sendMailAzure(string connectionString, string strSubject, string strBody, string strSender, string strRecipient)
        {

            EmailClient emailClient = new EmailClient(connectionString);

            var emailContent = new EmailContent(strSubject)
            {
                Html = strBody
            };

            var emailMessage = new EmailMessage(
                senderAddress: strSender,
                recipientAddress: strRecipient,
                content: emailContent);

            try
            {
                var emailSendOperation = emailClient.Send(
                    wait: WaitUntil.Completed,
                    message: emailMessage);
                return "Ok";
            }
            catch (RequestFailedException ex)
            {
                return $"Email send operation failed with error code: {ex.ErrorCode}, message: {ex.Message}";
            }

        }

        public string sendMail(string strMailFrom, string strNameFrom, string strMailTo, string srtNameTo, string strSubject, string srtBody, bool isBodyHTML,
            string strHost, bool enableSSL, int nmbPort, string strUsername, string strPass,
            List<Attachment> attachmentList = null)
        {

            try
            {

                MailMessage message = new MailMessage(new MailAddress(strMailFrom, strNameFrom), new MailAddress(strMailTo, srtNameTo));
                message.Subject = strSubject;
                message.Body = srtBody;
                message.IsBodyHtml = isBodyHTML;

                if (attachmentList != null)
                {
                    foreach (var item in attachmentList)
                    {
                        message.Attachments.Add(item);
                    }
                }


                SmtpClient smtpClient = new SmtpClient();
                smtpClient.Host = strHost;
                smtpClient.EnableSsl = enableSSL;

                NetworkCredential networkCredential = new NetworkCredential();
                networkCredential.UserName = strUsername;
                networkCredential.Password = strPass;
                smtpClient.UseDefaultCredentials = true;
                smtpClient.Credentials = networkCredential;
                smtpClient.Port = nmbPort;
                smtpClient.Send(message);

                return "Mail send succesfully, check your mail for Password Recovery Link";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }


        }


        public string recoverPassTemplate(string name, string link)
        {
            string strTemplate = @"

            <!DOCTYPE html PUBLIC ""-//W3C//DTD XHTML 1.0 Transitional//EN"" ""http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd"">
            <html xmlns=""http://www.w3.org/1999/xhtml"">
            <head>
            <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"" />
            <meta name="""" />
            <meta http-equiv=""Content-Type"" content=""text/html; charset=UTF-8"" />
            <meta name=""color-scheme"" content=""light dark"" />
            <meta name=""supported-color-schemes"" content=""light dark"" />
            <title></title>
            <style type=""text/css"" rel=""stylesheet"" media=""all"">
            /* Base ------------------------------ */
    
            @import url(""https://fonts.googleapis.com/css?family=Nunito+Sans:400,700&display=swap"");
            body {
              width: 100% !important;
              height: 100%;
              margin: 0;
              -webkit-text-size-adjust: none;
            }
    
            a {
              color: #3869D4;
            }
    
            a img {
              border: none;
            }
    
            td {
              word-break: break-word;
            }
    
            .preheader {
              display: none !important;
              visibility: hidden;
              mso-hide: all;
              font-size: 1px;
              line-height: 1px;
              max-height: 0;
              max-width: 0;
              opacity: 0;
              overflow: hidden;
            }
            /* Type ------------------------------ */
    
            body,
            td,
            th {
              font-family: ""Nunito Sans"", Helvetica, Arial, sans-serif;
            }
    
            h1 {
              margin-top: 0;
              color: #333333;
              font-size: 22px;
              font-weight: bold;
              text-align: left;
            }
    
            h2 {
              margin-top: 0;
              color: #333333;
              font-size: 16px;
              font-weight: bold;
              text-align: left;
            }
    
            h3 {
              margin-top: 0;
              color: #333333;
              font-size: 14px;
              font-weight: bold;
              text-align: left;
            }
    
            td,
            th {
              font-size: 16px;
            }
    
            p,
            ul,
            ol,
            blockquote {
              margin: .4em 0 1.1875em;
              font-size: 16px;
              line-height: 1.625;
            }
    
            p.sub {
              font-size: 13px;
            }
            /* Utilities ------------------------------ */
    
            .align-right {
              text-align: right;
            }
    
            .align-left {
              text-align: left;
            }
    
            .align-center {
              text-align: center;
            }
    
            .u-margin-bottom-none {
              margin-bottom: 0;
            }
            /* Buttons ------------------------------ */
    
            .button {
              background-color: #3869D4;
              border-top: 10px solid #3869D4;
              border-right: 18px solid #3869D4;
              border-bottom: 10px solid #3869D4;
              border-left: 18px solid #3869D4;
              display: inline-block;
              color: #FFF;
              text-decoration: none;
              border-radius: 3px;
              box-shadow: 0 2px 3px rgba(0, 0, 0, 0.16);
              -webkit-text-size-adjust: none;
              box-sizing: border-box;
            }
    
            .button--green {
              background-color: #22BC66;
              border-top: 10px solid #22BC66;
              border-right: 18px solid #22BC66;
              border-bottom: 10px solid #22BC66;
              border-left: 18px solid #22BC66;
            }
    
            .button--red {
              background-color: #FF6136;
              border-top: 10px solid #FF6136;
              border-right: 18px solid #FF6136;
              border-bottom: 10px solid #FF6136;
              border-left: 18px solid #FF6136;
            }
    
            @media only screen and (max-width: 500px) {
              .button {
                width: 100% !important;
                text-align: center !important;
              }
            }
            /* Attribute list ------------------------------ */
    
            .attributes {
              margin: 0 0 21px;
            }
    
            .attributes_content {
              background-color: #F4F4F7;
              padding: 16px;
            }
    
            .attributes_item {
              padding: 0;
            }
            /* Related Items ------------------------------ */
    
            .related {
              width: 100%;
              margin: 0;
              padding: 25px 0 0 0;
              -premailer-width: 100%;
              -premailer-cellpadding: 0;
              -premailer-cellspacing: 0;
            }
    
            .related_item {
              padding: 10px 0;
              color: #CBCCCF;
              font-size: 15px;
              line-height: 18px;
            }
    
            .related_item-title {
              display: block;
              margin: .5em 0 0;
            }
    
            .related_item-thumb {
              display: block;
              padding-bottom: 10px;
            }
    
            .related_heading {
              border-top: 1px solid #CBCCCF;
              text-align: center;
              padding: 25px 0 10px;
            }
            /* Discount Code ------------------------------ */
    
            .discount {
              width: 100%;
              margin: 0;
              padding: 24px;
              -premailer-width: 100%;
              -premailer-cellpadding: 0;
              -premailer-cellspacing: 0;
              background-color: #F4F4F7;
              border: 2px dashed #CBCCCF;
            }
    
            .discount_heading {
              text-align: center;
            }
    
            .discount_body {
              text-align: center;
              font-size: 15px;
            }
            /* Social Icons ------------------------------ */
    
            .social {
              width: auto;
            }
    
            .social td {
              padding: 0;
              width: auto;
            }
    
            .social_icon {
              height: 20px;
              margin: 0 8px 10px 8px;
              padding: 0;
            }
            /* Data table ------------------------------ */
    
            .purchase {
              width: 100%;
              margin: 0;
              padding: 35px 0;
              -premailer-width: 100%;
              -premailer-cellpadding: 0;
              -premailer-cellspacing: 0;
            }
    
            .purchase_content {
              width: 100%;
              margin: 0;
              padding: 25px 0 0 0;
              -premailer-width: 100%;
              -premailer-cellpadding: 0;
              -premailer-cellspacing: 0;
            }
    
            .purchase_item {
              padding: 10px 0;
              color: #51545E;
              font-size: 15px;
              line-height: 18px;
            }
    
            .purchase_heading {
              padding-bottom: 8px;
              border-bottom: 1px solid #EAEAEC;
            }
    
            .purchase_heading p {
              margin: 0;
              color: #85878E;
              font-size: 12px;
            }
    
            .purchase_footer {
              padding-top: 15px;
              border-top: 1px solid #EAEAEC;
            }
    
            .purchase_total {
              margin: 0;
              text-align: right;
              font-weight: bold;
              color: #333333;
            }
    
            .purchase_total--label {
              padding: 0 15px 0 0;
            }
    
            body {
              background-color: #F2F4F6;
              color: #51545E;
            }
    
            p {
              color: #51545E;
            }
    
            .email-wrapper {
              width: 100%;
              margin: 0;
              padding: 0;
              -premailer-width: 100%;
              -premailer-cellpadding: 0;
              -premailer-cellspacing: 0;
              background-color: #F2F4F6;
            }
    
            .email-content {
              width: 100%;
              margin: 0;
              padding: 0;
              -premailer-width: 100%;
              -premailer-cellpadding: 0;
              -premailer-cellspacing: 0;
            }
            /* Masthead ----------------------- */
    
            .email-masthead {
              padding: 25px 0;
              text-align: center;
            }
    
            .email-masthead_logo {
              width: 94px;
            }
    
            .email-masthead_name {
              font-size: 16px;
              font-weight: bold;
              color: #A8AAAF;
              text-decoration: none;
              text-shadow: 0 1px 0 white;
            }
            /* Body ------------------------------ */
    
            .email-body {
              width: 100%;
              margin: 0;
              padding: 0;
              -premailer-width: 100%;
              -premailer-cellpadding: 0;
              -premailer-cellspacing: 0;
            }
    
            .email-body_inner {
              width: 570px;
              margin: 0 auto;
              padding: 0;
              -premailer-width: 570px;
              -premailer-cellpadding: 0;
              -premailer-cellspacing: 0;
              background-color: #FFFFFF;
            }
    
            .email-footer {
              width: 570px;
              margin: 0 auto;
              padding: 0;
              -premailer-width: 570px;
              -premailer-cellpadding: 0;
              -premailer-cellspacing: 0;
              text-align: center;
            }
    
            .email-footer p {
              color: #A8AAAF;
            }
    
            .body-action {
              width: 100%;
              margin: 30px auto;
              padding: 0;
              -premailer-width: 100%;
              -premailer-cellpadding: 0;
              -premailer-cellspacing: 0;
              text-align: center;
            }
    
            .body-sub {
              margin-top: 25px;
              padding-top: 25px;
              border-top: 1px solid #EAEAEC;
            }
    
            .content-cell {
              padding: 45px;
            }
            /*Media Queries ------------------------------ */
    
            @media only screen and (max-width: 600px) {
              .email-body_inner,
              .email-footer {
                width: 100% !important;
              }
            }
    
            @media (prefers-color-scheme: dark) {
              body,
              .email-body,
              .email-body_inner,
              .email-content,
              .email-wrapper,
              .email-masthead,
              .email-footer {
                background-color: #333333 !important;
                color: #FFF !important;
              }
              p,
              ul,
              ol,
              blockquote,
              h1,
              h2,
              h3,
              span,
              .purchase_item {
                color: #FFF !important;
              }
              .attributes_content,
              .discount {
                background-color: #222 !important;
              }
              .email-masthead_name {
                text-shadow: none !important;
              }
            }
    
            :root {
              color-scheme: light dark;
              supported-color-schemes: light dark;
            }
            </style>
            <!--[if mso]>
            <style type=""text/css"">
              .f-fallback  {
                font-family: Arial, sans-serif;
              }
            </style>
          <![endif]-->
          </head>
          <body>
            <span class=""preheader"">Use this link to reset your password.</span>
            <table class=""email-wrapper"" width=""100%"" cellpadding=""0"" cellspacing=""0"" role=""presentation"">
              <tr>
                <td align=""center"">
                  <table class=""email-content"" width=""100%"" cellpadding=""0"" cellspacing=""0"" role=""presentation"">
                    <tr>
                      <td class=""email-masthead"">
                        <a href="" class=""f-fallback email-masthead_name"">
                        Systems X
                      </a>
                      </td>
                    </tr>
                    <!-- Email Body -->
                    <tr>
                      <td class=""email-body"" width=""570"" cellpadding=""0"" cellspacing=""0"">
                        <table class=""email-body_inner"" align=""center"" width=""570"" cellpadding=""0"" cellspacing=""0"" role=""presentation"">
                          <!-- Body content -->
                          <tr>
                            <td class=""content-cell"">
                              <div class=""f-fallback"">
                                <h1>Hi " + name + @",</h1> 
                                <p>You recently requested to reset your password for your Systems X account. Use the button below to reset it. </p>
                                <!-- Action -->
                                <table class=""body-action"" align=""center"" width=""100%"" cellpadding=""0"" cellspacing=""0"" role=""presentation"">
                                  <tr>
                                    <td align=""center"">
                                      
                                      <table width=""100%"" border=""0"" cellspacing=""0"" cellpadding=""0"" role=""presentation"">
                                        <tr>
                                          <td align=""center"">
                                            <a href="+ link + @" class=""f-fallback button button--green"" target=""_blank"">Reset your password</a>
                                          </td>
                                        </tr>
                                      </table>
                                    </td>
                                  </tr>
                                </table>
                                <p>For security, if you did not request a password reset, please ignore this email or <a href=""mailto: support@systems-x.com"">contact support</a> if you have questions.</p>
                                <p>Thanks,
                                  <br>The Systems X team</p>
                                <!-- Sub copy -->
                                <table class=""body-sub"" role=""presentation"">
                                  <tr>
                                    <td>
                                      <p class=""f-fallback sub"">If you’re having trouble with the button above, copy and paste the URL below into your web browser.</p>
                                      <p class=""f-fallback sub"">" + link + @"</p>
                                    </td>
                                  </tr>
                                </table>
                              </div>
                            </td>
                          </tr>
                        </table>
                      </td>
                    </tr>
                    <tr>
                      <td>
                        <table class=""email-footer"" align=""center"" width=""570"" cellpadding=""0"" cellspacing=""0"" role=""presentation"">
                          <tr>
                            <td class=""content-cell"" align=""center"">
                              <p class=""f-fallback sub align-center"">
                                Systems X
                                <br>
                                <br>
                              </p>
                            </td>
                          </tr>
                        </table>
                      </td>
                    </tr>
                  </table>
                </td>
              </tr>
            </table>
          </body>
        </html>";

            return strTemplate;
        }


        public string newUserTemplate(string company, string user, string url, string password, string mail)
        {
            string html = @"
                    <!DOCTYPE html PUBLIC ""-//W3C//DTD XHTML 1.0 Transitional//EN"" ""http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd"">
                    <html xmlns=""http://www.w3.org/1999/xhtml"">
                      <head>
                        <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"" />
                        <meta name=""x-apple-disable-message-reformatting"" />
                        <meta http-equiv=""Content-Type"" content=""text/html; charset=UTF-8"" />
                        <meta name=""color-scheme"" content=""light dark"" />
                        <meta name=""supported-color-schemes"" content=""light dark"" />
                        <title></title>
                        <style type=""text/css"" rel=""stylesheet"" media=""all"">
                        /* Base ------------------------------ */
    
                        @import url(""https://fonts.googleapis.com/css?family=Nunito+Sans:400,700&display=swap"");
                        body {
                          width: 100% !important;
                          height: 100%;
                          margin: 0;
                          -webkit-text-size-adjust: none;
                        }
    
                        a {
                          color: #3869D4;
                        }
        
                        a img {
                          border: none;
                        }
    
                        td {
                          word-break: break-word;
                        }
    
                        .preheader {
                          display: none !important;
                          visibility: hidden;
                          mso-hide: all;
                          font-size: 1px;
                          line-height: 1px;
                          max-height: 0;
                          max-width: 0;
                          opacity: 0;
                          overflow: hidden;
                        }
                        /* Type ------------------------------ */
    
                        body,
                        td,
                        th {
                          font-family: ""Nunito Sans"", Helvetica, Arial, sans-serif;
                        }
    
                        h1 {
                          margin-top: 0;
                          color: #333333;
                          font-size: 22px;
                          font-weight: bold;
                          text-align: left;
                        }
    
                        h2 {
                          margin-top: 0;
                          color: #333333;
                          font-size: 16px;
                          font-weight: bold;
                          text-align: left;
                        }
    
                        h3 {
                          margin-top: 0;
                          color: #333333;
                          font-size: 14px;
                          font-weight: bold;
                          text-align: left;
                        }
    
                        td,
                        th {
                          font-size: 16px;
                        }
    
                        p,
                        ul,
                        ol,
                        blockquote {
                          margin: .4em 0 1.1875em;
                          font-size: 16px;
                          line-height: 1.625;
                        }
    
                        p.sub {
                          font-size: 13px;
                        }
                        /* Utilities ------------------------------ */
    
                        .align-right {
                          text-align: right;
                        }
    
                        .align-left {
                          text-align: left;
                        }
    
                        .align-center {
                          text-align: center;
                        }
    
                        .u-margin-bottom-none {
                          margin-bottom: 0;
                        }
                        /* Buttons ------------------------------ */
    
                        .button {
                          background-color: #3869D4;
                          border-top: 10px solid #3869D4;
                          border-right: 18px solid #3869D4;
                          border-bottom: 10px solid #3869D4;
                          border-left: 18px solid #3869D4;
                          display: inline-block;
                          color: #FFF;
                          text-decoration: none;
                          border-radius: 3px;
                          box-shadow: 0 2px 3px rgba(0, 0, 0, 0.16);
                          -webkit-text-size-adjust: none;
                          box-sizing: border-box;
                        }
    
                        .button--green {
                          background-color: #22BC66;
                          border-top: 10px solid #22BC66;
                          border-right: 18px solid #22BC66;
                          border-bottom: 10px solid #22BC66;
                          border-left: 18px solid #22BC66;
                        }
    
                        .button--red {
                          background-color: #FF6136;
                          border-top: 10px solid #FF6136;
                          border-right: 18px solid #FF6136;
                          border-bottom: 10px solid #FF6136;
                          border-left: 18px solid #FF6136;
                        }
    
                        @media only screen and (max-width: 500px) {
                          .button {
                            width: 100% !important;
                            text-align: center !important;
                          }
                        }
                        /* Attribute list ------------------------------ */
    
                        .attributes {
                          margin: 0 0 21px;
                        }
    
                        .attributes_content {
                          background-color: #F4F4F7;
                          padding: 16px;
                        }
    
                        .attributes_item {
                          padding: 0;
                        }
                        /* Related Items ------------------------------ */
    
                        .related {
                          width: 100%;
                          margin: 0;
                          padding: 25px 0 0 0;
                          -premailer-width: 100%;
                          -premailer-cellpadding: 0;
                          -premailer-cellspacing: 0;
                        }
    
                        .related_item {
                          padding: 10px 0;
                          color: #CBCCCF;
                          font-size: 15px;
                          line-height: 18px;
                        }
    
                        .related_item-title {
                          display: block;
                          margin: .5em 0 0;
                        }
    
                        .related_item-thumb {
                          display: block;
                          padding-bottom: 10px;
                        }
    
                        .related_heading {
                          border-top: 1px solid #CBCCCF;
                          text-align: center;
                          padding: 25px 0 10px;
                        }
                        /* Discount Code ------------------------------ */
    
                        .discount {
                          width: 100%;
                          margin: 0;
                          padding: 24px;
                          -premailer-width: 100%;
                          -premailer-cellpadding: 0;
                          -premailer-cellspacing: 0;
                          background-color: #F4F4F7;
                          border: 2px dashed #CBCCCF;
                        }
    
                        .discount_heading {
                          text-align: center;
                        }
    
                        .discount_body {
                          text-align: center;
                          font-size: 15px;
                        }
                        /* Social Icons ------------------------------ */
    
                        .social {
                          width: auto;
                        }
    
                        .social td {
                          padding: 0;
                          width: auto;
                        }
    
                        .social_icon {
                          height: 20px;
                          margin: 0 8px 10px 8px;
                          padding: 0;
                        }
                        /* Data table ------------------------------ */
    
                        .purchase {
                          width: 100%;
                          margin: 0;
                          padding: 35px 0;
                          -premailer-width: 100%;
                          -premailer-cellpadding: 0;
                          -premailer-cellspacing: 0;
                        }
    
                        .purchase_content {
                          width: 100%;
                          margin: 0;
                          padding: 25px 0 0 0;
                          -premailer-width: 100%;
                          -premailer-cellpadding: 0;
                          -premailer-cellspacing: 0;
                        }
    
                        .purchase_item {
                          padding: 10px 0;
                          color: #51545E;
                          font-size: 15px;
                          line-height: 18px;
                        }
    
                        .purchase_heading {
                          padding-bottom: 8px;
                          border-bottom: 1px solid #EAEAEC;
                        }
    
                        .purchase_heading p {
                          margin: 0;
                          color: #85878E;
                          font-size: 12px;
                        }
    
                        .purchase_footer {
                          padding-top: 15px;
                          border-top: 1px solid #EAEAEC;
                        }
    
                        .purchase_total {
                          margin: 0;
                          text-align: right;
                          font-weight: bold;
                          color: #333333;
                        }
    
                        .purchase_total--label {
                          padding: 0 15px 0 0;
                        }
    
                        body {
                          background-color: #F2F4F6;
                          color: #51545E;
                        }
    
                        p {
                          color: #51545E;
                        }
    
                        .email-wrapper {
                          width: 100%;
                          margin: 0;
                          padding: 0;
                          -premailer-width: 100%;
                          -premailer-cellpadding: 0;
                          -premailer-cellspacing: 0;
                          background-color: #F2F4F6;
                        }
    
                        .email-content {
                          width: 100%;
                          margin: 0;
                          padding: 0;
                          -premailer-width: 100%;
                          -premailer-cellpadding: 0;
                          -premailer-cellspacing: 0;
                        }
                        /* Masthead ----------------------- */
    
                        .email-masthead {
                          padding: 25px 0;
                          text-align: center;
                        }
    
                        .email-masthead_logo {
                          width: 94px;
                        }
    
                        .email-masthead_name {
                          font-size: 16px;
                          font-weight: bold;
                          color: #A8AAAF;
                          text-decoration: none;
                          text-shadow: 0 1px 0 white;
                        }
                        /* Body ------------------------------ */
    
                        .email-body {
                          width: 100%;
                          margin: 0;
                          padding: 0;
                          -premailer-width: 100%;
                          -premailer-cellpadding: 0;
                          -premailer-cellspacing: 0;
                        }
    
                        .email-body_inner {
                          width: 570px;
                          margin: 0 auto;
                          padding: 0;
                          -premailer-width: 570px;
                          -premailer-cellpadding: 0;
                          -premailer-cellspacing: 0;
                          background-color: #FFFFFF;
                        }
    
                        .email-footer {
                          width: 570px;
                          margin: 0 auto;
                          padding: 0;
                          -premailer-width: 570px;
                          -premailer-cellpadding: 0;
                          -premailer-cellspacing: 0;
                          text-align: center;
                        }
    
                        .email-footer p {
                          color: #A8AAAF;
                        }
    
                        .body-action {
                          width: 100%;
                          margin: 30px auto;
                          padding: 0;
                          -premailer-width: 100%;
                          -premailer-cellpadding: 0;
                          -premailer-cellspacing: 0;
                          text-align: center;
                        }
    
                        .body-sub {
                          margin-top: 25px;
                          padding-top: 25px;
                          border-top: 1px solid #EAEAEC;
                        }
    
                        .content-cell {
                          padding: 45px;
                        }
                        /*Media Queries ------------------------------ */
    
                        @media only screen and (max-width: 600px) {
                          .email-body_inner,
                          .email-footer {
                            width: 100% !important;
                          }
                        }
    
                        @media (prefers-color-scheme: dark) {
                          body,
                          .email-body,
                          .email-body_inner,
                          .email-content,
                          .email-wrapper,
                          .email-masthead,
                          .email-footer {
                            background-color: #333333 !important;
                            color: #FFF !important;
                          }
                          p,
                          ul,
                          ol,
                          blockquote,
                          h1,
                          h2,
                          h3,
                          span,
                          .purchase_item {
                            color: #FFF !important;
                          }
                          .attributes_content,
                          .discount {
                            background-color: #222 !important;
                          }
                          .email-masthead_name {
                            text-shadow: none !important;
                          }
                        }
    
                        :root {
                          color-scheme: light dark;
                          supported-color-schemes: light dark;
                        }
                        </style>
                        <!--[if mso]>
                        <style type=""text/css"">
                          .f-fallback  {
                            font-family: Arial, sans-serif;
                          }
                        </style>
                      <![endif]-->
                      </head>
                      <body>
                        <span class=""preheader"">Registered User</span>
                        <table class=""email-wrapper"" width=""100%"" cellpadding=""0"" cellspacing=""0"" role=""presentation"">
                          <tr>
                            <td align=""center"">
                              <table class=""email-content"" width=""100%"" cellpadding=""0"" cellspacing=""0"" role=""presentation"">
                                <tr>
                                  <td class=""email-masthead"">
                                    <a href=""https://example.com"" class=""f-fallback email-masthead_name"">
                                    " + company + @"
                                  </a>
                                  </td>
                                </tr>
                                <!-- Email Body -->
                                <tr>
                                  <td class=""email-body"" width=""570"" cellpadding=""0"" cellspacing=""0"">
                                    <table class=""email-body_inner"" align=""center"" width=""570"" cellpadding=""0"" cellspacing=""0"" role=""presentation"">
                                      <!-- Body content -->
                                      <tr>
                                        <td class=""content-cell"">
                                          <div class=""f-fallback"">
                                            <h1>Hi " + user + @",</h1>
                                            <p>Welcome to " + company + @" account. <br />
                                            <strong>Below are the data to log in</strong></p>
                                            <!-- Action -->
                                            <table class=""body-action"" align=""center"" width=""100%"" cellpadding=""0"" cellspacing=""0"" role=""presentation"">
                                              <tr>
                                                <td align=""center"">
                          
                                                  <table width=""100%"" border=""0"" cellspacing=""0"" cellpadding=""0"" role=""presentation"">
                                                    <tr>
                                                      <td align=""center"">
                                                        <a href=""" + url + @""" class=""f-fallback button button--green"" target=""_blank"">Login</a>
                                                      </td>
                                                    </tr>
                                                  </table>
                                                </td>
                                              </tr>
                                            </table>

                                            <table class=""body-action"" align=""center"" width=""100%"" cellpadding=""0"" cellspacing=""0"" role=""presentation"">
                                              <tr>
                                                <td align=""right"">
                                                  Email:
                                                </td>
                                                <td align=""center"">
                                                  " + mail + @"
                                                </td>
                                              </tr>
                                              <tr>
                                                <td align=""right"">
                                                  Password:
                                                </td>
                                                <td align=""center"">
                                                  " + password + @"
                                                </td>

                                              </tr>

                                            </table>



                                            <p>For security, this request was received from " + company + @". If this information is not for you, please ignore this email or <a href=""mailto:support@systems-x.com"">contact support</a> if you have questions.</p>
                                            <p>Thanks,
                                              <br>The " + company + @" team</p>
                                            <!-- Sub copy -->
                                            <table class=""body-sub"" role=""presentation"">
                                              <tr>
                                                <td>
                                                  <p class=""f-fallback sub"">If you’re having trouble with the button above, copy and paste the URL below into your web browser.</p>
                                                  <p class=""f-fallback sub"">" + url + @"</p>
                                                </td>
                                              </tr>
                                            </table>
                                          </div>
                                        </td>
                                      </tr>
                                    </table>
                                  </td>
                                </tr>
                                <tr>
                                  <td>
                                    <table class=""email-footer"" align=""center"" width=""570"" cellpadding=""0"" cellspacing=""0"" role=""presentation"">
                                      <tr>
                                        <td class=""content-cell"" align=""center"">
                                          <p class=""f-fallback sub align-center"">
                                            " + company + @"
                        
                                          </p>
                                        </td>
                                      </tr>
                                    </table>
                                  </td>
                                </tr>
                              </table>
                            </td>
                          </tr>
                        </table>
                      </body>
                    </html>";

            return html;


        }

        public string VerifyMFACode(string code)
        {
            string html = @"

            <!DOCTYPE html PUBLIC ""-//W3C//DTD XHTML 1.0 Transitional//EN"" ""http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd"">
            <html xmlns=""http://www.w3.org/1999/xhtml"">
            <head>
            <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"" />
            <meta name="""" />
            <meta http-equiv=""Content-Type"" content=""text/html; charset=UTF-8"" />
            <meta name=""color-scheme"" content=""light dark"" />
            <meta name=""supported-color-schemes"" content=""light dark"" />
            <title></title>
            <style type=""text/css"" rel=""stylesheet"" media=""all"">
            /* Base ------------------------------ */
    
            @import url(""https://fonts.googleapis.com/css?family=Nunito+Sans:400,700&display=swap"");
            body {
              width: 100% !important;
              height: 100%;
              margin: 0;
              -webkit-text-size-adjust: none;
            }
    
            a {
              color: #3869D4;
            }
    
            a img {
              border: none;
            }
    
            td {
              word-break: break-word;
            }
    
            .preheader {
              display: none !important;
              visibility: hidden;
              mso-hide: all;
              font-size: 1px;
              line-height: 1px;
              max-height: 0;
              max-width: 0;
              opacity: 0;
              overflow: hidden;
            }
            /* Type ------------------------------ */
    
            body,
            td,
            th {
              font-family: ""Nunito Sans"", Helvetica, Arial, sans-serif;
            }
    
            h1 {
              margin-top: 0;
              color: #333333;
              font-size: 22px;
              font-weight: bold;
              text-align: left;
            }
    
            h2 {
              margin-top: 0;
              color: #333333;
              font-size: 16px;
              font-weight: bold;
              text-align: left;
            }
    
            h3 {
              margin-top: 0;
              color: #333333;
              font-size: 14px;
              font-weight: bold;
              text-align: left;
            }
    
            td,
            th {
              font-size: 16px;
            }
    
            p,
            ul,
            ol,
            blockquote {
              margin: .4em 0 1.1875em;
              font-size: 16px;
              line-height: 1.625;
            }
    
            p.sub {
              font-size: 13px;
            }
            /* Utilities ------------------------------ */
    
            .align-right {
              text-align: right;
            }
    
            .align-left {
              text-align: left;
            }
    
            .align-center {
              text-align: center;
            }
    
            .u-margin-bottom-none {
              margin-bottom: 0;
            }
            /* Buttons ------------------------------ */
    
            .button {
              background-color: #3869D4;
              border-top: 10px solid #3869D4;
              border-right: 18px solid #3869D4;
              border-bottom: 10px solid #3869D4;
              border-left: 18px solid #3869D4;
              display: inline-block;
              color: #FFF;
              text-decoration: none;
              border-radius: 3px;
              box-shadow: 0 2px 3px rgba(0, 0, 0, 0.16);
              -webkit-text-size-adjust: none;
              box-sizing: border-box;
            }
    
            .button--green {
              background-color: #22BC66;
              border-top: 10px solid #22BC66;
              border-right: 18px solid #22BC66;
              border-bottom: 10px solid #22BC66;
              border-left: 18px solid #22BC66;
            }
    
            .button--red {
              background-color: #FF6136;
              border-top: 10px solid #FF6136;
              border-right: 18px solid #FF6136;
              border-bottom: 10px solid #FF6136;
              border-left: 18px solid #FF6136;
            }
    
            @media only screen and (max-width: 500px) {
              .button {
                width: 100% !important;
                text-align: center !important;
              }
            }
            /* Attribute list ------------------------------ */
    
            .attributes {
              margin: 0 0 21px;
            }
    
            .attributes_content {
              background-color: #F4F4F7;
              padding: 16px;
            }
    
            .attributes_item {
              padding: 0;
            }
            /* Related Items ------------------------------ */
    
            .related {
              width: 100%;
              margin: 0;
              padding: 25px 0 0 0;
              -premailer-width: 100%;
              -premailer-cellpadding: 0;
              -premailer-cellspacing: 0;
            }
    
            .related_item {
              padding: 10px 0;
              color: #CBCCCF;
              font-size: 15px;
              line-height: 18px;
            }
    
            .related_item-title {
              display: block;
              margin: .5em 0 0;
            }
    
            .related_item-thumb {
              display: block;
              padding-bottom: 10px;
            }
    
            .related_heading {
              border-top: 1px solid #CBCCCF;
              text-align: center;
              padding: 25px 0 10px;
            }
            /* Discount Code ------------------------------ */
    
            .discount {
              width: 100%;
              margin: 0;
              padding: 24px;
              -premailer-width: 100%;
              -premailer-cellpadding: 0;
              -premailer-cellspacing: 0;
              background-color: #F4F4F7;
              border: 2px dashed #CBCCCF;
            }
    
            .discount_heading {
              text-align: center;
            }
    
            .discount_body {
              text-align: center;
              font-size: 15px;
            }
            /* Social Icons ------------------------------ */
    
            .social {
              width: auto;
            }
    
            .social td {
              padding: 0;
              width: auto;
            }
    
            .social_icon {
              height: 20px;
              margin: 0 8px 10px 8px;
              padding: 0;
            }
            /* Data table ------------------------------ */
    
            .purchase {
              width: 100%;
              margin: 0;
              padding: 35px 0;
              -premailer-width: 100%;
              -premailer-cellpadding: 0;
              -premailer-cellspacing: 0;
            }
    
            .purchase_content {
              width: 100%;
              margin: 0;
              padding: 25px 0 0 0;
              -premailer-width: 100%;
              -premailer-cellpadding: 0;
              -premailer-cellspacing: 0;
            }
    
            .purchase_item {
              padding: 10px 0;
              color: #51545E;
              font-size: 15px;
              line-height: 18px;
            }
    
            .purchase_heading {
              padding-bottom: 8px;
              border-bottom: 1px solid #EAEAEC;
            }
    
            .purchase_heading p {
              margin: 0;
              color: #85878E;
              font-size: 12px;
            }
    
            .purchase_footer {
              padding-top: 15px;
              border-top: 1px solid #EAEAEC;
            }
    
            .purchase_total {
              margin: 0;
              text-align: right;
              font-weight: bold;
              color: #333333;
            }
    
            .purchase_total--label {
              padding: 0 15px 0 0;
            }
    
            body {
              background-color: #F2F4F6;
              color: #51545E;
            }
    
            p {
              color: #51545E;
            }
    
            .email-wrapper {
              width: 100%;
              margin: 0;
              padding: 0;
              -premailer-width: 100%;
              -premailer-cellpadding: 0;
              -premailer-cellspacing: 0;
              background-color: #F2F4F6;
            }
    
            .email-content {
              width: 100%;
              margin: 0;
              padding: 0;
              -premailer-width: 100%;
              -premailer-cellpadding: 0;
              -premailer-cellspacing: 0;
            }
            /* Masthead ----------------------- */
    
            .email-masthead {
              padding: 25px 0;
              text-align: center;
            }
    
            .email-masthead_logo {
              width: 94px;
            }
    
            .email-masthead_name {
              font-size: 16px;
              font-weight: bold;
              color: #A8AAAF;
              text-decoration: none;
              text-shadow: 0 1px 0 white;
            }
            /* Body ------------------------------ */
    
            .email-body {
              width: 100%;
              margin: 0;
              padding: 0;
              -premailer-width: 100%;
              -premailer-cellpadding: 0;
              -premailer-cellspacing: 0;
            }
    
            .email-body_inner {
              width: 570px;
              margin: 0 auto;
              padding: 0;
              -premailer-width: 570px;
              -premailer-cellpadding: 0;
              -premailer-cellspacing: 0;
              background-color: #FFFFFF;
            }
    
            .email-footer {
              width: 570px;
              margin: 0 auto;
              padding: 0;
              -premailer-width: 570px;
              -premailer-cellpadding: 0;
              -premailer-cellspacing: 0;
              text-align: center;
            }
    
            .email-footer p {
              color: #A8AAAF;
            }
    
            .body-action {
              width: 100%;
              margin: 30px auto;
              padding: 0;
              -premailer-width: 100%;
              -premailer-cellpadding: 0;
              -premailer-cellspacing: 0;
              text-align: center;
            }
    
            .body-sub {
              margin-top: 25px;
              padding-top: 25px;
              border-top: 1px solid #EAEAEC;
            }
    
            .content-cell {
              padding: 45px;
            }
            /*Media Queries ------------------------------ */
    
            @media only screen and (max-width: 600px) {
              .email-body_inner,
              .email-footer {
                width: 100% !important;
              }
            }
    
            @media (prefers-color-scheme: dark) {
              body,
              .email-body,
              .email-body_inner,
              .email-content,
              .email-wrapper,
              .email-masthead,
              .email-footer {
                background-color: #333333 !important;
                color: #FFF !important;
              }
              p,
              ul,
              ol,
              blockquote,
              h1,
              h2,
              h3,
              span,
              .purchase_item {
                color: #FFF !important;
              }
              .attributes_content,
              .discount {
                background-color: #222 !important;
              }
              .email-masthead_name {
                text-shadow: none !important;
              }
            }
    
            :root {
              color-scheme: light dark;
              supported-color-schemes: light dark;
            }
            </style>
            <!--[if mso]>
            <style type=""text/css"">
              .f-fallback  {
                font-family: Arial, sans-serif;
              }
            </style>
          <![endif]-->
          </head>
          <body>
            <span class=""preheader"">Use this link to reset your password.</span>
            <table class=""email-wrapper"" width=""100%"" cellpadding=""0"" cellspacing=""0"" role=""presentation"">
              <tr>
                <td align=""center"">
                  <table class=""email-content"" width=""100%"" cellpadding=""0"" cellspacing=""0"" role=""presentation"">
                    <tr>
                      <td class=""email-masthead"">
                        <a href="" class=""f-fallback email-masthead_name"">
                        Systems X
                      </a>
                      </td>
                    </tr>
                    <!-- Email Body -->
                    <tr>
                      <td class=""email-body"" width=""570"" cellpadding=""0"" cellspacing=""0"">
                        <table class=""email-body_inner"" align=""center"" width=""570"" cellpadding=""0"" cellspacing=""0"" role=""presentation"">
                          <!-- Body content -->
                          <tr>
                            <td class=""content-cell"">
                              <div class=""f-fallback"">
                                <h1>Account Login,</h1> 
                                <p>Copy the email confirmation code given below and paste it in signup page security code textbox. </p>
                                <!-- Action -->
                                <table class=""body-action"" align=""center"" width=""100%"" cellpadding=""0"" cellspacing=""0"" role=""presentation"">
                                  <tr>
                                    <td align=""center"">
                                      
                                      <table width=""100%"" border=""0"" cellspacing=""0"" cellpadding=""0"" role=""presentation"">
                                        <tr>
                                          <td align=""center"">
                                            " + code + @" 
                                          </td>
                                        </tr>
                                      </table>
                                    </td>
                                  </tr>
                                </table>
                                <p>For security, if you did not request a password reset, please ignore this email or <a href=""mailto: support@systems-x.com"">contact support</a> if you have questions.</p>
                                <p>Thanks,
                                  <br>The Systems X team</p>
                                <!-- Sub copy -->
                              </div>
                            </td>
                          </tr>
                        </table>
                      </td>
                    </tr>
                    <tr>
                      <td>
                        <table class=""email-footer"" align=""center"" width=""570"" cellpadding=""0"" cellspacing=""0"" role=""presentation"">
                          <tr>
                            <td class=""content-cell"" align=""center"">
                              <p class=""f-fallback sub align-center"">
                                Systems X
                                <br>
                                <br>
                              </p>
                            </td>
                          </tr>
                        </table>
                      </td>
                    </tr>
                  </table>
                </td>
              </tr>
            </table>
          </body>
        </html>";


            return html;
        }
    }

}