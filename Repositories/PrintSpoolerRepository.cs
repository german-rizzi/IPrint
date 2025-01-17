using Dapper;
using IPrint.Entities;
using System.Data.SqlClient;

namespace IPrint.Repositories
{
	public class PrintSpoolerRepository
	{
		private readonly string _connectionString;

		public PrintSpoolerRepository()
		{
			_connectionString = "";
		}

		public async Task<IEnumerable<PrintSpooler>> GetPendingAsync()
		{
			using var connection = new SqlConnection(_connectionString);
			string query = "SELECT * FROM print_spooler WHERE procesado = 0";
			return await connection.QueryAsync<PrintSpooler>(query);
		}

		public async Task UpdateStatusAsync(int status, int idEmpresa)
		{
			using var connection = new SqlConnection(_connectionString);
			string query = "UPDATE print_spooler SET procesado = @Procesado, intentos = intentos + 1 WHERE idEmpresa = @IdEmpresa";
			await connection.ExecuteAsync(query, new { Procesado = status, IdEmpresa = idEmpresa });
		}
	}
}
