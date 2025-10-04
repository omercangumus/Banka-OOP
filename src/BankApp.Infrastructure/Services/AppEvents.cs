using System;

namespace BankApp.Infrastructure.Services
{
    /// <summary>
    /// Uygulama genelinde event hub - Observer Pattern
    /// Dashboard ve diğer formlar buradaki eventleri dinleyerek güncellenecek
    /// </summary>
    public static class AppEvents
    {
        /// <summary>
        /// Veri değiştiğinde fırlatılır (Hisse alım/satım, Para transferi, Kredi onayı vb.)
        /// </summary>
        public static event EventHandler<DataChangedEventArgs>? DataChanged;

        /// <summary>
        /// Kullanıcı giriş yaptığında fırlatılır
        /// </summary>
        public static event EventHandler<UserLoggedInEventArgs>? UserLoggedIn;

        /// <summary>
        /// Veri değişikliği bildirimi gönder
        /// </summary>
        public static void NotifyDataChanged(string source, string action)
        {
            DataChanged?.Invoke(null, new DataChangedEventArgs(source, action));
        }

        /// <summary>
        /// Kullanıcı girişi bildirimi gönder
        /// </summary>
        public static void NotifyUserLoggedIn(int userId, string username, string role)
        {
            UserLoggedIn?.Invoke(null, new UserLoggedInEventArgs(userId, username, role));
        }

        /// <summary>
        /// Mevcut oturum bilgisi
        /// </summary>
        public static class CurrentSession
        {
            public static int UserId { get; set; }
            public static string Username { get; set; } = string.Empty;
            public static string Role { get; set; } = "User";
            public static bool IsAdmin => Role == "Admin";

            public static void Set(int userId, string username, string role)
            {
                UserId = userId;
                Username = username;
                Role = role;
            }

            public static void Clear()
            {
                UserId = 0;
                Username = string.Empty;
                Role = "User";
            }
        }
    }

    public class DataChangedEventArgs : EventArgs
    {
        public string Source { get; }
        public string Action { get; }
        public DateTime Timestamp { get; }

        public DataChangedEventArgs(string source, string action)
        {
            Source = source;
            Action = action;
            Timestamp = DateTime.Now;
        }
    }

    public class UserLoggedInEventArgs : EventArgs
    {
        public int UserId { get; }
        public string Username { get; }
        public string Role { get; }

        public UserLoggedInEventArgs(int userId, string username, string role)
        {
            UserId = userId;
            Username = username;
            Role = role;
        }
    }
}
