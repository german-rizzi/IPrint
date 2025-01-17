namespace IPrint.Entities
{
	public class PrintSpooler
	{
		public int IdEmpresa { get; set; }
		public string Url { get; set; }
		public bool Porait { get; set; }
		public string Courier { get; set; }
		public int Procesado { get; set; }
		public int Intentos { get; set; }
		public int IdEquipo { get; set; }
	}
}
