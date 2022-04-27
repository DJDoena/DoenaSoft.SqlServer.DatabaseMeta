namespace DoenaSoft.SqlServerDatabaseMeta
{
    using System.Collections.Generic;
    using System.Linq;

    internal sealed class TableMeta : MetaBase, ITableMeta
    {
        private readonly List<IColumnMeta> _columms;

        private readonly List<IForeignKeyMeta> _outgoingForeignKeys;

        private readonly List<IForeignKeyMeta> _incomingForeignKeys;

        private readonly List<IIndexMeta> _indices;

        private readonly List<ICheckMeta> _checks;

        public TableType Type { get; }

        public IReadOnlyList<IColumnMeta> Columms => _columms.AsReadOnly();

        public IIndexMeta PrimaryKey => this.Indices.FirstOrDefault(i => i.Properties.HasFlag(IndexType.PrimaryKey));

        public IReadOnlyList<IForeignKeyMeta> OutgoingForeignKeys => _outgoingForeignKeys.AsReadOnly();

        public IReadOnlyList<IForeignKeyMeta> IncomingForeignKeys => _incomingForeignKeys.AsReadOnly();

        public IReadOnlyList<IIndexMeta> Indices => _indices.AsReadOnly();

        public IReadOnlyList<ICheckMeta> Checks => _checks.AsReadOnly();

        internal TableMeta(string name, string description, string type) : base(name, description)
        {
            if (type == "BASE TABLE")
            {
                this.Type = TableType.Table;
            }
            else if (type == "VIEW")
            {
                this.Type = TableType.View;
            }
            else
            {
                this.Type = TableType.Unknown;
            }

            _columms = new List<IColumnMeta>();
            _outgoingForeignKeys = new List<IForeignKeyMeta>();
            _incomingForeignKeys = new List<IForeignKeyMeta>();
            _indices = new List<IIndexMeta>();
            _checks = new List<ICheckMeta>();
        }

        public override string ToString() => $"{this.Type}: {base.ToString()}";

        public override int GetHashCode() => base.GetHashCode();

        public override bool Equals(object obj)
        {
            if (!(obj is ITableMeta other))
            {
                return false;
            }

            return this.MetaId.Equals(other.MetaId);
        }

        internal void AddColumn(IColumnMeta column) => _columms.Add(column);

        internal void AddOutgoingForeignKey(IForeignKeyMeta foreignKey) => _outgoingForeignKeys.Add(foreignKey);

        internal void AddIncomingForeignKey(IForeignKeyMeta foreignKey) => _incomingForeignKeys.Add(foreignKey);

        internal void AddIndex(IIndexMeta index) => _indices.Add(index);

        internal void AddCheck(ICheckMeta check) => _checks.Add(check);
    }
}