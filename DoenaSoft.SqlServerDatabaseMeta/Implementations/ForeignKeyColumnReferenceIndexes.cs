namespace DoenaSoft.SqlServerDatabaseMeta
{
    internal struct ForeignKeyColumnReferenceIndexes
    {
        public int SourceColumnId { get; }

        public int TargetColumnId { get; }

        public ForeignKeyColumnReferenceIndexes(int sourceColumnId, int targetColumnId)
        {
            this.SourceColumnId = sourceColumnId;

            this.TargetColumnId = targetColumnId;
        }

        public override string ToString() => $"{SourceColumnId} -> {TargetColumnId}";
    }
}