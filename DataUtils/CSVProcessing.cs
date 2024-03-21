using CsvHelper;
using CsvHelper.Configuration;
using System.Globalization;

namespace DataUtils
{
    public class CSVProcessing
    {
        private static CSVProcessing? s_instance = null;
        private static object s_instanceLock = new object();
        private protected CSVProcessing() { }

        public static CSVProcessing GetInstance()
        {
            // Такая реалиизация позволяет избежать проблем при использовании многопоточности.
            if (s_instance is null)
            {
                lock (s_instanceLock)
                {
                    if (s_instance is null)
                    {
                        s_instance = new CSVProcessing();
                    }
                }
            }
            return s_instance;
        }

        public static async Task<List<MetroStation>> Read(MemoryStream stream)
        {
            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                Delimiter = ",",
                HasHeaderRecord = true
            };

            using (var reader = new StreamReader(stream))
            {
                using var csv = new CsvReader(reader, config);

                await csv.ReadAsync();
                csv.ReadHeader();
                await csv.ReadAsync();

                return csv.GetRecords<MetroStation>().ToList();
            }
        }
    }
}
