using System.Collections.Generic;

namespace DoenaSoft.SqlServerDatabaseMeta
{
    /// <summary>
    /// Represents a user table or a view in the database.
    /// </summary>
    public interface ITableMeta : IMetaBase
    {
        /// <summary>
        /// Table or view.
        /// </summary>
        TableType Type { get; }

        /// <summary>
        /// Columns of this table / view.
        /// </summary>
        IReadOnlyList<IColumnMeta> Columms { get; }

        /// <summary>
        /// The index that represents the primary key of this table.
        /// </summary>
        /// <remarks>
        /// Can be null in a table.
        /// Will be null in a view.
        /// </remarks>
        IIndexMeta PrimaryKey { get; }
        
        /// <summary>
        /// Foreign keys that point from this table to another.
        /// </summary>
        IReadOnlyList<IForeignKeyMeta> OutgoingForeignKeys { get; }

        /// <summary>
        /// Foreign keys that point from another table to this one.
        /// </summary>
        IReadOnlyList<IForeignKeyMeta> IncomingForeignKeys { get; }

        /// <summary>
        /// Indicies for this table.
        /// </summary>
        IReadOnlyList<IIndexMeta> Indices { get; }

        /// <summary>
        /// Check constraints for this table.
        /// </summary>
        IReadOnlyList<ICheckMeta> Checks { get; }
    }
}