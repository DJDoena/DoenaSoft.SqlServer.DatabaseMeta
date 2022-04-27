namespace DoenaSoft.SqlServerDatabaseMeta
{
    /// <summary>
    /// Represents a from-to relation from a source <see cref="IColumnMeta">column</see> to a target column in a <see cref="IForeignKeyMeta">foreign key</see>.
    /// </summary>
    public interface IForeignKeyColumnReference
    {
        /// <summary>
        /// The source column.
        /// </summary>
        IColumnMeta SourceColumn { get; }

        /// <summary>
        /// The target column.
        /// </summary>
        IColumnMeta TargetColumn { get; }
    }
}