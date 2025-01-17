namespace IPrint.Services
{
	public class FileStorageService
	{
		public void Save(string path, byte[] content, bool createIfNotExists = false)
		{
			if(createIfNotExists)
			{
				string? basePath = Path.GetDirectoryName(path);

				if(!string.IsNullOrEmpty(basePath) && !Directory.Exists(basePath))
				{
					Directory.CreateDirectory(basePath);
				}
			}

			File.WriteAllBytes(path, content);
		}

		public byte[] Get(string path)
		{
			byte[] content = new byte[] { };

			try
			{
				content = File.ReadAllBytes(path);
			}
			catch (IOException)
			{
				//ignore
			}

            return content;
		}
	}
}
