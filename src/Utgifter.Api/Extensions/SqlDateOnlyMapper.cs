using System.Data;
using Dapper;

namespace Utgifter.Api.Extensions;

public class SqlDateOnlyMapper: SqlMapper.TypeHandler<DateOnly> // Dapper handler for DateOnly
{
    public override DateOnly Parse(object value) => DateOnly.FromDateTime((DateTime)value);

    public override void SetValue(IDbDataParameter parameter, DateOnly value)
    {
        parameter.DbType = DbType.Date;
        parameter.Value = value;
    }
}

