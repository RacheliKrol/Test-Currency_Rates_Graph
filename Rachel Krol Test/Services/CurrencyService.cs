using Rachel_Krol_Test.Models;
using Rachel_Krol_Test.Services.Interfaces;
using System.Xml.Linq;

namespace Rachel_Krol_Test.Services
{
    public class CurrencyService: ICurrencyService
    {
        private readonly HttpClient _httpClient;
        private readonly string[] _currencies = { "USD", "GBP", "SEK", "CHF" };

        public CurrencyService(HttpClient httpClient) 
        {
            _httpClient = httpClient;
        }
        private async Task<Dictionary<(DateTime, string), decimal>> DownloadDataFromApi(DateTime start, DateTime end)
        {
            string xmlResponse = "";
            try 
            {
                string baseUrl = "https://edge.boi.gov.il/FusionEdgeServer/sdmx/v2/data/dataflow/BOI.STATISTICS/EXR/1.0";
                string url = $"{baseUrl}?startperiod={start:yyyy-MM-dd}&endperiod={end:yyyy-MM-dd}";
                xmlResponse = await _httpClient.GetStringAsync(url);
            }
            catch (HttpRequestException e)
            {
                throw new Exception("Error fetching data from the API", e);
            }

            var xmldoc = XDocument.Parse(xmlResponse);
            var seriesList = xmldoc.Descendants().Where(x => x.Name.LocalName == "Series" &&
                                _currencies.Contains(x.Attribute("BASE_CURRENCY")?.Value));
            var dataMap = new Dictionary<(DateTime, string), decimal>();

            foreach (var series in seriesList)
            {
                string currencyCode = series.Attribute("BASE_CURRENCY")?.Value;

                foreach (var obs in series.Elements().Where(e => e.Name.LocalName == "Obs"))
                {
                    DateTime date = DateTime.Parse(obs.Attribute("TIME_PERIOD").Value);
                    decimal value = decimal.Parse(obs.Attribute("OBS_VALUE").Value);

                    dataMap[(date, currencyCode)] = value;
                }
            }
            return dataMap;
        }

        private async Task<decimal?> GetRateByDate(string currency, DateTime date)
        {
            DateTime start = date.AddDays(-5);
            var data = await DownloadDataFromApi(start, date);

            var datesToCheck = Enumerable.Range(1, 5)
                                         .Select(daysBack => date.AddDays(-daysBack))
                                         .OrderByDescending(d => d);

            foreach (var checkDate in datesToCheck)
            {
                if (data.TryGetValue((checkDate, currency), out var value))
                {
                    return value;
                }
            }
            return null;
        }

        public async Task<List<CurrencyRate>> GetRatesByDateRange(DateTime start, DateTime end)
        {
            var data = DownloadDataFromApi(start, end).Result;
            var result = new List<CurrencyRate>();
            foreach (var currency in _currencies)
            {
                decimal? lastValue = null;
                for (DateTime date = start; date <= end; date = date.AddDays(1))
                {
                    if (data.TryGetValue((date, currency), out var value))
                    {
                        lastValue = value;
                    }
                    else if (lastValue == null)// חסר ערך ואין לנו נתונים על ערך קודם.
                    {
                        lastValue = await GetRateByDate(currency, date);                           
                    }

                    result.Add(new CurrencyRate
                    {
                        Date = date,
                        CurrencyName = currency,
                        Value = lastValue.Value
                    });
                }
            }
            return result;
        }

        public async Task<List<CurrencyRate>> GetRatesForYear(int monthNum)
        {
            var result = new List<CurrencyRate>();
            DateTime today = DateTime.Today;
            int day = today.Day;
            for (int i = 0; i < monthNum; i++)
            {
                DateTime monthDate = today.AddMonths(-i);

                int daysInMonth = DateTime.DaysInMonth(monthDate.Year, monthDate.Month);
                int dayToUse = Math.Min(day, daysInMonth);

                DateTime targetDate = new DateTime(monthDate.Year, monthDate.Month, dayToUse);

                foreach (var currency in _currencies)
                {
                    decimal? value = await GetRateByDate(currency, targetDate);

                    if (value != null)
                    {
                        result.Add(new CurrencyRate
                        {
                            Date = targetDate,
                            CurrencyName = currency,
                            Value = value.Value
                        });
                    }
                }
            }
            return result;
        }

    }
}
