using Dapper;
using IPrint.Entities;
using MySqlConnector;

namespace IPrint.Repositories
{
	public class PrintSpoolerRepository
	{
		private readonly string _connectionString;

		public PrintSpoolerRepository()
		{
			_connectionString = "Server=127.0.0.1;Port=33073;Database=encuentra_api_dev;User ID=root;Password=Encu3ntrA.;";
		}

		public async Task<IEnumerable<PrintSpooler>> GetByStatusAsync(int status, int companyId)
		{
			await using var connection = new MySqlConnection(_connectionString);
			await connection.OpenAsync();
			string query = "SELECT Id, UrlArchivo as Url, Printed, Porait, PrinterId, IdEquipo as EquipId, IdEmpresa as CompanyId  FROM print_spooler WHERE idEmpresa = @CompanyId and printed = @Printed";
			return await connection.QueryAsync<PrintSpooler>(query, new { CompanyId = companyId, Printed = status });
		}

		public async Task UpdateStatusAsync(string id, int status, int companyId)
		{
			await using var connection = new MySqlConnection(_connectionString);
			await connection.OpenAsync();
			string query = "UPDATE print_spooler SET printed = @Printed, intentos = intentos + 1 WHERE idEmpresa = @CompanyId and id = @Id";
			await connection.ExecuteAsync(query, new { Id = id, CompanyId = companyId, Printed = status });
		}
	}
}
