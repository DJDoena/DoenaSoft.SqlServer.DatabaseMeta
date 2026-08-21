namespace DoenaSoft.SqlServerDatabaseMeta;

/// <summary>
/// Represents a scalar-valued user-defined function in the database.
/// </summary>
public interface IScalarFunctionMeta : IMetaBase
{
    /// <summary>
    /// The schema the function belongs to (e.g. "dbo").
    /// </summary>
    string Schema { get; }

    /// <summary>
    /// The full CREATE FUNCTION definition text as stored in sys.sql_modules.
    /// </summary>
    string Definition { get; }
}
