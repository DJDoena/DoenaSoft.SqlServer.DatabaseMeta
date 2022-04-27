namespace DoenaSoft.SqlServerDatabaseMeta
{
    /// <summary>
    /// The data type as defined in the underlying database.
    /// </summary>
    public enum SqlDataType : byte
    {
        /// <summary />
        Unknown,

        /// <summary />
        bigint,

        /// <summary />
        @int,

        /// <summary />
        smallint,

        /// <summary />
        tinyint,

        /// <summary />
        @decimal,

        /// <summary />
        money,

        /// <summary />
        numeric,

        /// <summary />
        smallmoney,

        /// <summary />
        @float,

        /// <summary />
        real,

        /// <summary />
        binary,

        /// <summary />
        image,

        /// <summary>
        /// rowversion
        /// </summary>
        timestamp,

        /// <summary />
        varbinary,

        /// <summary />
        bit,

        /// <summary />
        @char,

        /// <summary />
        nchar,

        /// <summary />
        ntext,

        /// <summary />
        nvarchar,

        /// <summary />
        text,

        /// <summary />
        varchar,

        /// <summary />
        xml,

        /// <summary />
        date,

        /// <summary />
        datetime,

        /// <summary />
        datetime2,

        /// <summary />
        smalldatetime,

        /// <summary />
        datetimeoffset,

        /// <summary />
        time,

        /// <summary />
        uniqueidentifier,

        /// <summary />
        geography,

        /// <summary />
        geometry,

        /// <summary />
        sql_variant,

        /// <summary />
        hierarchyid,
    }
}