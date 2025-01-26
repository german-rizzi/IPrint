using System.Text.Json.Serialization;

namespace IPrint.Models
{
    public class LabelProfile
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public LabelConfig Configuration { get; set; }
        public List<LabelProfileConditional> Conditionals { get; set; }
        [JsonIgnore]
        public List<string> Printers { get; set; }
    }
}
