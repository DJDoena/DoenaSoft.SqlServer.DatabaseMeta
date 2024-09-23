namespace DoenaSoft.SqlServerDatabaseMeta;

/// <summary>
/// Defines the type that <see cref="ITableMeta"/> represents. There is no major difference between a user table and a view, except in the <see cref="IColumnMeta">column properties</see>.
/// </summary>
public enum TableType : byte
{
    /// <summary />
    Unknown,

    /// <summary />
    Table,

    /// <summary />
    View,
}