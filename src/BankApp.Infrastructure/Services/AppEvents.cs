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
            public static int CustomerId { get; set; }
            public static int ActiveAccountId { get; set; }
            public static string Username { get; set; } = string.Empty;
            public static string Role { get; set; } = "User";
            public static bool IsAdmin => Role == "Admin";

            public static void Set(int userId, string username, string role)
            {
                UserId = userId;
                Username = username;
                Role = role;
            }
            
            public static void SetCustomer(int customerId, int defaultAccountId)
            {
                CustomerId = customerId;
                ActiveAccountId = defaultAccountId;
                System.Diagnostics.Debug.WriteLine($"[CRITICAL] Session.SetCustomer customerId={customerId} activeAccountId={defaultAccountId}");
            }
            
            public static void SetActiveAccount(int accountId)
            {
                var oldId = ActiveAccountId;
                ActiveAccountId = accountId;
                System.Diagnostics.Debug.WriteLine($"[CRITICAL] ActiveAccountChanged old={oldId} new={accountId}");
                ActiveAccountChanged?.Invoke(null, new ActiveAccountChangedEventArgs(oldId, accountId));
            }

            public static void Clear()
            {
                UserId = 0;
                CustomerId = 0;
                ActiveAccountId = 0;
                Username = string.Empty;
                Role = "User";
            }
        }
        
        /// <summary>
        /// Aktif hesap değiştiğinde fırlatılır
        /// </summary>
        public static event EventHandler<ActiveAccountChangedEventArgs>? ActiveAccountChanged;
        
        /// <summary>
        /// Trade tamamlandığında fırlatılır - RefreshPipeline tetikler
        /// </summary>
        public static event EventHandler<TradeCompletedEventArgs>? TradeCompleted;
        
        /// <summary>
        /// Trade tamamlandı bildirimi - TEK REFRESH PIPELINE
        /// </summary>
        public static void NotifyTradeCompleted(int accountId, int customerId, string symbol, decimal amount, bool isBuy)
        {
            System.Diagnostics.Debug.WriteLine($"[CRITICAL] TradeCommitted accountId={accountId} customerId={customerId} symbol={symbol} amount={amount} isBuy={isBuy}");
            System.Diagnostics.Debug.WriteLine($"[CRITICAL] RefreshPipeline START reason=Trade");
            
            TradeCompleted?.Invoke(null, new TradeCompletedEventArgs(accountId, customerId, symbol, amount, isBuy));
            NotifyDataChanged("Trade", isBuy ? "Buy" : "Sell");
            
            System.Diagnostics.Debug.WriteLine($"[CRITICAL] RefreshPipeline END");
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
    
    public class ActiveAccountChangedEventArgs : EventArgs
    {
        public int OldAccountId { get; }
        public int NewAccountId { get; }
        public DateTime Timestamp { get; }

        public ActiveAccountChangedEventArgs(int oldAccountId, int newAccountId)
        {
            OldAccountId = oldAccountId;
            NewAccountId = newAccountId;
            Timestamp = DateTime.Now;
        }
    }
    
    public class TradeCompletedEventArgs : EventArgs
    {
        public int AccountId { get; }
        public int CustomerId { get; }
        public string Symbol { get; }
        public decimal Amount { get; }
        public bool IsBuy { get; }
        public DateTime Timestamp { get; }

        public TradeCompletedEventArgs(int accountId, int customerId, string symbol, decimal amount, bool isBuy)
        {
            AccountId = accountId;
            CustomerId = customerId;
            Symbol = symbol;
            Amount = amount;
            IsBuy = isBuy;
            Timestamp = DateTime.Now;
        }
    }
}
