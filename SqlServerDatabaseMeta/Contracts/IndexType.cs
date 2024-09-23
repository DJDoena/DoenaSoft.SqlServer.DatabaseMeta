using System;

namespace DoenaSoft.SqlServerDatabaseMeta;

/// <summary>
/// Gives information about the type of the index.
/// </summary>
[Flags]
public enum IndexType : byte
{
    /// <summary />
    Unknown = 0,

    /// <summary />
    Clustered = 1,

    /// <summary />
    NonClustered = 2,

    /// <summary />
    Heap = 4,

    /// <summary />
    Unique = 8,

    /// <summary />
    PrimaryKey = 16,
}