using System.Threading.Tasks;

namespace BankApp.Core.Interfaces
{
    public interface IAIService
    {
        Task<string> GetFinancialAdviceAsync(int userId);
        Task<string> GetResponseAsync(string query);
    }
}
