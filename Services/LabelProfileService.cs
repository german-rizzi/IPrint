using IPrint.Exceptions;
using IPrint.Models;
using System.Text;
using System.Text.Json;

namespace IPrint.Services
{
    public class LabelProfileService(FileStorageService fileStorageService, PrinterConfigService printerConfigService)
	{
		private static readonly string FILE_CONFIG_PATH = Path.Combine("user", "labels", "config.json");

        private List<PrinterConfig> printerConfigs { get { return printerConfigService.GetAll(); } }

        public void Add(LabelProfile labelProfile)
		{
			var configs = GetAll();

            if (configs.Any(e => e.Name.Equals(labelProfile.Name, StringComparison.OrdinalIgnoreCase)))
            {
                throw new AlertException("Perfil de impresión ya existente");
            }

            labelProfile.Id = Guid.NewGuid().ToString();
            configs.Add(labelProfile);

            SaveConfig(configs);
            setToPrintersConfigs(labelProfile);
        }

        private void setToPrintersConfigs(LabelProfile labelProfile)
        {
            if (labelProfile.PrinterIds?.Any() ?? false)
            {
                var printerConfigs = printerConfigService.GetAll();

                if (printerConfigs.Any())
                {
                    var selectedPrinterConfigs = printerConfigs.Where(e => labelProfile.PrinterIds.Contains(e.Id) && !(e.LabelProfileIds?.Contains(labelProfile.Id) ?? false)).ToList();
                    
                    if(selectedPrinterConfigs.Any())
                    {
                        selectedPrinterConfigs.ForEach(e => e.LabelProfileIds.Add(labelProfile.Id));
                        printerConfigService.Update(selectedPrinterConfigs);
                    }
                }
            }
        }

        public void Update(LabelProfile labelProfile)
        {
            Update(new List<LabelProfile>() { labelProfile });
        }

        public void Update(List<LabelProfile> labelProfiles)
        {
            var configs = GetAll();

            foreach (var labelProfile in labelProfiles)
            {
                var exists = configs.First(e => e.Id.Equals(labelProfile.Id));

                if (exists is not null)
                {
                    var index = configs.IndexOf(exists);
                    configs[index] = labelProfile;
                    setToPrintersConfigs(labelProfile);
                }
            }

            SaveConfig(configs);
        }

        public List<LabelProfile> GetAll()
		{
			var result = new List<LabelProfile>();
			string appPath = AppContext.BaseDirectory;
			string filePath = Path.Combine(appPath, FILE_CONFIG_PATH);

			byte[] content = fileStorageService.Get(filePath);
			
			if(content.Length > 0)
			{
				string jsonConfigs = Encoding.UTF8.GetString(content);
				result = JsonSerializer.Deserialize<List<LabelProfile>>(jsonConfigs);
			}

			return result;
		}

        public void Delete(string id)
        {
            var configs = GetAll();
			configs = configs.Where(e => !e.Id.Equals(id)).ToList();
			SaveConfig(configs);
        }

		private void SaveConfig(List<LabelProfile> configs)
		{
            string jsonConfigs = JsonSerializer.Serialize(configs);
            byte[] content = Encoding.UTF8.GetBytes(jsonConfigs);

            string appPath = AppContext.BaseDirectory;
            string filePath = Path.Combine(appPath, FILE_CONFIG_PATH);
            fileStorageService.Save(filePath, content, true);
        }
    }
}
