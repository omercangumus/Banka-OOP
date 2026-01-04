using System;
using System.Threading.Tasks;
using BankApp.Infrastructure.Services;

namespace BankApp.UI
{
    public static class TestEmail
    {
        public static async Task SendTestEmail()
        {
            try
            {
                var emailService = new SmtpEmailService();
                await emailService.SendEmailAsync(
                    "novabank.com@gmail.com", 
                    "NovaBank Test Email", 
                    "<h1>NovaBank Admin Panel Test</h1><p>Email sending is working correctly!</p><p>Sent at: " + DateTime.Now + "</p>"
                );
                
                System.Windows.Forms.MessageBox.Show("Test email sent successfully!", "Success", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show($"Test email failed: {ex.Message}", "Error", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
            }
        }
    }
}
