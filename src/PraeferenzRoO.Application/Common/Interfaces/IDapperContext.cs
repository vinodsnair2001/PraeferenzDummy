using System.Data;

namespace PraeferenzRoO.Application.Common.Interfaces;

public interface IDapperContext
{
    IDbConnection CreateConnection();
}
