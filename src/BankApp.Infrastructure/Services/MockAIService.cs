using System.Text;
using System.Threading.Tasks;
using BankApp.Core.Interfaces;

namespace BankApp.Infrastructure.Services
{
    /// <summary>
    /// Mock AI servisi - Demo yapay zeka yanıtları
    /// Created by Fırat Üniversitesi Standartları, 01/01/2026
    /// </summary>
    public class MockAIService : IAIService
    {
        /// <summary>
        /// Kullanıcıya finansal tavsiye verir
        /// </summary>
        /// <param name="userId">Kullanıcı ID</param>
        /// <returns>Finansal tavsiye metni</returns>
        public Task<string> GetFinancialAdviceAsync(int userId)
        {
            var sb = new StringBuilder();
            sb.Append("Yapay zeka önerisi: Harcamalarınız bu ay %20 arttı. ");
            sb.Append("Tasarruf yapmak için restoran harcamalarını azaltmayı düşünebilirsiniz.");
            return Task.FromResult(sb.ToString());
        }

        /// <summary>
        /// Kullanıcı sorgusuna AI yanıtı verir
        /// </summary>
        /// <param name="query">Kullanıcı sorusu</param>
        /// <returns>AI yanıtı</returns>
        public Task<string> GetResponseAsync(string query)
        {
            var sb = new StringBuilder();
            sb.Append("AI Yanıtı: '");
            sb.Append(query);
            sb.Append("' ile ilgili şu an bir önerim yok ancak harcamalarınız dengeli görünüyor.");
            return Task.FromResult(sb.ToString());
        }
    }
}
