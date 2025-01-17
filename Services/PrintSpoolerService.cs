using IPrint.Entities;
using IPrint.Repositories;

namespace IPrint.Services
{
	public class PrintSpoolerService()
	{
		private static readonly PrintSpoolerRepository printSpoolerRepository = new();
		public async Task<IEnumerable<PrintSpooler>> GetPendingAsync()
		{
			return await printSpoolerRepository.GetPendingAsync();
		}

		public async Task UpdateStatusAsync(int status, int idEmpresa)
		{
			await printSpoolerRepository.UpdateStatusAsync(status, idEmpresa);
		}
	}
}
