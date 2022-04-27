namespace DoenaSoft.SqlServerDatabaseMeta
{
    using System;

    internal abstract class MetaBase : IMetaBase
    {
        public Guid MetaId { get; }

        public string Name { get; }

        public string Description { get; }

        protected MetaBase(string name, string description)
        {
            this.MetaId = Guid.NewGuid();
            this.Name = name;
            this.Description = description;
        }

        public override string ToString()
        {
            if (!string.IsNullOrWhiteSpace(this.Description))
            {
                return $"{this.Name} ({this.Description})";
            }
            else
            {
                return this.Name;
            }
        }

        public override int GetHashCode() => this.MetaId.GetHashCode();

        public override abstract bool Equals(object obj);
    }
}