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
    public const string Columns = @"SELECT SchemaCol.table_name AS TableName,
       SchemaCol.Column_name AS ColumnName,
       SchemaCol.ordinal_position AS ColumnIndex,
       SchemaCol.column_default AS DefaultValue,
       CASE
           WHEN SchemaCol.IS_NULLABLE = 'YES' THEN
               1
           WHEN SchemaCol.IS_NULLABLE = 'NO' THEN
               0
           ELSE
               NULL
       END AS IsNullable,
       SchemaCol.Data_type AS DataType,
       SchemaCol.numeric_precision AS NumericPrecision,
       SchemaCol.numeric_scale AS NumericScale,
       SchemaCol.Character_Maximum_length AS MaxTextLength,
       SchemaCol.collation_name AS TextCollation,
       itab.IsIdentity AS IsIdentity,
       itab.Description AS Description,
       itab.ColumnId
FROM INFORMATION_SCHEMA.COLUMNS SchemaCol
    LEFT OUTER JOIN
    (
        SELECT tab.name AS TableName,
               col.name AS ColumnName,
               col.is_identity AS IsIdentity,
               col.is_nullable AS IsNullable,
               CAST(ep.value AS varchar) AS Description,
               col.column_id AS ColumnId
        FROM sys.columns col
            INNER JOIN sys.tables tab
                ON col.object_id = tab.object_id
            LEFT OUTER JOIN sys.extended_properties ep
                ON col.object_id = ep.major_id
                   AND col.column_id = ep.minor_id
                   AND ep.name = 'MS_Description'
        WHERE tab.type = 'U'
    ) itab
        ON SchemaCol.TABLE_NAME = itab.TableName
           AND SchemaCol.COLUMN_NAME = itab.ColumnName
ORDER BY TableName,
         ColumnIndex";

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
    public const string Indices = @"SELECT o.NAME AS 'TableName',
       i.NAME AS 'IndexName',
       LOWER(i.type_desc) + CASE
                                WHEN i.is_unique = 1 THEN
                                    ', unique'
                                ELSE
                                    ''
                            END + CASE
                                      WHEN i.is_primary_key = 1 THEN
                                          ', primary key'
                                      ELSE
                                          ''
                                  END AS 'Properties',
       STUFF(
       (
           SELECT ', ' + sc.NAME AS ""text()""
           FROM syscolumns AS sc
               INNER JOIN sys.index_columns AS ic
                   ON ic.object_id = sc.id
                      AND ic.column_id = sc.colid
           WHERE sc.id = so.object_id
                 AND ic.index_id = i1.indid
                 AND ic.is_included_column = 0
           ORDER BY key_ordinal
           FOR XML PATH('')
       ),
       1,
       2,
       ''
            ) AS 'Columns',
       STUFF(
       (
           SELECT ', ' + sc.NAME AS ""text()""
           FROM syscolumns AS sc
               INNER JOIN sys.index_columns AS ic
                   ON ic.object_id = sc.id
                      AND ic.column_id = sc.colid
           WHERE sc.id = so.object_id
                 AND ic.index_id = i1.indid
                 AND ic.is_included_column = 1
           ORDER BY index_column_id
           FOR XML PATH('')
       ),
       1,
       2,
       ''
            ) AS 'IncludedColumns',
       i.index_id as IndexId,
       cast(ep.value as varchar) as Description
FROM sysindexes AS i1
    INNER JOIN sys.indexes AS i
        ON i.object_id = i1.id
           AND i.index_id = i1.indid
    INNER JOIN sysobjects AS o
        ON o.id = i1.id
    INNER JOIN sys.objects AS so
        ON so.object_id = o.id
           AND is_ms_shipped = 0
    INNER JOIN sys.schemas AS s
        ON s.schema_id = so.schema_id
    left outer join sys.objects so1
        on i.object_id = so1.parent_object_id
           and i.name = so1.name
    left outer join sys.extended_properties ep
        on so1.object_id = ep.major_id
           and ep.name = 'MS_Description'
WHERE so.type = 'U'
      AND i1.indid < 255
      AND i1.STATUS & 64 = 0 --index with duplicates
      AND i1.STATUS & 8388608 = 0 --auto created index
      AND i1.STATUS & 16777216 = 0 --stats no recompute
      AND i.type_desc <> 'heap'
      AND so.NAME <> 'sysdiagrams'
Order by TableName,
         IndexId";

    /// <summary>
    /// Scalar-valued user-defined functions
    /// </summary>
    public const string ScalarFunctions = @"SELECT
    s.name AS SchemaName,
    o.name AS FunctionName,
    m.definition AS Definition,
    CAST(ep.value AS varchar) AS Description
FROM sys.objects o
    INNER JOIN sys.schemas s
        ON s.schema_id = o.schema_id
    INNER JOIN sys.sql_modules m
        ON m.object_id = o.object_id
    LEFT OUTER JOIN sys.extended_properties ep
        ON ep.major_id = o.object_id
           AND ep.minor_id = 0
           AND ep.name = 'MS_Description'
WHERE o.type = 'FN'
ORDER BY SchemaName,
         FunctionName";

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
