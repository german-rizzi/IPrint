using IPrint.Converters.JsonConverter;
using System.Text.Json.Serialization;

namespace IPrint.Models
{
    public class PrinterConfig
	{
		public int Id { get; set; }
		public string Name { get; set; }
        [JsonConverter(typeof(EnumStringConverter<LabelTypes>))]
        public LabelTypes Type { get; set; }
		public List<string> LabelProfileIds { get; set; } = new();
	}
}
