using Blazorise.Extensions;
using IPrint.Exceptions;
using IPrint.Models;
using System.Text;
using System.Text.Json;

namespace IPrint.Services
{
    public class PrinterConfigService(FileStorageService fileStorageService, PrinterService printerService)
	{
		private static readonly string FILE_CONFIG_PATH = Path.Combine(FileSystem.AppDataDirectory, "user", "printers", "config.json");

		public void Add(PrinterConfig printerConfig)
		{
            var configs = GetAll();

            if (configs.Any(e => e.Name == printerConfig.Name))
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
                var exists = configs.First(e => e.Name.Equals(printerConfig.Name));

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
			byte[] content = fileStorageService.Get(FILE_CONFIG_PATH);
			
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

        public void Delete(string name)
        {
            var configs = GetAll();
            configs = configs.Where(e => !e.Name.Equals(name)).ToList();
            SaveConfig(configs);
        }

        private void SaveConfig(List<PrinterConfig> configs)
        {
            string jsonConfigs = JsonSerializer.Serialize(configs);
            byte[] content = Encoding.UTF8.GetBytes(jsonConfigs);
            fileStorageService.Save(FILE_CONFIG_PATH, content, true);
        }
    }
}
