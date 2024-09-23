using System.Collections.Generic;
using System.Linq;

namespace DoenaSoft.SqlServerDatabaseMeta;

internal sealed class IndexMeta : MetaBase, IIndexMeta
{
    public ITableMeta Table { get; }

    public int IndexId { get; }

    public IReadOnlyList<IColumnMeta> Columns { get; }

    public IndexType Properties { get; }

    internal IndexMeta(string name
        , string description
        , ITableMeta table
        , int indexId
        , List<IColumnMeta> columns
        , List<string> propertyTags)
        : base(name, description)
    {
        this.Table = table;
        this.IndexId = indexId;
        this.Columns = columns.AsReadOnly();
        this.Properties = GetPropertyFlags(propertyTags);
    }

    public override string ToString()
        => $"Index: {this.Table.Name}.{base.ToString()} ({string.Join(", ", this.Columns.Select(c => c.Name))})";

    public override int GetHashCode()
        => base.GetHashCode();

    public override bool Equals(object obj)
    {
        if (obj is not IIndexMeta other)
        {
            return false;
        }

        return this.MetaId.Equals(other.MetaId);
    }

    private static IndexType GetPropertyFlags(List<string> propertyTags)
    {
        var result = IndexType.Unknown;

        if (propertyTags.Contains("clustered"))
        {
            result |= IndexType.Clustered;
        }

        if (propertyTags.Contains("nonclustered"))
        {
            result |= IndexType.NonClustered;
        }

        if (propertyTags.Contains("heap"))
        {
            result |= IndexType.Heap;
        }

        if (propertyTags.Contains("unique"))
        {
            result |= IndexType.Unique;
        }

        if (propertyTags.Contains("primary key"))
        {
            result |= IndexType.PrimaryKey;
        }

        return result;
    }
}