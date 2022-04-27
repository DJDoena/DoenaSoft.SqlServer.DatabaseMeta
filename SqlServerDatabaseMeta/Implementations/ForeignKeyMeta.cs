namespace DoenaSoft.SqlServerDatabaseMeta
{
    using System.Collections.Generic;
    using System.Linq;

    internal sealed class ForeignKeyMeta : MetaBase, IForeignKeyMeta
    {
        private readonly int _targetTableIndexId;

        private readonly List<ForeignKeyColumnReferenceIndexes> _columnReferenceIndexes;

        public ITableMeta SourceTable => this.SourceColumns.First().Table;

        public IReadOnlyList<IColumnMeta> SourceColumns { get; }

        public ITableMeta TargetTable { get; }

        public IIndexMeta TargetIndex => this.TargetTable.Indices.First(i => i.IndexId == _targetTableIndexId);

        public IReadOnlyList<IForeignKeyColumnReference> ColumnReferences => this.GetColumnReferences().ToList().AsReadOnly();

        internal ForeignKeyMeta(string name, string description, List<IColumnMeta> sourceColumns, ITableMeta targetTable, int targetTableIndexId, List<ForeignKeyColumnReferenceIndexes> columnReferences) : base(name, description)
        {
            this.SourceColumns = sourceColumns.AsReadOnly();
            this.TargetTable = targetTable;
            _targetTableIndexId = targetTableIndexId;
            _columnReferenceIndexes = columnReferences;
        }

        public override string ToString()
        {
            var references = this.ColumnReferences;

            var sourceColumnText = references.Count > 1
                ? $"[{string.Join(", ", references.Select(r => r.SourceColumn.Name))}]"
                : references[0].SourceColumn.Name;

            var targetColumText = references.Count > 1
                ? $"[{string.Join(", ", references.Select(r => r.TargetColumn.Name))}]"
                : references[0].TargetColumn.Name;

            var result = $"Foreign key: {this.SourceTable.Name}.{sourceColumnText} -> {this.TargetTable.Name}.{targetColumText} ({base.ToString()})";

            return result;
        }

        public override int GetHashCode() => base.GetHashCode();

        public override bool Equals(object obj)
        {
            if (!(obj is IForeignKeyMeta other))
            {
                return false;
            }

            return this.MetaId.Equals(other.MetaId);
        }

        private IEnumerable<IForeignKeyColumnReference> GetColumnReferences()
        {
            var targetColumns = this.TargetIndex.Columns;

            foreach (var sourceColumn in this.SourceColumns)
            {
                var reference = _columnReferenceIndexes.First(r => r.SourceColumnId == sourceColumn.ColumnId);

                var targetColumn = targetColumns.First(tc => tc.ColumnId == reference.TargetColumnId);

                yield return new ForeignKeyColumnReference(sourceColumn, targetColumn);
            }
        }
    }
}