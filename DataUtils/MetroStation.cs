using CsvHelper.Configuration.Attributes;
using System.Text.Json.Serialization;

namespace DataUtils
{
    public class MetroStation
    {
        [JsonPropertyName("object_category_Id")]
        [Name("object_category_Id")]
        public int? ObjectCategoryId { get; set; }

        [JsonPropertyName("ID")]
        [Name("ID")]
        public int id { get; set; }

        [JsonPropertyName("Name")]
        [Name("Name")]
        public string? Name { get; set; }

        [JsonPropertyName("Station")]
        [Name("Station")]
        public string? Station { get; set; }

        [JsonPropertyName("RailwayLine")]
        [Name("RailwayLine")]
        public string? RailwayLine { get; set; }

        [JsonPropertyName("WorkingHours")]
        [Name("WorkingHours")]
        [TypeConverter(typeof(WorkingHoursConverter))]
        public string? WorkingHours { get; set; }

        [JsonPropertyName("Latitude_WGS84")]
        [Name("Latitude_WGS84")]
        public double Latitude_WGS84 { get; set; }

        [JsonPropertyName("Longitude_WGS84")]
        [Name("Longitude_WGS84")]
        public double Longitude_WGS84 { get; set; }

        [JsonPropertyName("ObjectStatus")]
        [Name("ObjectStatus")]
        public string? ObjectStatus { get; set; }

        [JsonPropertyName("global_id")]
        [Name("global_id")]
        public long GlobalId { get; set; }

        [JsonPropertyName("geodata_center")]
        [Name("geodata_center")]
        public string? GeodataCenter { get; set; }

        [JsonPropertyName("geoarea")]
        [Name("geoarea")]
        public string? Geoarea { get; set; }
    }
}