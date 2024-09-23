using System;

namespace DoenaSoft.SqlServerDatabaseMeta;

/// <summary>
/// Represents a column in a user table / user view.
/// </summary>
public interface IColumnMeta : IMetaBase
{
    /// <summary>
    /// The table this column belongs to.
    /// </summary>
    ITableMeta Table { get; }

    /// <summary>
    /// Table or view.
    /// </summary>
    TableType TableType { get; }

    /// <summary>
    /// The ordinal position within the table.
    /// </summary>
    int Index { get; }

    /// <summary>
    /// The internal database meta Id of the column.
    /// </summary>
    /// <remarks>
    /// Is only set when the <see cref="TableType"/> is <see cref="TableType.Table"/>.
    /// Can be but isn't necessarily identical with the <see cref="Index"/>.
    /// Unlike the <see cref="IMetaBase.MetaId"/> this Id originates from the underlying database.
    /// </remarks>
    int? ColumnId { get; }

    /// <summary>
    /// The data type as defined in the underlying database.
    /// </summary>
    SqlDataType SqlDataType { get; }

    /// <summary>
    /// The .Net datatype that a tool like EntityFramework would generate for this column's value.
    /// </summary>
    Type NetDataType { get; }

    /// <summary>
    /// Default value of the field when the column's value is not explicitly specified during INSERT.
    /// </summary>
    string DefaultValue { get; }

    /// <summary>
    /// Whether or not NULL is a valid field value.
    /// </summary>
    bool IsNullable { get; }

    /// <summary>
    /// If the field auto-increments.
    /// </summary>
    bool? IsIdentity { get; }

    /// <summary>
    /// Size of the whole part of a floating-point number.
    /// </summary>
    /// <remarks>
    /// The field is only used when the column is of any numeric <see cref="SqlDataType">data type</see> (e.g. int, float, etc.).
    /// </remarks>
    int? NumericPrecision { get; }

    /// <summary>
    /// Size of the fraction part of a floating-point number.
    /// </summary>
    /// <remarks>
    /// The field is only used when the column is of any numeric <see cref="SqlDataType">data type</see> (e.g. int, float, etc.).
    /// </remarks>
    int? NumericScale { get; }

    /// <summary>
    /// Maximum number of characters that can be entered into a text field.
    /// </summary>
    /// <remarks>
    /// When a database type like nvarchar (or any other n*) is used, the needed storage space can exceed that of the defined characters which is due to the way UTF-8 stores its data.
    /// The field is only used when the column is of any text <see cref="SqlDataType">data type</see> (e.g. nchar, varchar, etc.).
    /// </remarks>
    int? MaxTextLength { get; }

    /// <summary>
    /// The sorting / search collation for this text field
    /// </summary>
    /// <remarks>
    /// <a href="https://www.red-gate.com/simple-talk/databases/sql-server/t-sql-programming-sql-server/questions-sql-server-collations-shy-ask/#post-70501-_Toc479001442">What is a SQL Server collation?</a>
    /// The field is only used when the column is of any text <see cref="SqlDataType">data type</see> (e.g. nchar, varchar, etc.).
    /// </remarks>
    string TextCollation { get; }
}