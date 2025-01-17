using IPrint.Models;
using System.Text;
using System.Text.Json;

namespace IPrint.Services
{
    public class UserConfigService(FileStorageService fileStorageService)
    {
        private static readonly string FILE_CONFIG_PATH = Path.Combine("user", "config.json");

        public void Add(UserConfig userConfig)
        {
            string jsonConfigs = JsonSerializer.Serialize(userConfig);
            byte[] content = Encoding.UTF8.GetBytes(jsonConfigs);

            string appPath = AppContext.BaseDirectory;
            string filePath = Path.Combine(appPath, FILE_CONFIG_PATH);
            fileStorageService.Save(filePath, content, true);
        }

        public UserConfig Get()
        {
            UserConfig result = null;
            string appPath = AppContext.BaseDirectory;
            string filePath = Path.Combine(appPath, FILE_CONFIG_PATH);

            byte[] content = fileStorageService.Get(filePath);

            if (content.Length > 0)
            {
                string jsonConfigs = Encoding.UTF8.GetString(content);
                result = JsonSerializer.Deserialize<UserConfig>(jsonConfigs);
            }

            return result;
        }
    }
}
