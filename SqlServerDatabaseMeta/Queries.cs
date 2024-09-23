using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace DoenaSoft.SqlServerDatabaseMeta;

/// <summary>
/// Contains the SQL queries executed for this library
/// </summary>
public static class Queries
{
    /// <summary>
    /// Tables and Views
    /// </summary>
    public const string Tables= @"SELECT TableName,
       Type,
       Description
FROM
(
    SELECT t.name AS TableName,
           'BASE TABLE' AS Type,
           CAST(ep.value AS varchar) AS Description
    FROM sys.tables AS t
        LEFT OUTER JOIN sys.extended_properties AS ep
            ON t.object_id = ep.major_id
               AND ep.minor_id = 0
               AND ep.name = 'MS_Description'
    UNION
    SELECT v.name AS TableName,
           'VIEW' AS Type,
           CAST(ep.value AS varchar) AS Description
    FROM sys.views AS v
        LEFT OUTER JOIN sys.extended_properties AS ep
            ON v.object_id = ep.major_id
               AND ep.minor_id = 0
               AND ep.name = 'MS_Description'
) AS Tables";

    /// <summary>
    /// Columns
    /// </summary>
    public const string Columns= "select table_name as TableName,\r\n       Column_name as ColumnName,\r\n       ordina" +
                "l_position as ColumnIndex,\r\n       column_default as DefaultValue,\r\n       CASE\r" +
                "\n           WHEN SchemaCol.IS_NULLABLE = \'YES\' THEN\r\n               1\r\n         " +
                "  WHEN SchemaCol.IS_NULLABLE = \'NO\' THEN\r\n               0\r\n           ELSE\r\n   " +
                "            NULL\r\n       END AS IsNullable,\r\n       Data_type as DataType,\r\n    " +
                "   numeric_precision as NumericPrecision,\r\n       numeric_scale as NumericScale," +
                "\r\n       Character_Maximum_length as MaxTextLength,\r\n       collation_name as Te" +
                "xtCollation,\r\n       itab.IsIdentity as IsIdentity,\r\n       itab.Description as " +
                "Description,\r\n       itab.ColumnId\r\nfrom INFORMATION_SCHEMA.COLUMNS SchemaCol\r\n " +
                "   left outer join\r\n    (\r\n        select tab.name as TableName,\r\n              " +
                " col.name as ColumnName,\r\n               col.is_identity as IsIdentity,\r\n       " +
                "        col.is_nullable as IsNullable,\r\n               cast(ep.value as varchar)" +
                " as Description,\r\n               col.column_id as ColumnId\r\n        from sys.col" +
                "umns col\r\n            inner join sys.tables tab\r\n                on col.object_i" +
                "d = tab.object_id\r\n            left outer join sys.extended_properties ep\r\n     " +
                "           on col.object_id = ep.major_id\r\n                   and col.column_id " +
                "= ep.minor_id\r\n                   and ep.name = \'MS_Description\'\r\n        where " +
                "tab.type = \'U\'\r\n    ) itab\r\n        on SchemaCol.TABLE_NAME = itab.TableName\r\n  " +
                "         and SchemaCol.COLUMN_NAME = itab.ColumnName\r\norder by TableName,\r\n     " +
                "    ColumnIndex";

    /// <summary>
    /// Foreign Keys
    /// </summary>
    public const string ForeignKeys= @"SELECT f.name as ForeignKeyName,
       OBJECT_NAME(f.parent_object_id) SourceTableName,
       COL_NAME(fc.parent_object_id, fc.parent_column_id) as ColumName,
       OBJECT_NAME(f.referenced_object_id) as TargetTableName,
       f.key_index_id as TargetTableIndexId,
       fc.parent_column_id as SourceColumnIndex,
       fc.referenced_column_id as TargetColumnIndex,
       cast(ep.value as varchar) as Description
FROM sys.foreign_keys AS f
    INNER JOIN sys.foreign_key_columns AS fc
        ON f.OBJECT_ID = fc.constraint_object_id
    left outer join sys.extended_properties ep
        on f.object_id = ep.major_id
           and ep.name = 'MS_Description'
    INNER JOIN sys.tables t
        ON t.OBJECT_ID = fc.referenced_object_id
order by SourceTableName,
         TargetTableName,
         SourceColumnIndex";

