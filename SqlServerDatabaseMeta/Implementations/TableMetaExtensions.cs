using System.Collections.Generic;
using System.Linq;

namespace DoenaSoft.SqlServerDatabaseMeta;

/// <summary>
/// Extension methods that operate on the meta objects of this library.
/// </summary>
public static class TableMetaExtensions
{
    /// <summary>
    /// Returns a list of all the meta objects that were collected by an implementation of <see cref="IMetaReader"/>.
    /// </summary>
    /// <param name="tableMetas">list of tables / views</param>
    /// <returns>A list of all the meta objects related to the input.</returns>
    public static HashSet<IMetaBase> GetAllMetaObjects(this IEnumerable<ITableMeta> tableMetas)
    {
        var result = new HashSet<IMetaBase>();

        if (tableMetas != null)
        {
            var adder = new MetaAdder(result);

            foreach (var tableMeta in tableMetas)
            {
                adder.AddTable(tableMeta);
            }
        }

        return result;
    }

    /// <summary>
    /// Returns a list of all the meta objects that were collected by an implementation of <see cref="IMetaReader"/>.
    /// </summary>
    /// <param name="tableMeta">table / view</param>
    /// <returns>A list of all the meta objects related to the input.</returns>
    public static HashSet<IMetaBase> GetAllMetaObjects(this ITableMeta tableMeta)
    {
        var result = new HashSet<IMetaBase>();

        if (tableMeta != null)
        {
            (new MetaAdder(result)).AddTable(tableMeta);
        }

        return result;
    }

    private sealed class MetaAdder
    {
        private readonly HashSet<IMetaBase> _meta;

        public MetaAdder(HashSet<IMetaBase> meta)
        {
            _meta = meta;
        }

        public void AddTable(ITableMeta table)
        {
            if (table != null && !_meta.Contains(table))
            {
                _meta.Add(table);

                this.AddColumns(table.Columms);

                this.AddChecks(table.Checks);

                this.AddIndices(table.Indices);

                this.AddForeignKeys(table.OutgoingForeignKeys);

                this.AddForeignKeys(table.IncomingForeignKeys);

                this.AddIndex(table.PrimaryKey);
            }
        }

        private void AddChecks(IEnumerable<ICheckMeta> checks)
        {
            if (checks?.Any() == true)
            {
                foreach (var check in checks)
                {
                    this.AddCheck(check);
                }
            }
        }

        private void AddCheck(ICheckMeta check)
        {
            if (check != null && !_meta.Contains(check))
            {
                _meta.Add(check);

                this.AddTable(check.Table);
            }
        }

        private void AddColumns(IEnumerable<IColumnMeta> columns)
        {
            if (columns?.Any() == true)
            {
                foreach (var column in columns)
                {
                    this.AddColumn(column);
                }
            }
        }

        private void AddColumn(IColumnMeta column)
        {
            if (column != null && !_meta.Contains(column))
            {
                _meta.Add(column);

                this.AddTable(column.Table);
            }
        }

        private void AddIndices(IEnumerable<IIndexMeta> indices)
        {
            if (indices?.Any() == true)
            {
                foreach (var index in indices)
                {
                    this.AddIndex(index);
                }
            }
        }

        private void AddIndex(IIndexMeta index)
        {
            if (index != null && !_meta.Contains(index))
            {
                _meta.Add(index);

                this.AddColumns(index.Columns);

                this.AddTable(index.Table);
            }
        }

        private void AddForeignKeys(IReadOnlyList<IForeignKeyMeta> foreignKeys)
        {
            if (foreignKeys?.Any() == true)
            {
                foreach (var foreignKey in foreignKeys)
                {
                    this.AddForeignKey(foreignKey);
                }
            }
        }

        private void AddForeignKey(IForeignKeyMeta foreignKey)
        {
            if (foreignKey != null && !_meta.Contains(foreignKey))
            {
                _meta.Add(foreignKey);

                this.AddTable(foreignKey.SourceTable);

                this.AddTable(foreignKey.TargetTable);

                this.AddColumns(foreignKey.SourceColumns);

                this.AddIndex(foreignKey.TargetIndex);

                if (foreignKey.ColumnReferences?.Any() == true)
                {
                    foreach (var columnReference in foreignKey.ColumnReferences)
                    {
                        this.AddColumn(columnReference.SourceColumn);
                        this.AddColumn(columnReference.TargetColumn);
                    }
                }
            }
        }
    }
}