using System.Collections.Generic;

namespace DoenaSoft.SqlServerDatabaseMeta
{
    /// <summary>
    /// Represents an index on a <see cref="ITableMeta">table</see> which may be a unique key or clustered (or ...) index.
    /// </summary>
    public interface IIndexMeta : IMetaBase
    {
        /// <summary>
        /// The table this index is implemented on.
        /// </summary>
        ITableMeta Table { get; }

        /// <summary>
        /// The internal database meta Id of the index.
        /// </summary>
        /// <remarks>
        /// Unlike the <see cref="IMetaBase.MetaId"/> this Id originates from the underlying database.
        /// </remarks>
        int IndexId { get; }

        /// <summary>
        /// The indexed columns.
        /// </summary>
        IReadOnlyList<IColumnMeta> Columns { get; }

        /// <summary>
        /// Gives information about the type of the index.
        /// </summary>
        IndexType Properties { get; }
    }
}