using CsvHelper.Configuration;
using DataUtils;
using Newtonsoft.Json;
using System.Globalization;

namespace RailwayBot
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                Delimiter = ";",
                HasHeaderRecord = true
            };

            var stations = new List<MetroStation>();
            using var stream = new StreamReader(@"C:\Users\79787\Downloads\csvjson2.json");
            /*using (var csv = new CsvReader(reader, config))
            {
                csv.Read();
                Console.WriteLine(csv.ReadHeader());
                csv.Read();

                stations = csv.GetRecords<MetroStation>().ToList();
            }*/
            string jsonString = stream.ReadToEnd();
            stations = JsonConvert.DeserializeObject<List<MetroStation>>(jsonString);

            foreach (var st in stations)
            {
                Console.WriteLine(st.id);
                Console.WriteLine(st.WorkingHours);
            }
        }
    }
}