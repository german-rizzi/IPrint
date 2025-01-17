using Blazorise.Extensions;
using IPrint.Exceptions;
using IPrint.Models;
using System.Text;
using System.Text.Json;

namespace IPrint.Services
{
    public class PrinterConfigService(FileStorageService fileStorageService, PrinterService printerService)
	{
		private static readonly string FILE_CONFIG_PATH = Path.Combine("user", "printers", "config.json");

		public void Add(PrinterConfig printerConfig)
		{
            var systemPrinter = GetSystemPrinter(printerConfig.Name);
            printerConfig.Id = systemPrinter.Id;

            var configs = GetAll();

            if (configs.Any(e => e.Id == printerConfig.Id))
            {
                throw new AlertException("Impresora ya existente");
            }

            configs.Add(printerConfig);
            SaveConfig(configs);
        }

        public void Update(PrinterConfig printerConfig)
        {
            Update(new List<PrinterConfig>() { printerConfig });
        }

        public void Update(List<PrinterConfig> printerConfigs)
        {
            var configs = GetAll();

            foreach (var printerConfig in printerConfigs)
            {
                var exists = configs.First(e => e.Id.Equals(printerConfig.Id));

                if (exists is not null)
                {
                    var index = configs.IndexOf(exists);
                    configs[index] = printerConfig;
                }
            }

            SaveConfig(configs);
        }

        public List<PrinterConfig> GetAll()
		{
			var result = new List<PrinterConfig>();
			string appPath = AppContext.BaseDirectory;
			string filePath = Path.Combine(appPath, FILE_CONFIG_PATH);

			byte[] content = fileStorageService.Get(filePath);
			
			if(content.Length > 0)
			{
				string jsonConfigs = Encoding.UTF8.GetString(content);

				if (!jsonConfigs.IsNullOrEmpty())
				{
                    result = JsonSerializer.Deserialize<List<PrinterConfig>>(jsonConfigs);
                }
			}

			return result;
		}

        public void Delete(string id)
        {
            var configs = GetAll();
            configs = configs.Where(e => !e.Id.Equals(id)).ToList();
            SaveConfig(configs);
        }

        private void SaveConfig(List<PrinterConfig> configs)
        {
            string jsonConfigs = JsonSerializer.Serialize(configs);
            byte[] content = Encoding.UTF8.GetBytes(jsonConfigs);

            string appPath = AppContext.BaseDirectory;
            string filePath = Path.Combine(appPath, FILE_CONFIG_PATH);
            fileStorageService.Save(filePath, content, true);
        }

        private SystemPrinter GetSystemPrinter(string name)
        {
            var printer = printerService.Find(name);

            if (printer is null)
            {
                throw new AlertException("No se encontró impresora en el sistema");
            }

            return printer;
        }
    }
}
