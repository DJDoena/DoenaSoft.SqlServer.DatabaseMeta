namespace DoenaSoft.SqlServerDatabaseMeta
{
    using System.Collections.Generic;

    /// <summary>
    /// Represents a foreign key from one <see cref="ITableMeta">table</see> to a primary key or a unique key of another (or the same) table.
    /// </summary>
    public interface IForeignKeyMeta : IMetaBase
    {
        /// <summary>
        /// The table from which the foreign key relation starts.
        /// </summary>
        ITableMeta SourceTable { get; }

        /// <summary>
        /// The column(s) in the source table.
        /// </summary>
        IReadOnlyList<IColumnMeta> SourceColumns { get; }

        /// <summary>
        /// The table to which the foreign key relation points.
        /// </summary>
        ITableMeta TargetTable { get; }

        /// <summary>
        /// While configuring a foreign key, one points one (or more) <see cref="SourceColumns">source columns</see> to one (or more) target columns.
        /// However the target columns must either be represented by a primary or unique key in the target table and are represented as such.
        /// </summary>
        /// <remarks>
        /// Primary or unique keys in a database can consist or only one <see cref="IColumnMeta">column</see> or a combination of more than one.
        /// The amount and <see cref="IColumnMeta.SqlDataType">data types</see> of the source and target columns need to match for a foreign key relationship to be definable.
        /// </remarks>
        IIndexMeta TargetIndex { get; }

        /// <summary>
        /// The specific relation of each <see cref="SourceColumns">source column</see> to each target column in the <see cref="TargetIndex">target index</see>.
        /// </summary>
        IReadOnlyList<IForeignKeyColumnReference> ColumnReferences { get; }
    }
}