namespace DoenaSoft.SqlServerDatabaseMeta;

internal sealed class ForeignKeyColumnReference : IForeignKeyColumnReference
{
    public IColumnMeta SourceColumn { get; }

    public IColumnMeta TargetColumn { get; }

    public ForeignKeyColumnReference(IColumnMeta sourceColumn
        , IColumnMeta targetColumn)
    {
        this.SourceColumn = sourceColumn;
        this.TargetColumn = targetColumn;
    }

    public override string ToString()
        => $"Foreign key: {this.SourceColumn.Table.Name}.{this.SourceColumn.Name} -> {this.TargetColumn.Table.Name}.{this.TargetColumn.Name}";
}