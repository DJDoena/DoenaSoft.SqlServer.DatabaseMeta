# SQL Server Database Meta Reader

[![NuGet](https://img.shields.io/nuget/v/DoenaSoft.SqlServerDatabaseMeta.svg)](https://www.nuget.org/packages/DoenaSoft.SqlServerDatabaseMeta)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

A .NET library that provides read-only access to SQL Server database metadata,
enabling you to programmatically discover and analyse the structure of a database.

## Supported Frameworks

- .NET Framework 4.7.2
- .NET 10.0

## Installation

```
dotnet add package DoenaSoft.SqlServerDatabaseMeta
```

Or via the Package Manager Console:

```
Install-Package DoenaSoft.SqlServerDatabaseMeta
```

## Features

| Category          | What is read                                                              |
|-------------------|---------------------------------------------------------------------------|
| Tables and Views  | Name, type (BASE TABLE / VIEW), MS_Description                            |
| Columns           | Name, data type, ordinal, nullability, identity, default, precision,      |
|                   | scale, max length, collation, MS_Description                              |
| Foreign Keys      | Name, source/target table and columns, column index pairs, MS_Description |
| Indexes           | Name, type flags (unique, primary key, clustered), key columns,           |
|                   | included columns, MS_Description                                          |
| Check Constraints | Name, table, SQL definition expression, MS_Description                    |
| Scalar Functions  | Name, schema, full CREATE FUNCTION definition text, MS_Description        |

Descriptions are read from the MS_Description extended property.

## API Overview

### Reading table metadata

```csharp
IMetaReader reader = new MetaReader();

// Option 1: individual credentials
IReadOnlyList<ITableMeta> tables = reader.Read("myServer", "myDatabase", "user", "password");

// Option 2: connection string
IReadOnlyList<ITableMeta> tables = reader.Read("Server=myServer;Database=myDatabase;...");

// Option 3: already-open connection
using var connection = new SqlConnection(connectionString);
connection.Open();
IReadOnlyList<ITableMeta> tables = reader.Read(connection);
```

### Reading scalar-valued functions

```csharp
IReadOnlyList<IScalarFunctionMeta> functions = reader.ReadScalarFunctions(connectionString);

foreach (IScalarFunctionMeta fn in functions)
{
    Console.WriteLine(fn.Schema + "." + fn.Name);
    Console.WriteLine(fn.Definition);  // full CREATE FUNCTION text
}
```

### Navigating the object graph

```csharp
foreach (ITableMeta table in tables)
{
    Console.WriteLine(table.Name + " (" + table.Type + ")");

    foreach (IColumnMeta col in table.Columms)
    {
        Console.WriteLine("  " + col.Name + " " + col.DataType
            + (col.IsNullable ? " NULL" : " NOT NULL"));
    }

    foreach (IIndexMeta idx in table.Indices)
    {
        Console.WriteLine("  INDEX " + idx.Name + " [" + idx.Properties + "]");
    }

    foreach (IForeignKeyMeta fk in table.OutgoingForeignKeys)
    {
        Console.WriteLine("  FK " + fk.Name + " -> " + fk.TargetTable.Name);
    }

    foreach (ICheckMeta ck in table.Checks)
    {
        Console.WriteLine("  CHECK " + ck.Name + ": " + ck.Check);
    }
}
```

## Key Interfaces

| Interface           | Description                                                |
|---------------------|-----------------------------------------------------------|
| IMetaReader         | Entry point; call Read() or ReadScalarFunctions()         |
| ITableMeta          | Table or view with columns, indexes, keys, and checks     |
| IColumnMeta         | Column with type info and constraints                     |
| IIndexMeta          | Index with key columns and included columns               |
| IForeignKeyMeta     | Foreign key with source/target table and column refs      |
| ICheckMeta          | Check constraint with SQL definition text                 |
| IScalarFunctionMeta | Scalar UDF with schema and CREATE FUNCTION body           |
| IMetaBase           | Base interface: MetaId (transient GUID), Name, Description|

All collections are IReadOnlyList<T>. MetaReader is non-static and can be
subclassed to override Read() or ReadScalarFunctions() for custom behaviour.

## Notes

- Only SQL Server is supported (uses System.Data.SqlClient).
- MetaId is a transient GUID generated at read time; it is not persisted and
  will differ between runs.
- Descriptions are taken from the MS_Description extended property where present.
- The Queries static class exposes all SQL strings used internally, which may be
  useful for diagnostics or custom tooling.

## License

MIT

## Changelog

### Version 2.2.0
- Migrated all data access from XSD typed datasets to raw `SqlCommand`/`SqlDataReader`
- Added `IScalarFunctionMeta` interface to expose scalar-valued function metadata
- Added `ReadScalarFunctions()` overloads to `IMetaReader` and `MetaReader`
- Removed dependency on `System.Data.DataSetExtensions`

### Version 2.1.0
- Added `IncludedColumns` property to `IIndexMeta` to expose non-key covering index columns

### Version 2.0.0
- Multi-targeting support for .NET Framework 4.7.2 and .NET 10.0
- Updated package references
- SDK-style project format
