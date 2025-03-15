using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MGOBankApp.BLL.Utilities
{
    internal static class EmailTemplates
    {
        //TODO: Change "Changepassword" button and support link 
        public static string GetSecurityAlertMessage(string userName, string originalIp, string newIp)
        {
            return $@"
<!DOCTYPE html>
<html lang='en'>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>Security Alert</title>
    <style>
        body {{ font-family: Arial, sans-serif; color: #333; line-height: 1.6; }}
        h2 {{ color: #ff5c5c; }}
        p {{ font-size: 16px; }}
        .alert {{ background-color: #f8f8f8; padding: 10px; border-left: 5px solid #ff5c5c; }}
        .footer {{ font-size: 14px; color: #777; margin-top: 20px; }}
        .button {{
            display: inline-block;
            background-color: #ff5c5c;
            color: #fff;
            padding: 10px 15px;
            text-decoration: none;
            border-radius: 5px;
            font-weight: bold;
        }}
    </style>
</head>
<body>
    <h2>Security Alert: Unusual Login Activity</h2>
    <p>Hello {userName},</p>

    <p>We noticed a login attempt from a new IP address that doesn't match the one typically used to access your account. For your security, we wanted to alert you about this change.</p>

    <div class='alert'>
        <p><strong>Original IP Address:</strong> {originalIp}</p>
        <p><strong>New IP Address:</strong> {newIp}</p>
    </div>

    <p>If this was you, there's no need for concern. If you don't recognize this activity, we recommend updating your password and reviewing your account activity for any suspicious actions.</p>

    <p><a href='https://localhost:7160/Identity/Account/Manage/ChangePassword' class='button'>Change Password</a></p>

    <div class='footer'>
        <p>If you need assistance, please contact our support team at <a href='mailto:har.mkrtchyan2006@gmail.com'>har.mkrtchyan2006@gmail.com</a>.</p> 
        <p>Best regards,</p>
        <p>The Security Team<br>ScanGuard</p>
    </div>
</body>
</html>
";
        }
    }
}
