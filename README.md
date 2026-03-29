# SQL Server Database Meta Reader

[![NuGet](https://img.shields.io/nuget/v/DoenaSoft.SqlServerDatabaseMeta.svg)](https://www.nuget.org/packages/DoenaSoft.SqlServerDatabaseMeta)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

A .NET library that provides read-only access to SQL Server database metadata, enabling you to programmatically discover and analyze database structure.

## Features

- **Tables and Views**: Read table and view definitions with descriptions
- **Columns**: Access column metadata including data types, nullability, defaults, and identity columns
- **Primary Keys**: Discover primary key constraints
- **Foreign Keys**: Read foreign key relationships and establish links between tables
- **Indexes**: Query index definitions including unique and primary key indexes
- **Check Constraints**: Access check constraint definitions

## Supported Frameworks

- .NET Framework 4.7.2
- .NET 10.0

## Installation

Install via NuGet Package Manager:

```bash
dotnet add package DoenaSoft.SqlServerDatabaseMeta
```

Or via Package Manager Console:

```powershell
Install-Package DoenaSoft.SqlServerDatabaseMeta
```

## Quick Start

```csharp
using DoenaSoft.SqlServerDatabaseMeta;
using System.Data.SqlClient;

// Using a connection string
string connectionString = "Server=myServer;Database=myDatabase;Trusted_Connection=True;";
var metaReader = new MetaReader();
var tables = metaReader.Read(connectionString);

// Or using an open connection
using (var connection = new SqlConnection(connectionString))
{
    connection.Open();
    var tables = metaReader.Read(connection);

    foreach (var table in tables)
    {
        Console.WriteLine($"Table: {table.Name}");

        foreach (var column in table.Columns)
        {
            Console.WriteLine($"  Column: {column.Name} ({column.DataType})");
        }
    }
}
```

## Usage Examples

### Reading Table Metadata

```csharp
var metaReader = new MetaReader();
var tables = metaReader.Read(connectionString);

foreach (var table in tables)
{
    Console.WriteLine($"Table: {table.Name}");
    Console.WriteLine($"Type: {table.Type}"); // BASE TABLE or VIEW
    Console.WriteLine($"Description: {table.Description}");
}
```

### Accessing Column Information

```csharp
foreach (var table in tables)
{
    foreach (var column in table.Columns)
    {
        Console.WriteLine($"Column: {column.Name}");
        Console.WriteLine($"  Data Type: {column.DataType}");
        Console.WriteLine($"  Nullable: {column.IsNullable}");
        Console.WriteLine($"  Identity: {column.IsIdentity}");
        Console.WriteLine($"  Max Length: {column.MaxTextLength}");
    }
}
```

### Discovering Foreign Key Relationships

```csharp
foreach (var table in tables)
{
    foreach (var foreignKey in table.ForeignKeys)
    {
        Console.WriteLine($"Foreign Key: {foreignKey.Name}");
        Console.WriteLine($"  References: {foreignKey.TargetTable.Name}");
        Console.WriteLine($"  Source Column: {foreignKey.SourceColumn.Name}");
        Console.WriteLine($"  Target Column: {foreignKey.TargetColumn.Name}");
    }
}
```

### Working with Indexes

```csharp
foreach (var table in tables)
{
    foreach (var index in table.Indexes)
    {
        Console.WriteLine($"Index: {index.Name}");
        Console.WriteLine($"  Type: {index.Type}");
        Console.WriteLine($"  Is Unique: {index.IsUnique}");
        Console.WriteLine($"  Is Primary Key: {index.IsPrimaryKey}");
        Console.WriteLine($"  Columns: {string.Join(", ", index.Columns)}");
    }
}
```

## SQL Queries Used

This library uses several SQL queries to retrieve metadata from SQL Server system tables and views.

### Tables and Views Query

```sql
SELECT TableName,
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
) AS Tables

```

### Columns Query

```sql
select table_name as TableName,
       Column_name as ColumnName,
       ordinal_position as ColumnIndex,
       column_default as DefaultValue,
       CASE
           WHEN SchemaCol.IS_NULLABLE = 'YES' THEN
               1
           WHEN SchemaCol.IS_NULLABLE = 'NO' THEN
               0
           ELSE
               NULL
       END AS IsNullable,
       Data_type as DataType,
       numeric_precision as NumericPrecision,
       numeric_scale as NumericScale,
       Character_Maximum_length as MaxTextLength,
       collation_name as TextCollation,
       itab.IsIdentity as IsIdentity,
       itab.Description as Description,
       itab.ColumnId
from INFORMATION_SCHEMA.COLUMNS SchemaCol
    left outer join
    (
        select tab.name as TableName,
               col.name as ColumnName,
               col.is_identity as IsIdentity,
               col.is_nullable as IsNullable,
               cast(ep.value as varchar) as Description,
               col.column_id as ColumnId
        from sys.columns col
            inner join sys.tables tab
                on col.object_id = tab.object_id
            left outer join sys.extended_properties ep
                on col.object_id = ep.major_id
                   and col.column_id = ep.minor_id
                   and ep.name = 'MS_Description'
        where tab.type = 'U'
    ) itab
        on SchemaCol.TABLE_NAME = itab.TableName
           and SchemaCol.COLUMN_NAME = itab.ColumnName
order by TableName,
         ColumnIndex
```

### Foreign Keys Query

```sql
SELECT f.name as ForeignKeyName,
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
         SourceColumnIndex
```

### Indices (Indexes) Query

```sql
SELECT o.NAME AS 'TableName',
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
           SELECT ', ' + sc.NAME AS "text()"
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
         IndexId
```

### Checks Query

```sql
select cc.name as CheckName,
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
         CheckName
```
## API Reference

### MetaReader Class

The main class for reading database metadata.

#### Methods

- `IReadOnlyList<ITableMeta> Read(string connectionString)`
  - Reads metadata using a connection string
  - Returns a list of table metadata objects

- `IReadOnlyList<ITableMeta> Read(SqlConnection openConnection)`
  - Reads metadata using an open connection
  - Returns a list of table metadata objects

### ITableMeta Interface

Represents a table or view in the database.

#### Properties

- `string Name` - The table or view name
- `string Type` - "BASE TABLE" or "VIEW"
- `string Description` - The extended property description
- `IReadOnlyList<IColumnMeta> Columns` - List of columns
- `IReadOnlyList<IForeignKeyMeta> ForeignKeys` - List of foreign keys
- `IReadOnlyList<IIndexMeta> Indexes` - List of indexes
- `IReadOnlyList<ICheckMeta> Checks` - List of check constraints

### IColumnMeta Interface

Represents a column in a table or view.

#### Properties

- `string Name` - Column name
- `string DataType` - SQL data type
- `bool IsNullable` - Whether the column allows NULL values
- `bool IsIdentity` - Whether the column is an identity column
- `string DefaultValue` - Default value expression
- `int? MaxTextLength` - Maximum text length for string columns
- `int? NumericPrecision` - Precision for numeric columns
- `int? NumericScale` - Scale for numeric columns
- `string Description` - Extended property description

### IForeignKeyMeta Interface

Represents a foreign key relationship.

#### Properties

- `string Name` - Foreign key constraint name
- `ITableMeta SourceTable` - The table containing the foreign key
- `IColumnMeta SourceColumn` - The column in the source table
- `ITableMeta TargetTable` - The referenced table
- `IColumnMeta TargetColumn` - The referenced column
- `string Description` - Extended property description

### IIndexMeta Interface

Represents an index on a table.

#### Properties

- `string Name` - Index name
- `string Type` - Index type (e.g., "clustered", "nonclustered")
- `bool IsUnique` - Whether the index is unique
- `bool IsPrimaryKey` - Whether the index is a primary key
- `IReadOnlyList<string> Columns` - List of column names in the index
- `string Description` - Extended property description

### ICheckMeta Interface

Represents a check constraint.

#### Properties

- `string Name` - Constraint name
- `string Definition` - The check constraint expression
- `string Description` - Extended property description

## Requirements

- SQL Server 2012 or later
- Read access to system tables and views
- Read access to extended properties

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

## Repository

Source code: [https://github.com/DJDoena/DoenaSoft.SqlServer.DatabaseMeta](https://github.com/DJDoena/DoenaSoft.SqlServer.DatabaseMeta)

## Author

DJ Doena - Doena Soft.

## Changelog

### Version 2.0.0
- Multi-targeting support for .NET Framework 4.7.2 and .NET 10.0
- Updated package references
- SDK-style project format
