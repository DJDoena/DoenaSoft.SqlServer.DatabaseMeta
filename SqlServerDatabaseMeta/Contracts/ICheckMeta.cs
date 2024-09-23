namespace DoenaSoft.SqlServerDatabaseMeta;

/// <summary>
/// Represents a check constraint on a <see cref="ITableMeta">table</see>.
/// </summary>
public interface ICheckMeta : IMetaBase
{
    /// <summary>
    /// The table this check is implemented on.
    /// </summary>
    ITableMeta Table { get; }

    /// <summary>
    /// The SQL text the check is executing.
    /// </summary>
    string Check { get; }
}