using IPrint.Converters.JsonConverter;
using System.Text.Json.Serialization;

namespace IPrint.Models
{
    public class PrinterConfig
	{
		public string Name { get; set; }
        [JsonConverter(typeof(EnumStringConverter<PrinterTypes>))]
        public PrinterTypes Type { get; set; }
		public List<string> LabelProfileIds { get; set; } = new();
	}
}
