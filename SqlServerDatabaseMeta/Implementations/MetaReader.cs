using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;

namespace DoenaSoft.SqlServerDatabaseMeta;


/// <summary>
/// Extracts all the meta information about tables, columns, etc.
/// </summary>
public class MetaReader : IMetaReader
{
    private Dictionary<string, TableMeta> _meta;

    private SqlConnection _connection;

    /// <summary>
    /// Opens a SQL server database connection with the given parameters and extracts all the meta information about tables, columns, etc.
    /// </summary>
    /// <param name="server">server name</param>
    /// <param name="database">database / catalog name</param>
    /// <param name="user">user name</param>
    /// <param name="password">password</param>
    /// <returns>the meta information</returns>
    public IReadOnlyList<ITableMeta> Read(string server, string database, string user, string password)
        => this.Read($"Data Source={server};Initial Catalog={database};User ID={user};Password={password}");

    /// <summary>
    /// Opens a SQL server database connection with the given <paramref name="connectionString">connection string</paramref> and extracts all the meta information about tables, columns, etc.
    /// </summary>
    /// <param name="connectionString">connection string</param>
    /// <returns>the meta information</returns>
    public IReadOnlyList<ITableMeta> Read(string connectionString)
    {
        using var connection = new SqlConnection(connectionString);

        connection.Open();

        var result = this.Read(connection);

        return result;
    }

    /// <summary>
    /// Uses and already opened database connection and extracts all the meta information about tables, columns, etc.
    /// </summary>
    /// <param name="openConnection">open SQL server connection</param>
    /// <returns>the meta information</returns>
    public virtual IReadOnlyList<ITableMeta> Read(SqlConnection openConnection)
    {
        _connection = openConnection;

        _meta = [];

        this.AddTables();

        this.AddColumns();

        this.AddForeignKeys();

        this.AddIndices();

        this.AddChecks();

        var result = (new List<ITableMeta>(_meta.Values)).AsReadOnly();

        return result;
    }

    private void AddTables()
    {
        using var command = new SqlCommand(Queries.Tables, _connection);

        using var reader = command.ExecuteReader();

        while (reader.Read())
        {
            var tableName = reader.GetString(reader.GetOrdinal("TableName"));
            var type = reader.GetString(reader.GetOrdinal("Type"));
            var description = GetNullableString(reader, "Description");

            _meta.Add(tableName.ToLowerInvariant(), new TableMeta(tableName, description, type));
        }
    }

    private void AddColumns()
    {
        using var command = new SqlCommand(Queries.Columns, _connection);

        using var reader = command.ExecuteReader();

        while (reader.Read())
        {
            this.AddColumn(reader);
        }
    }

    private void AddColumn(SqlDataReader reader)
    {
        var tableName = reader.GetString(reader.GetOrdinal("TableName"));

        var table = _meta[tableName.ToLowerInvariant()];

        var columnName = reader.GetString(reader.GetOrdinal("ColumnName"));
        var columnIndex = reader.GetInt32(reader.GetOrdinal("ColumnIndex"));
        var defaultValue = GetNullableString(reader, "DefaultValue");
        var isNullableInt = GetNullableInt(reader, "IsNullable");
        var isNullable = Convert.ToBoolean(isNullableInt ?? 0);
        var dataType = reader.GetString(reader.GetOrdinal("DataType"));
        var numericPrecision = GetNullableInt(reader, "NumericPrecision");
        var numericScale = GetNullableInt(reader, "NumericScale");
        var maxTextLength = GetNullableInt(reader, "MaxTextLength");
        var textCollation = GetNullableString(reader, "TextCollation");
        var isIdentity = GetNullableBool(reader, "IsIdentity");
        var description = GetNullableString(reader, "Description");
        var columnId = GetNullableInt(reader, "ColumnId");

        table.AddColumn(new ColumnMeta(columnName, description, table, columnIndex, columnId, dataType, defaultValue)
        {
            IsNullable = isNullable,
            IsIdentity = isIdentity,
            NumericPrecision = numericPrecision,
            NumericScale = numericScale,
            MaxTextLength = maxTextLength,
            TextCollation = textCollation,
        });
    }

