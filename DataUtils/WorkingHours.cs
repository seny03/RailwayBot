namespace DataUtils
{
    public class WorkingHours
    {
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }

        public WorkingHours(TimeSpan startTime, TimeSpan endTime)
        {
            StartTime = startTime;
            EndTime = endTime;
        }

        public WorkingHours() : this(DateTime.Now.TimeOfDay, DateTime.Now.TimeOfDay) { }
        public override string ToString()
        {
            return $"{StartTime:hh\\:mm}-{EndTime:hh\\:mm}";
        }
    }
}