    /// <summary>
    /// Indices (Indexes)
    /// </summary>
    public const string Indices = "SELECT o.NAME AS \'TableName\',\r\n       i.NAME AS \'IndexName\',\r\n       LOWER(i.type" +
                "_desc) + CASE\r\n                                WHEN i.is_unique = 1 THEN\r\n      " +
                "                              \', unique\'\r\n                                ELSE\r\n" +
                "                                    \'\'\r\n                            END + CASE\r\n" +
                "                                      WHEN i.is_primary_key = 1 THEN\r\n          " +
                "                                \', primary key\'\r\n                               " +
                "       ELSE\r\n                                          \'\'\r\n                     " +
                "             END AS \'Properties\',\r\n       STUFF(\r\n       (\r\n           SELECT \'," +
                " \' + sc.NAME AS \"text()\"\r\n           FROM syscolumns AS sc\r\n               INNER" +
                " JOIN sys.index_columns AS ic\r\n                   ON ic.object_id = sc.id\r\n     " +
                "                 AND ic.column_id = sc.colid\r\n           WHERE sc.id = so.object" +
                "_id\r\n                 AND ic.index_id = i1.indid\r\n                 AND ic.is_inc" +
                "luded_column = 0\r\n           ORDER BY key_ordinal\r\n           FOR XML PATH(\'\')\r\n" +
                "       ),\r\n       1,\r\n       2,\r\n       \'\'\r\n            ) AS \'Columns\',\r\n       " +
                "i.index_id as IndexId,\r\n       cast(ep.value as varchar) as Description\r\nFROM sy" +
                "sindexes AS i1\r\n    INNER JOIN sys.indexes AS i\r\n        ON i.object_id = i1.id\r" +
                "\n           AND i.index_id = i1.indid\r\n    INNER JOIN sysobjects AS o\r\n        O" +
                "N o.id = i1.id\r\n    INNER JOIN sys.objects AS so\r\n        ON so.object_id = o.id" +
                "\r\n           AND is_ms_shipped = 0\r\n    INNER JOIN sys.schemas AS s\r\n        ON " +
                "s.schema_id = so.schema_id\r\n    left outer join sys.objects so1\r\n        on i.ob" +
                "ject_id = so1.parent_object_id\r\n           and i.name = so1.name\r\n    left outer" +
                " join sys.extended_properties ep\r\n        on so1.object_id = ep.major_id\r\n      " +
                "     and ep.name = \'MS_Description\'\r\nWHERE so.type = \'U\'\r\n      AND i1.indid < 2" +
                "55\r\n      AND i1.STATUS & 64 = 0 --index with duplicates\r\n      AND i1.STATUS & " +
                "8388608 = 0 --auto created index\r\n      AND i1.STATUS & 16777216 = 0 --stats no " +
                "recompute\r\n      AND i.type_desc <> \'heap\'\r\n      AND so.NAME <> \'sysdiagrams\'\r\n" +
                "Order by TableName,\r\n         IndexId";

    /// <summary>
    /// Checks
    /// </summary>
    public const string Checks= @"select cc.name as CheckName,
       t.name as TableName,
       cc.Definition,
       cast(ep.value as varchar) as Description
from sys.check_constraints cc
    inner join sys.tables t
        on cc.parent_object_id = t.object_id
    left outer join sys.extended_properties ep
        on cc.object_id = ep.major_id
           and ep.name = 'MS_Description'
where t.type = 'U'
order by TableName,
         CheckName";

}
