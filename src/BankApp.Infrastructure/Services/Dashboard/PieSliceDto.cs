namespace BankApp.Infrastructure.Services.Dashboard
{
    /// <summary>
    /// Pasta grafiği dilimi için DTO
    /// </summary>
    public class PieSliceDto
    {
        public string Category { get; set; }
        public decimal Amount { get; set; }
        public double Percentage { get; set; }
    }
}
