using IPrint.Converters.JsonConverter;
using System.Text.Json.Serialization;

namespace IPrint.Models
{
    public class LabelProfileConditional
    {
        public string Key {  get; set; }
        [JsonConverter(typeof(EnumStringConverter<Operators>))]
        public Operators Operator { get; set; }
        public string Value { get; set; }
    }
}
