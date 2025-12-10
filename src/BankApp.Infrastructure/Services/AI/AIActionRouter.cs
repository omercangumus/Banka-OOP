using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace BankApp.Infrastructure.Services.AI
{
    /// <summary>
    /// AI Action Router - Parses AI responses for tool calls and executes actions
    /// </summary>
    public class AIActionRouter
    {
        public event EventHandler<ActionEventArgs>? ActionRequested;
        public event EventHandler<ConfirmationEventArgs>? ConfirmationRequired;
        
        private static readonly HashSet<string> ConfirmationRequiredActions = new()
        {
            "StartTransfer",
            "StartLoanPayment",
            "PrepareTrade"
        };

        /// <summary>
        /// Parse AI response for tool calls
        /// </summary>
        public AIToolCall? ParseToolCall(string aiResponse)
        {
            // Look for JSON tool call in response
            // Format: {"action": "Navigate", "params": {"screen": "Dashboard"}}
            var match = Regex.Match(aiResponse, @"\{[\s]*""action""[\s]*:[\s]*""(\w+)""[\s]*,[\s]*""params""[\s]*:[\s]*(\{[^}]+\})[\s]*\}", RegexOptions.Singleline);
            
            if (match.Success)
            {
                try
                {
                    var action = match.Groups[1].Value;
                    var paramsJson = match.Groups[2].Value;
                    var parameters = JsonSerializer.Deserialize<Dictionary<string, object>>(paramsJson);
                    
                    return new AIToolCall
                    {
                        Action = action,
                        Parameters = parameters ?? new Dictionary<string, object>(),
                        RawJson = match.Value
                    };
                }
                catch { }
            }
            
            return null;
        }

        /// <summary>
        /// Execute a tool call action
        /// </summary>
        public async Task<ActionResult> ExecuteActionAsync(AIToolCall toolCall)
        {
            // Check if confirmation required
            if (ConfirmationRequiredActions.Contains(toolCall.Action))
            {
                var confirmArgs = new ConfirmationEventArgs
                {
                    Action = toolCall.Action,
                    Parameters = toolCall.Parameters,
                    Message = GetConfirmationMessage(toolCall)
                };
                
                ConfirmationRequired?.Invoke(this, confirmArgs);
                
                if (!confirmArgs.Confirmed)
                {
                    return new ActionResult
                    {
                        Success = false,
                        Message = "İşlem kullanıcı tarafından iptal edildi.",
                        ActionType = toolCall.Action
                    };
                }
            }
            
            // Execute the action
            var args = new ActionEventArgs
            {
                Action = toolCall.Action,
                Parameters = toolCall.Parameters
            };
            
            ActionRequested?.Invoke(this, args);
            
            return new ActionResult
            {
                Success = args.Handled,
                Message = args.ResultMessage ?? "İşlem tamamlandı.",
                ActionType = toolCall.Action,
                Data = args.ResultData
            };
        }

        private string GetConfirmationMessage(AIToolCall toolCall)
        {
            return toolCall.Action switch
            {
                "StartTransfer" => $"💸 Transfer işlemi başlatılacak. Onaylıyor musunuz?",
                "StartLoanPayment" => $"💳 Kredi ödemesi başlatılacak. Onaylıyor musunuz?",
                "PrepareTrade" => $"📈 Yatırım işlemi hazırlanacak. Onaylıyor musunuz?",
                _ => $"{toolCall.Action} işlemi yapılacak. Onaylıyor musunuz?"
            };
        }

        /// <summary>
        /// Get tool schema for AI system prompt
        /// </summary>
        public static string GetToolSchema()
        {
            return @"
Kullanılabilir Aksiyonlar (JSON formatında döndür):

1. Navigate - Ekran yönlendirme
   {""action"": ""Navigate"", ""params"": {""screen"": ""Dashboard|Portfolio|Investment|LoanPayment|Transfer""}}

2. GetUserSnapshot - Kullanıcı finansal özeti
   {""action"": ""GetUserSnapshot"", ""params"": {}}

3. StartTransfer - Transfer başlat (ONAY GEREKTİRİR)
   {""action"": ""StartTransfer"", ""params"": {""toIban"": ""TR..."", ""amount"": 1000, ""description"": ""Açıklama""}}

4. StartLoanPayment - Kredi ödemesi (ONAY GEREKTİRİR)
   {""action"": ""StartLoanPayment"", ""params"": {""loanId"": 1, ""amount"": 500}}

5. PrepareTrade - Yatırım işlemi hazırla (ONAY GEREKTİRİR)
   {""action"": ""PrepareTrade"", ""params"": {""symbol"": ""AAPL"", ""side"": ""buy|sell"", ""quantity"": 10}}

6. GetChartContext - Grafik bağlamı al
   {""action"": ""GetChartContext"", ""params"": {""symbol"": ""AAPL"", ""timeframe"": ""1D""}}

ÖNEMLİ: Para transferi, ödeme veya alım-satım işlemlerinde kullanıcıdan açık onay al.
";
        }
    }

    public class AIToolCall
    {
        public string Action { get; set; } = "";
        public Dictionary<string, object> Parameters { get; set; } = new();
        public string RawJson { get; set; } = "";
    }

    public class ActionResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = "";
        public string ActionType { get; set; } = "";
        public object? Data { get; set; }
    }

    public class ActionEventArgs : EventArgs
    {
        public string Action { get; set; } = "";
        public Dictionary<string, object> Parameters { get; set; } = new();
        public bool Handled { get; set; }
        public string? ResultMessage { get; set; }
        public object? ResultData { get; set; }
    }

    public class ConfirmationEventArgs : EventArgs
    {
        public string Action { get; set; } = "";
        public Dictionary<string, object> Parameters { get; set; } = new();
        public string Message { get; set; } = "";
        public bool Confirmed { get; set; }
    }
}
