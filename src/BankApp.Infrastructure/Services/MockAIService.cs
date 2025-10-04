using BankApp.Core.Interfaces;
using System.Threading.Tasks;

namespace BankApp.Infrastructure.Services
{
    public class MockAIService : IAIService
    {
        public Task<string> GetFinancialAdviceAsync(int userId)
        {
             // Returns generic financial advice for now
             return Task.FromResult("Yapay zeka önerisi: Harcamalarınız bu ay %20 arttı. Tasarruf yapmak için restoran harcamalarını azaltmayı düşünebilirsiniz.");
        }

        public Task<string> GetResponseAsync(string query)
        {
             // Echo or simple response for now
             return Task.FromResult($"AI Yanıtı: '{query}' ile ilgili şu an bir önerim yok ancak harcamalarınız dengeli görünüyor.");
        }
    }
}
