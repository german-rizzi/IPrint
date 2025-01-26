namespace IPrint.Entities
{
	public class PrintSpooler
	{
		public string Id { get; set; }
        public int PrinterId { get; set; }
		public string Url { get; set; }
		public bool Porait { get; set; }
		public int Printed { get; set; }
        public int EquipId { get; set; }
        public int CompanyId { get; set; }
    }
}
