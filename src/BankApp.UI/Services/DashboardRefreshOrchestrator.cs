using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BankApp.UI.Services
{
    /// <summary>
    /// Merkezi dashboard refresh koordinatörü
    /// Debounce + CancellationToken + Concurrency Guard
    /// </summary>
    public sealed class DashboardRefreshOrchestrator : IDisposable
    {
        private static DashboardRefreshOrchestrator _instance;
        private static readonly object _lock = new object();
        
        private CancellationTokenSource _cts;
        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);
        private readonly int _debounceMs;
        private DateTime _lastRequestTime = DateTime.MinValue;
        private bool _disposed = false;
        
        /// <summary>
        /// Dashboard yenilendiğinde tetiklenir
        /// </summary>
        public event EventHandler<DashboardRefreshEventArgs> DashboardRefreshed;
        
        /// <summary>
        /// Refresh isteği geldiğinde tetiklenir (UI loading state için)
        /// </summary>
        public event EventHandler RefreshStarted;
        
        /// <summary>
        /// Singleton instance
        /// </summary>
        public static DashboardRefreshOrchestrator Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        _instance ??= new DashboardRefreshOrchestrator(300);
                    }
                }
                return _instance;
            }
        }
        
        private DashboardRefreshOrchestrator(int debounceMs = 300)
        {
            _debounceMs = debounceMs;
        }
        
        /// <summary>
        /// Dashboard yenileme isteği gönder (debounced)
        /// </summary>
        public async Task RequestRefreshAsync(int customerId, RefreshReason reason = RefreshReason.Manual)
        {
            if (_disposed) return;
            
            // Cancel previous pending request
            _cts?.Cancel();
            _cts = new CancellationTokenSource();
            var token = _cts.Token;
            
            _lastRequestTime = DateTime.Now;
            
            try
            {
                // Debounce wait
                await Task.Delay(_debounceMs, token);
                
                // Concurrency guard
                if (!await _semaphore.WaitAsync(0, token))
                {
                    // Another refresh is in progress, skip
                    return;
                }
                
                try
                {
                    if (token.IsCancellationRequested) return;
                    
                    // Notify refresh started
                    RefreshStarted?.Invoke(this, EventArgs.Empty);
                    
                    // Notify subscribers with refresh data
                    DashboardRefreshed?.Invoke(this, new DashboardRefreshEventArgs
                    {
                        CustomerId = customerId,
                        Reason = reason,
                        Timestamp = DateTime.Now
                    });
                }
                finally
                {
                    _semaphore.Release();
                }
            }
            catch (OperationCanceledException)
            {
                // Debounce cancelled, newer request incoming
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"DashboardRefreshOrchestrator Error: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Immediate refresh (no debounce)
        /// </summary>
        public void RequestImmediateRefresh(int customerId, RefreshReason reason = RefreshReason.Manual)
        {
            if (_disposed) return;
            
            DashboardRefreshed?.Invoke(this, new DashboardRefreshEventArgs
            {
                CustomerId = customerId,
                Reason = reason,
                Timestamp = DateTime.Now
            });
        }
        
        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            
            _cts?.Cancel();
            _cts?.Dispose();
            _semaphore?.Dispose();
        }
    }
    
    /// <summary>
    /// Refresh event args
    /// </summary>
    public class DashboardRefreshEventArgs : EventArgs
    {
        public int CustomerId { get; set; }
        public RefreshReason Reason { get; set; }
        public DateTime Timestamp { get; set; }
    }
    
    /// <summary>
    /// Refresh nedeni
    /// </summary>
    public enum RefreshReason
    {
        Manual,
        Transfer,
        LoanPayment,
        Investment,
        TabChanged,
        TimerTick
    }
}
