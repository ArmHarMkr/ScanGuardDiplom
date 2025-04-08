using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScanGuard.BLL.Utilities
{
    public static class EmailTemplates
    {
        public static string GetConfirmationEmail(string userName, string confirmationLink)
        {
            return $@"
<!DOCTYPE html>
<html lang='en'>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>ScanGuard - Confirm Your Email</title>
    <style>
        body {{
            font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
            background: #1a1a2e;
            color: #edf2f4;
            line-height: 1.6;
            margin: 0;
            padding: 20px;
        }}
        .container {{
            max-width: 600px;
            margin: 20px auto;
            background: #2b2d42;
            border-radius: 8px;
            padding: 30px;
            box-shadow: 0 0 30px rgba(239, 35, 60, 0.2);
            border-top: 4px solid #ef233c;
        }}
        h2 {{
            color: #ef233c;
            margin: 0 0 25px 0;
            font-size: 26px;
            font-weight: 600;
        }}
        p {{
            margin: 0 0 25px 0;
            font-size: 16px;
            color:whitesmoke;
        }}
        .button {{
            display: inline-block;
            background: #ef233c;
            background: linear-gradient(135deg, #ef233c, #d90429);
            color: white !important;
            padding: 14px 30px;
            text-decoration: none;
            border-radius: 6px;
            font-weight: 600;
            margin: 20px 0;
            transition: all 0.3s ease;
            box-shadow: 0 4px 15px rgba(239, 35, 60, 0.4);
            border: none;
            font-size: 16px;
        }}
        .button:hover {{
            transform: translateY(-2px);
            box-shadow: 0 6px 20px rgba(239, 35, 60, 0.6);
        }}
        .footer {{
            margin-top: 35px;
            font-size: 14px;
            color: #8d99ae;
            border-top: 1px solid #3a3d5e;
            padding-top: 20px;
        }}
        .logo {{
            color: #edf2f4;
            font-weight: 700;
            font-size: 22px;
            margin-bottom: 25px;
            display: block;
        }}
        a {{
            color: #ef233c;
            text-decoration: none;
            font-weight: 500;
        }}
        .highlight {{
            background: rgba(239, 35, 60, 0.1);
            padding: 15px;
            border-radius: 6px;
            margin: 25px 0;
        }}
    </style>
</head>
<body>
    <div class='container'>
        <span class='logo'>ScanGuard</span>
        <h2>Complete Your Registration, {userName}!</h2>
        
        <p>We're excited to have you join our security community. To activate your ScanGuard account and unlock all features, please verify your email address:</p>
        
        <div class='highlight'>
            <a href='{confirmationLink}' class='button'>Verify Email Address</a>
        </div>
        
        <p>This link will expire in 24 hours. If you didn't create this account, please <a href='mailto:zadref45@gmail.com'>contact our support team</a> immediately.</p>
        
        <div class='footer'>
            <p>Your first line of defense in the digital world</p>
            <p>© {DateTime.Now.Year} ScanGuard. All rights reserved.</p>
        </div>
    </div>
</body>
</html>";
        }

        public static string GetSecurityAlertMessage(string userName, string originalIp, string newIp, string changePasswordUrl)
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

    <p><a href='{changePasswordUrl}' class='button'>Change Password</a></p>

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