    private void AddForeignKeys()
    {
        var rows = new List<ForeignKeyRow>();

        using var command = new SqlCommand(Queries.ForeignKeys, _connection);

        using var reader = command.ExecuteReader();

        while (reader.Read())
        {
            rows.Add(new ForeignKeyRow(
                foreignKeyName: reader.GetString(reader.GetOrdinal("ForeignKeyName"))
                , sourceTableName: reader.GetString(reader.GetOrdinal("SourceTableName"))
                , columnName: reader.GetString(reader.GetOrdinal("ColumName"))
                , targetTableName: reader.GetString(reader.GetOrdinal("TargetTableName"))
                , targetTableIndexId: reader.GetInt32(reader.GetOrdinal("TargetTableIndexId"))
                , sourceColumnIndex: reader.GetInt32(reader.GetOrdinal("SourceColumnIndex"))
                , targetColumnIndex: reader.GetInt32(reader.GetOrdinal("TargetColumnIndex"))
                , description: GetNullableString(reader, "Description")));
        }

        var rowGroups = rows.GroupBy(r => new Tuple<string, string>(r.SourceTableName, r.ForeignKeyName));

        foreach (var rowGroup in rowGroups)
        {
            this.AddForeignKey(rowGroup);
        }
    }

    private void AddForeignKey(IEnumerable<ForeignKeyRow> keyGroup)
    {
        var first = keyGroup.First();

        var sourceTable = _meta[first.SourceTableName.ToLowerInvariant()];

        var sourceColumns = keyGroup.Select(key => sourceTable.Columms.First(stc => stc.Name.Equals(key.ColumnName, StringComparison.OrdinalIgnoreCase))).ToList();

        var targetTable = _meta[first.TargetTableName.ToLowerInvariant()];

        var columnReferenceIndexes = keyGroup.Select(key => new ForeignKeyColumnReferenceIndexes(key.SourceColumnIndex, key.TargetColumnIndex)).ToList();

        var foreignKey = new ForeignKeyMeta(first.ForeignKeyName, first.Description, sourceColumns, targetTable, first.TargetTableIndexId, columnReferenceIndexes);

        sourceTable.AddOutgoingForeignKey(foreignKey);

        targetTable.AddIncomingForeignKey(foreignKey);
    }

    private void AddIndices()
    {
        using var command = new SqlCommand(Queries.Indices, _connection);

        using var reader = command.ExecuteReader();

        while (reader.Read())
        {
            this.AddIndex(reader);
        }
    }

    private void AddIndex(SqlDataReader reader)
    {
        var tableName = reader.GetString(reader.GetOrdinal("TableName"));

        var table = _meta[tableName.ToLowerInvariant()];

        var columnNames = reader.GetString(reader.GetOrdinal("Columns"))
            .Split([','], StringSplitOptions.RemoveEmptyEntries)
            .Select(p => p.Trim());

        var columns = columnNames.Select(cn => table.Columms.First(c => c.Name.Equals(cn, StringComparison.OrdinalIgnoreCase))).ToList();

        var rawIncluded = GetNullableString(reader, "IncludedColumns");

        var includedColumns = rawIncluded == null
            ? []
            : rawIncluded.Split([','], StringSplitOptions.RemoveEmptyEntries)
                         .Select(p => p.Trim())
                         .Select(cn => table.Columms.First(c => c.Name.Equals(cn, StringComparison.OrdinalIgnoreCase)))
                         .ToList<IColumnMeta>();

        var propertyTags = reader.GetString(reader.GetOrdinal("Properties"))
            .Split([','], StringSplitOptions.RemoveEmptyEntries)
            .Select(p => p.Trim())
            .ToList();

        var indexId = reader.GetInt32(reader.GetOrdinal("IndexId"));
        var description = GetNullableString(reader, "Description");
        var indexName = reader.GetString(reader.GetOrdinal("IndexName"));

        table.AddIndex(new IndexMeta(indexName, description, table, indexId, columns, includedColumns, propertyTags));
    }

