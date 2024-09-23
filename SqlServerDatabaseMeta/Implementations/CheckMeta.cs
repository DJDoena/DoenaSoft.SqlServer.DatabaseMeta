namespace DoenaSoft.SqlServerDatabaseMeta;

internal sealed class CheckMeta : MetaBase, ICheckMeta
{
    public ITableMeta Table { get; }

    public string Check { get; }

    public CheckMeta(string name
        , string description
        , ITableMeta table
        , string check)
        : base(name, description)
    {
        this.Table = table;
        this.Check = check;
    }

    public override string ToString()
        => $"Check: {this.Table.Name}.{base.ToString()}";

    public override int GetHashCode()
        => base.GetHashCode();

    public override bool Equals(object obj)
    {
        if (obj is not ICheckMeta other)
        {
            return false;
        }

        return this.MetaId.Equals(other.MetaId);
    }
}