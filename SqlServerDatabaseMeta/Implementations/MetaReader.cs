namespace DoenaSoft.SqlServerDatabaseMeta
{
    using System;
    using System.Collections.Generic;
    using System.Data.SqlClient;
    using System.Linq;
    using DatabaseSchemaTableAdapters;

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
        public IReadOnlyList<ITableMeta> Read(string server, string database, string user, string password) => this.Read($"Data Source={server};Initial Catalog={database};User ID={user};Password={password}");

        /// <summary>
        /// Opens a SQL server database connection with the given <paramref name="connectionString">connection string</paramref> and extracts all the meta information about tables, columns, etc.
        /// </summary>
        /// <param name="connectionString">connection string</param>
        /// <returns>the meta information</returns>
        public IReadOnlyList<ITableMeta> Read(string connectionString)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                var result = this.Read(connection);

                return result;
            }
        }

        /// <summary>
        /// Uses and already opened database connection and extracts all the meta information about tables, columns, etc.
        /// </summary>
        /// <param name="openConnection">open SQL server connection</param>
        /// <returns>the meta information</returns>
        public virtual IReadOnlyList<ITableMeta> Read(SqlConnection openConnection)
        {
            _connection = openConnection;

            _meta = new Dictionary<string, TableMeta>();

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
            using (var tableAdapter = new TablesTableAdapter()
            {
                Connection = _connection,
            })
            {
                using (var dataTable = tableAdapter.GetData())
                {
                    foreach (var row in dataTable.Rows.Cast<DatabaseSchema.TablesRow>())
                    {
                        this.AddTable(row);
                    }
                }
            }
        }

        private void AddTable(DatabaseSchema.TablesRow row)
        {
            var description = row.IsDescriptionNull()
                ? null
                : row.Description;

            _meta.Add(row.TableName.ToLowerInvariant(), new TableMeta(row.TableName, description, row.Type));
        }

        private void AddColumns()
        {
            using (var tableAdapter = new ColumnsTableAdapter()
            {
                Connection = _connection,
            })
            {
                using (var dataTable = tableAdapter.GetData())
                {
                    foreach (var row in dataTable.Rows.Cast<DatabaseSchema.ColumnsRow>())
                    {
                        this.AddColumn(row);
                    }
                }
            }
        }

        private void AddColumn(DatabaseSchema.ColumnsRow row)
        {
            var table = _meta[row.TableName.ToLowerInvariant()];

            var description = row.IsDescriptionNull()
                ? null
                : row.Description;

            var columnId = row.IsColumnIdNull()
                ? (int?)null
                : row.ColumnId;

            var defaultValue = row.IsDefaultValueNull()
               ? null
               : row.DefaultValue;

            var isNullable = Convert.ToBoolean(row.IsNullable);

            var isIdentity = row.IsIsIdentityNull()
                 ? (bool?)null
                : row.IsIdentity;

            var numericPrecision = row.IsNumericPrecisionNull()
                ? (int?)null
                : row.NumericPrecision;

            var numericScale = row.IsNumericScaleNull()
                ? (int?)null
                : row.NumericScale;

            var maxTextLength = row.IsMaxTextLengthNull()
                ? (int?)null
                : row.MaxTextLength;

            var textCollation = row.IsTextCollationNull()
                ? null
                : row.TextCollation;

            table.AddColumn(new ColumnMeta(row.ColumnName, description, table, row.ColumnIndex, columnId, row.DataType, defaultValue)
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
            using (var tableAdapter = new ForeignKeysTableAdapter()
            {
                Connection = _connection,
            })
            {
                using (var dataTable = tableAdapter.GetData())
                {
                    var rowGroups = dataTable.Rows.Cast<DatabaseSchema.ForeignKeysRow>().GroupBy(r => new Tuple<string, string>(r.SourceTableName, r.ForeignKeyName));

                    foreach (var rowGroup in rowGroups)
                    {
                        this.AddForeignKey(rowGroup);
                    }
                }
            }
        }

        private void AddForeignKey(IEnumerable<DatabaseSchema.ForeignKeysRow> keyGroup)
        {
            var first = keyGroup.First();

            var sourceTable = _meta[first.SourceTableName.ToLowerInvariant()];

            var sourceColumns = keyGroup.Select(key => sourceTable.Columms.First(stc => stc.Name.Equals(key.ColumName, StringComparison.OrdinalIgnoreCase))).ToList();

            var targetTable = _meta[first.TargetTableName.ToLowerInvariant()];

            var description = first.IsDescriptionNull()
                ? null
                : first.Description;

            var columnReferenceIndexes = keyGroup.Select(key => new ForeignKeyColumnReferenceIndexes(key.SourceColumnIndex, key.TargetColumnIndex)).ToList();

            var foreignKey = new ForeignKeyMeta(first.ForeignKeyName, description, sourceColumns, targetTable, first.TargetTableIndexId, columnReferenceIndexes);

            sourceTable.AddOutgoingForeignKey(foreignKey);

            targetTable.AddIncomingForeignKey(foreignKey);
        }

        private void AddIndices()
        {
            using (var tableAdapter = new IndicesTableAdapter()
            {
                Connection = _connection,
            })
            {
                using (var dataTable = tableAdapter.GetData())
                {
                    foreach (var row in dataTable.Rows.Cast<DatabaseSchema.IndicesRow>())
                    {
                        this.AddIndex(row);
                    }
                }
            }
        }

        private void AddIndex(DatabaseSchema.IndicesRow row)
        {
            var table = _meta[row.TableName.ToLowerInvariant()];

            var columnNames = row.Columns.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(p => p.Trim());

            var columns = columnNames.Select(cn => table.Columms.First(c => c.Name.Equals(cn, StringComparison.OrdinalIgnoreCase))).ToList();

            var propertyTags = row.Properties.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(p => p.Trim()).ToList();

            var description = row.IsDescriptionNull()
                ? null
                : row.Description;

            table.AddIndex(new IndexMeta(row.IndexName, description, table, row.IndexId, columns, propertyTags));
        }

        private void AddChecks()
        {
            using (var tableAdapter = new ChecksTableAdapter()
            {
                Connection = _connection,
            })
            {
                using (var dataTable = tableAdapter.GetData())
                {
                    foreach (var row in dataTable.Rows.Cast<DatabaseSchema.ChecksRow>())
                    {
                        this.AddCheck(row);
                    }
                }
            }
        }

        private void AddCheck(DatabaseSchema.ChecksRow row)
        {
            var table = _meta[row.TableName.ToLowerInvariant()];

            var description = row.IsDescriptionNull()
                ? null
                : row.Description;

            table.AddCheck(new CheckMeta(row.CheckName, description, table, row.Definition));
        }
    }
}