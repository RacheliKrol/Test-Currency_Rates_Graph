using Rachel_Krol_Test.Models;

namespace Rachel_Krol_Test.Services.Interfaces
{
    public interface ICurrencyService
    {
        Task<List<CurrencyRate>> GetRatesForYear(int monthNum);
        Task<List<CurrencyRate>> GetRatesByDateRange(DateTime start, DateTime end);
    }
}
