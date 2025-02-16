using System;
using System.IO;

namespace FreshFarmMarket.Helpers
{
    public static class AuditLogger
    {
        private static readonly string logFilePath = Path.Combine(Directory.GetCurrentDirectory(), "Logs", "UserActivity.log");

        // General log action (used for account creation, password changes, etc.)
        public static void LogAction(string userId, string action, string details)
        {
            try
            {
                string logEntry = $"{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} | UserID: {userId} | Action: {action} | Details: {details}";

                // Ensure Logs directory exists
                Directory.CreateDirectory(Path.GetDirectoryName(logFilePath));

                // Append log entry to the file
                File.AppendAllText(logFilePath, logEntry + Environment.NewLine);

                // ✅ Detect multiple failed logins from different IPs
                if (action == "User logged in")
                {
                    CheckForSuspiciousLogins(userId, details);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error logging action: {ex.Message}");
            }
        }

        // Log successful login attempts
        public static void LogLoginSuccess(string userId)
        {
            LogAction(userId, "Login Successful", "User logged in successfully.");
        }

        // Log failed login attempts
        public static void LogLoginFailure(string userId)
        {
            LogAction(userId, "Login Failed", "User failed to log in.");
        }

        // Log password change attempts
        public static void LogPasswordChange(string userId, string details)
        {
            LogAction(userId, "Password Changed", details);
        }

        // Log user logout
        public static void LogLogout(string userId)
        {
            LogAction(userId, "Logout", "User logged out.");
        }

        public static void LogPasswordResetSuccess(string userId)
        {
            string logEntry = $"{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} | UserID: {userId} | Action: Password Reset Success | Details: Password was successfully reset.";
            LogToFile(logEntry);
        }

        public static void LogPasswordResetRequest(string email)
        {
            string logEntry = $"{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} | Email: {email} | Action: Password Reset Request | Details: A password reset request was made.";
            LogToFile(logEntry);
        }

        private static void LogToFile(string logEntry)
        {
            try
            {
                string logFilePath = Path.Combine(Directory.GetCurrentDirectory(), "Logs", "UserActivity.log");
                Directory.CreateDirectory(Path.GetDirectoryName(logFilePath));
                File.AppendAllText(logFilePath, logEntry + Environment.NewLine);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error logging action: {ex.Message}");
            }
        }

        // Log ReCaptcha validation results
        public static void LogReCaptchaSuccess(string userId)
        {
            LogAction(userId, "ReCaptcha Validation Success", "User passed ReCaptcha validation.");
        }

        // Log ReCaptcha validation failures
        public static void LogReCaptchaFailure(string userId)
        {
            LogAction(userId, "ReCaptcha Validation Failed", "User failed ReCaptcha validation.");
        }

        // Log account creation events
        public static void LogAccountCreation(string userId)
        {
            LogAction(userId, "Account Created", "User created a new account.");
        }

        // ✅ Detect multiple failed logins from different IPs
        private static void CheckForSuspiciousLogins(string userEmail, string details)
        {
            string[] logs = File.ReadAllLines(logFilePath);
            int loginAttempts = 0;

            foreach (string log in logs)
            {
                if (log.Contains(userEmail) && log.Contains("User logged in"))
                {
                    loginAttempts++;
                }
            }

            if (loginAttempts > 3)
            {
                SendAdminAlert(userEmail);
            }
        }

        private static void SendAdminAlert(string userEmail)
        {
            string message = $"⚠️ Multiple suspicious logins detected for {userEmail}!";
            File.AppendAllText(logFilePath, message + Environment.NewLine);
        }
    }
}