    private void AddChecks()
    {
        using var command = new SqlCommand(Queries.Checks, _connection);

        using var reader = command.ExecuteReader();

        while (reader.Read())
        {
            this.AddCheck(reader);
        }
    }

    private void AddCheck(SqlDataReader reader)
    {
        var tableName = reader.GetString(reader.GetOrdinal("TableName"));

        var table = _meta[tableName.ToLowerInvariant()];

        var checkName = reader.GetString(reader.GetOrdinal("CheckName"));
        var definition = GetNullableString(reader, "Definition");
        var description = GetNullableString(reader, "Description");

        table.AddCheck(new CheckMeta(checkName, description, table, definition));
    }

    /// <inheritdoc/>
    public IReadOnlyList<IScalarFunctionMeta> ReadScalarFunctions(string server, string database, string user, string password)
    {
        var result = this.ReadScalarFunctions($"Data Source={server};Initial Catalog={database};User ID={user};Password={password}");

        return result;
    }

    /// <inheritdoc/>
    public IReadOnlyList<IScalarFunctionMeta> ReadScalarFunctions(string connectionString)
    {
        using var connection = new SqlConnection(connectionString);

        connection.Open();

        var result = this.ReadScalarFunctions(connection);

        return result;
    }

    /// <inheritdoc/>
    public virtual IReadOnlyList<IScalarFunctionMeta> ReadScalarFunctions(SqlConnection openConnection)
    {
        var functions = new List<IScalarFunctionMeta>();

        using var command = new SqlCommand(Queries.ScalarFunctions, openConnection);

        using var reader = command.ExecuteReader();

        while (reader.Read())
        {
            var schema = reader.GetString(reader.GetOrdinal("SchemaName"));
            var name = reader.GetString(reader.GetOrdinal("FunctionName"));
            var definition = GetNullableString(reader, "Definition");
            var description = GetNullableString(reader, "Description");

            functions.Add(new ScalarFunctionMeta(name, description, schema, definition));
        }

        var result = functions.AsReadOnly();

        return result;
    }

    private static string GetNullableString(SqlDataReader reader, string columnName)
    {
        var ordinal = reader.GetOrdinal(columnName);

        return reader.IsDBNull(ordinal)
            ? null
            : reader.GetString(ordinal);
    }

    private static int? GetNullableInt(SqlDataReader reader, string columnName)
    {
        var ordinal = reader.GetOrdinal(columnName);

        return reader.IsDBNull(ordinal)
            ? null
            : Convert.ToInt32(reader.GetValue(ordinal));
    }

    private static bool? GetNullableBool(SqlDataReader reader, string columnName)
    {
        var ordinal = reader.GetOrdinal(columnName);

        return reader.IsDBNull(ordinal)
            ? null
            : reader.GetBoolean(ordinal);
    }

    private sealed class ForeignKeyRow
    {
        public string ForeignKeyName { get; }
        public string SourceTableName { get; }
        public string ColumnName { get; }
        public string TargetTableName { get; }
        public int TargetTableIndexId { get; }
        public int SourceColumnIndex { get; }
        public int TargetColumnIndex { get; }
        public string Description { get; }

        [DebuggerStepThrough]
        public ForeignKeyRow(string foreignKeyName
            , string sourceTableName
            , string columnName
            , string targetTableName
            , int targetTableIndexId
            , int sourceColumnIndex
            , int targetColumnIndex
            , string description)
        {
            this.ForeignKeyName = foreignKeyName;
            this.SourceTableName = sourceTableName;
            this.ColumnName = columnName;
            this.TargetTableName = targetTableName;
            this.TargetTableIndexId = targetTableIndexId;
            this.SourceColumnIndex = sourceColumnIndex;
            this.TargetColumnIndex = targetColumnIndex;
            this.Description = description;
        }
    }
}
