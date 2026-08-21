namespace DoenaSoft.SqlServerDatabaseMeta;

internal sealed class ScalarFunctionMeta : MetaBase, IScalarFunctionMeta
{
    public string Schema { get; }

    public string Definition { get; }

    public ScalarFunctionMeta(string name
        , string description
        , string schema
        , string definition)
        : base(name, description)
    {
        this.Schema = schema;
        this.Definition = definition;
    }

    public override string ToString()
        => $"Function: {this.Schema}.{base.ToString()}";

    public override int GetHashCode()
        => base.GetHashCode();

    public override bool Equals(object obj)
    {
        if (obj is not IScalarFunctionMeta other)
        {
            return false;
        }

        return this.MetaId.Equals(other.MetaId);
    }
}
