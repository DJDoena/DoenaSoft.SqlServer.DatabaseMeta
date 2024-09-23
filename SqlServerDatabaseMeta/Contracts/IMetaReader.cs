using System.Collections.Generic;
using System.Data.SqlClient;

namespace DoenaSoft.SqlServerDatabaseMeta;

/// <summary>
/// Interface to represent the actual <see cref="MetaReader"/>. Interface can be used for mocking / testing purposes.
/// </summary>
public interface IMetaReader
{
    /// <summary>
    /// Opens a SQL server database connection with the given parameters and extracts all the meta information about tables, columns, etc.
    /// </summary>
    /// <param name="server">server name</param>
    /// <param name="database">database / catalog name</param>
    /// <param name="user">user name</param>
    /// <param name="password">password</param>
    /// <returns>the meta information</returns>
    IReadOnlyList<ITableMeta> Read(string server, string database, string user, string password);

    /// <summary>
    /// Opens a SQL server database connection with the given <paramref name="connectionString">connection string</paramref> and extracts all the meta information about tables, columns, etc.
    /// </summary>
    /// <param name="connectionString">connection string</param>
    /// <returns>the meta information</returns>
    IReadOnlyList<ITableMeta> Read(string connectionString);

    /// <summary>
    /// Uses and already opened database connection and extracts all the meta information about tables, columns, etc.
    /// </summary>
    /// <param name="openConnection">open SQL server connection</param>
    /// <returns>the meta information</returns>
    IReadOnlyList<ITableMeta> Read(SqlConnection openConnection);
}