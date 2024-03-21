using Newtonsoft.Json;

namespace DataUtils
{
    public class JSONProcessing
    {
        private static JSONProcessing? s_instance = null;
        private static object s_instanceLock = new object();
        private protected JSONProcessing() { }

        public static JSONProcessing GetInstance()
        {
            // Такая реалиизация позволяет избежать проблем при использовании многопоточности.
            if (s_instance is null)
            {
                lock (s_instanceLock)
                {
                    if (s_instance is null)
                    {
                        s_instance = new JSONProcessing();
                    }
                }
            }
            return s_instance;
        }

        public static async Task<List<MetroStation>> Read(MemoryStream stream)
        {
            using (var reader = new StreamReader(stream))
            {
                string jsonString = await reader.ReadToEndAsync();
                return JsonConvert.DeserializeObject<List<MetroStation>>(jsonString)!;
            }
        }
    }
}
