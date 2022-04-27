namespace DoenaSoft.SqlServerDatabaseMeta
{
    using System;
    using System.Reflection;

    internal sealed class ColumnMeta : MetaBase, IColumnMeta
    {
        public ITableMeta Table { get; }

        public TableType TableType => this.Table.Type;

        public int Index { get; }

        public int? ColumnId { get; }

        public SqlDataType SqlDataType { get; }

        public Type NetDataType => this.GetNetDataType();

        public string DefaultValue { get; }

        public bool IsNullable { get; internal set; }

        public bool? IsIdentity { get; internal set; }

        public int? NumericPrecision { get; internal set; }

        public int? NumericScale { get; internal set; }

        public int? MaxTextLength { get; internal set; }

        public string TextCollation { get; internal set; }

        internal ColumnMeta(string name, string description, ITableMeta table, int index, int? columId, string dataType, string defaultValue) : base(name, description)
        {
            this.Table = table;
            this.Index = index;
            this.ColumnId = columId;
            this.SqlDataType = GetSqlDataType(dataType);
            this.DefaultValue = defaultValue;
        }

        public override string ToString() => $"Column: {this.Table.Name}.{base.ToString()}";

        public override int GetHashCode() => base.GetHashCode();

        public override bool Equals(object obj)
        {
            if (!(obj is IColumnMeta other))
            {
                return false;
            }

            return this.MetaId.Equals(other.MetaId);
        }

        private static SqlDataType GetSqlDataType(string input)
        {
            if (Enum.TryParse<SqlDataType>(input.ToLower(), out var result))
            {
                return result;
            }
            else
            {
                return SqlDataType.Unknown;
            }
        }

        private Type GetNetDataType()
        {
            switch (this.SqlDataType)
            {
                case SqlDataType.bigint:
                    {
                        return this.IsNullable ? typeof(long?) : typeof(long);
                    }
                case SqlDataType.@int:
                    {
                        return this.IsNullable ? typeof(int?) : typeof(int);
                    }
                case SqlDataType.smallint:
                    {
                        return this.IsNullable ? typeof(short?) : typeof(short);
                    }
                case SqlDataType.tinyint:
                    {
                        return this.IsNullable ? typeof(byte?) : typeof(byte);
                    }
                case SqlDataType.@decimal:
                case SqlDataType.money:
                case SqlDataType.numeric:
                case SqlDataType.smallmoney:
                    {
                        return this.IsNullable ? typeof(decimal?) : typeof(decimal);
                    }
                case SqlDataType.@float:
                    {
                        return this.IsNullable ? typeof(double?) : typeof(double);
                    }
                case SqlDataType.real:
                    {
                        return this.IsNullable ? typeof(float?) : typeof(float);
                    }
                case SqlDataType.binary:
                case SqlDataType.image:
                case SqlDataType.timestamp: //rowversion
                case SqlDataType.varbinary:
                    {
                        return typeof(byte[]);
                    }
                case SqlDataType.bit:
                    {
                        return this.IsNullable ? typeof(bool?) : typeof(bool);
                    }
                case SqlDataType.@char:
                case SqlDataType.nchar:
                case SqlDataType.ntext:
                case SqlDataType.nvarchar:
                case SqlDataType.text:
                case SqlDataType.varchar:
                case SqlDataType.xml:
                    {
                        return typeof(string);
                    }
                case SqlDataType.date:
                case SqlDataType.datetime:
                case SqlDataType.datetime2:
                case SqlDataType.smalldatetime:
                    {
                        return this.IsNullable ? typeof(DateTime?) : typeof(DateTime);
                    }
                case SqlDataType.datetimeoffset:
                    {
                        return this.IsNullable ? typeof(DateTimeOffset?) : typeof(DateTimeOffset);
                    }
                case SqlDataType.time:
                    {
                        return this.IsNullable ? typeof(TimeSpan?) : typeof(TimeSpan);
                    }
                case SqlDataType.uniqueidentifier:
                    {
                        return this.IsNullable ? typeof(Guid?) : typeof(Guid);
                    }
                case SqlDataType.geography:
                    {
                        return GetEntityFrameworkType("System.Data.Entity.Spatial.DbGeography");
                    }
                case SqlDataType.geometry:
                    {
                        return GetEntityFrameworkType("System.Data.Entity.Spatial.DbGeometry");
                    }
                case SqlDataType.sql_variant:
                case SqlDataType.hierarchyid:
                default:
                    {
                        throw new NotSupportedException($"'{this.SqlDataType}' is currently not supported");
                    }
            }
        }

        private static Type GetEntityFrameworkType(string typeName)
        {
            const string AssemblyName = "EntityFramework.dll";

            try
            {
                var assembly = Assembly.LoadFrom(AssemblyName);

                var type = assembly.GetType(typeName);

                return type;
            }
            catch
            {
                throw new NotSupportedException($"Could not load type '{typeName}' from assembly '{AssemblyName}'.");
            }
        }
    }
}