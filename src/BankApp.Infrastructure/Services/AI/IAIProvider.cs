using System.Collections.Generic;
using System.Threading.Tasks;

namespace BankApp.Infrastructure.Services.AI
{
    /// <summary>
    /// Chat message for conversation history
    /// </summary>
    public class ChatMessage
    {
        public string Role { get; set; } = string.Empty; // "user" or "assistant"
        public string Content { get; set; } = string.Empty;
    }
    
    /// <summary>
    /// AI Provider interface for LLM interactions
    /// </summary>
    public interface IAIProvider
    {
        /// <summary>
        /// Send a request to the AI and get a response
        /// </summary>
        Task<string> AskAsync(AiRequest request);
        
        /// <summary>
        /// Check if the provider is available (has valid API key, etc.)
        /// </summary>
        bool IsAvailable { get; }
        
        /// <summary>
        /// Provider name for display
        /// </summary>
        string ProviderName { get; }
        
        /// <summary>
        /// Clear conversation history
        /// </summary>
        void ClearHistory();
    }
    
    /// <summary>
    /// AI Request model containing user message and context
    /// </summary>
    public class AiRequest
    {
        public string UserMessage { get; set; } = string.Empty;
        public string Topic { get; set; } = string.Empty;
        public AiContext Context { get; set; } = new AiContext();
    }
    
    /// <summary>
    /// Context data for AI to analyze
    /// </summary>
    public class AiContext
    {
        // Portfolio data
        public decimal NetWorth { get; set; }
        public decimal TotalProfit { get; set; }
        public decimal ProfitPercent { get; set; }
        public decimal TotalPortfolioValue { get; set; }
        
        // Transaction data
        public int RecentTransactionCount { get; set; }
        public decimal TotalSpending { get; set; }
        public string SpendingByCategory { get; set; } = string.Empty;
        public string RecentTransactions { get; set; } = string.Empty;
        
        // Account data
        public decimal TotalBalance { get; set; }
        public int AccountCount { get; set; }
        
        // Stock data (for market analysis)
        public string StockSymbol { get; set; } = string.Empty;
        public double StockPrice { get; set; }
        public double StockChange { get; set; }
        public double StockChangePercent { get; set; }
        public string StockNews { get; set; } = string.Empty;
        public List<StockData> StockData { get; set; } = new List<StockData>();
        
        // User info
        public string Username { get; set; } = string.Empty;
    }
    
    /// <summary>
    /// Stock data for AI analysis
    /// </summary>
    public class StockData
    {
        public string Symbol { get; set; } = string.Empty;
        public decimal CurrentPrice { get; set; }
        public decimal ChangePercent { get; set; }
        public decimal Volume { get; set; }
    }
}
