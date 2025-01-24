using IPrint.Models;
using System.Text;
using System.Text.Json;

namespace IPrint.Services
{
    public class UserConfigService(FileStorageService fileStorageService)
    {
        private static readonly string FILE_CONFIG_PATH = Path.Combine(FileSystem.AppDataDirectory, "user", "config.json");

        public void Add(UserConfig userConfig)
        {
            string jsonConfigs = JsonSerializer.Serialize(userConfig);
            byte[] content = Encoding.UTF8.GetBytes(jsonConfigs);
            fileStorageService.Save(FILE_CONFIG_PATH, content, true);
        }

        public UserConfig Get()
        {
            UserConfig result = null;
            byte[] content = fileStorageService.Get(FILE_CONFIG_PATH);

            if (content.Length > 0)
            {
                string jsonConfigs = Encoding.UTF8.GetString(content);
                result = JsonSerializer.Deserialize<UserConfig>(jsonConfigs);
            }

            return result;
        }
    }
}
