using System.Data;
using Microsoft.Extensions.Configuration;
using Npgsql;
using PraeferenzRoO.Application.Common.Interfaces;

namespace PraeferenzRoO.Persistence.Context;

public sealed class DapperContext : IDapperContext
{
    private readonly string _connectionString;

    public DapperContext(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' is not configured.");
    }

    public IDbConnection CreateConnection() => new NpgsqlConnection(_connectionString);
}
