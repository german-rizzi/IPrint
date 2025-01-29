using IPrint.Entities;
using IPrint.Repositories;

namespace IPrint.Services
{
	public class PrintSpoolerService(PrintSpoolerRepository printSpoolerRepository )
	{
		public async Task<IEnumerable<PrintSpooler>> GetByStatusAsync(int status, int equipId, int companyId)
		{
			return await printSpoolerRepository.GetByStatusAsync(status, equipId, companyId);
		}

		public async Task UpdateStatusAsync(string id, int status, int comapnyId)
		{
			await printSpoolerRepository.UpdateStatusAsync(id, status, comapnyId);
		}
	}
}
