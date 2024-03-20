using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.TypeConversion;

namespace DataUtils
{
    public class WorkingHoursConverter : ITypeConverter
    {
        public object ConvertFromString(string text, IReaderRow row, MemberMapData memberMapData)
        {
            if (text.ToLower() == "круглосуточно")
            {
                return new WorkingHours(TimeSpan.MinValue, TimeSpan.MaxValue);
            }
            try
            {
                var times = text.Split('-');
                if (times.Length != 2)
                {
                    throw new InvalidOperationException("Invalid working hours format.");
                }

                var startTime = TimeSpan.Parse(times[0]);
                var endTime = TimeSpan.Parse(times[1]);

                return new WorkingHours(startTime, endTime);
            }
            catch (Exception ex)
            {
                throw new ArgumentException($"Не удалось преобразовать строку \"{text}\" к формату промежутка времени: {ex.Message}");
            }
        }

        public string ConvertToString(object value, IWriterRow row, MemberMapData memberMapData)
        {
            if (value is WorkingHours workingHours)
            {
                if (workingHours.StartTime == TimeSpan.MinValue && workingHours.EndTime == TimeSpan.MaxValue)
                {
                    return "круглосуточно";
                }
                else
                {
                    return workingHours.ToString();
                }
            }

            throw new InvalidOperationException("Объект должен быть типа WorkinHours.");
        }
    }
}
