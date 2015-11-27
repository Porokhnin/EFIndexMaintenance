using System;

namespace EntityFrameworkIndexSuppoter.Storage
{
    public class SqlScripts
    {
        public static String SelectIndexes = "SELECT " +
                                                   "OBJECT_SCHEMA_NAME(t.[object_id], DB_ID()) AS [Schema]," +
                                                   "t.[name] AS [TableName]," +
                                                   "ind.[name] AS [IndexName]," +
                                                   "col.[name] AS [ColumnName]," +
                                                   "ic.column_id AS [TableColumnOrder]," +
	                                               "ic.index_column_id AS [IndexColumnOrder]," +
                                                   "ind.[type_desc] AS [IndexType]," +
                                                   "col.is_identity AS [IsIdentity]," +
                                                   "ind.[is_unique] AS [IsUnique]," +
                                                   "ind.[is_primary_key] AS [IsPrimary]," +
                                                   "ic.[is_descending_key] AS [IsDescending]," +
                                                   "ic.[is_included_column] AS [IsIncludedColumn] " +
                                               "FROM " +
                                                    "sys.indexes ind "+
                                               "INNER JOIN "+
                                                    "sys.index_columns ic "+
                                                    "ON ind.object_id = ic.object_id AND ind.index_id = ic.index_id "+
                                               "INNER JOIN "+
                                                    "sys.columns col "+
                                                    "ON ic.object_id = col.object_id and ic.column_id = col.column_id "+
                                               "INNER JOIN "+
                                                    "sys.tables t "+
                                                    "ON ind.object_id = t.object_id "+
                                               "WHERE "+
                                                    "t.is_ms_shipped = 0 AND "+
                                                    "ind.is_primary_key = 0 "+
                                               "ORDER BY " +
                                                    "[Schema],"+
                                                    "[TableName],"+
                                                    "[IndexName],"+
                                                    "[IndexColumnOrder]," + 
                                                    "[ColumnName]" ;

            public static String SelectForeignKeyIndexes = "SELECT " +
                                                                "sc.name as [Schema],"+
	                                                            "t.name as TableName,"+
                                                                "OBJECT_NAME(fkc.constraint_object_id) as ForeignKeyName,"+
                                                                "OBJECT_NAME(fkc.referenced_object_id) ReferencedTableName,"+
	                                                            "c.name as ColumnName,"+
	                                                            "i.name as IndexName "+
                                                            "FROM    sys.foreign_key_columns fkc "+
                                                            "JOIN    sys.index_columns ic ON ic.object_id = fkc.parent_object_id AND ic.column_id = fkc.parent_column_id "+
                                                            "JOIN	sys.indexes i ON i.index_id = ic.index_id AND i.object_id = ic.object_id "+
                                                            "JOIN	sys.columns c ON c.object_id = ic.object_id AND c.column_id = ic.column_id "+
                                                            "JOIN    sys.tables t ON t.object_id = c.object_id "+
                                                            "JOIN    sys.schemas sc ON sc.schema_id = t.schema_id "+
                                                            "WHERE   t.is_ms_shipped = 0 " +
                                                            "ORDER BY tableName, OBJECT_NAME(fkc.constraint_object_id), columnName";

            public static String SelectForeignKeyInfo = "SELECT "+
                                                                "fk.name AS [ForeignKeyName]," +
                                                                "OBJECT_NAME(fk.parent_object_id) AS [TableName]," +
                                                                "c1.name AS [ColumnName],"+
                                                                "OBJECT_NAME(fk.referenced_object_id) AS [ReferencedTableName]," +
                                                                "c2.name AS [ReferencedColumnName] " +
                                                        "FROM sys.foreign_keys fk "+
                                                        "INNER JOIN sys.foreign_key_columns fkc "+
                                                            "ON fk.object_id = fkc.constraint_object_id "+
                                                        "INNER JOIN sys.columns c1 "+
                                                            "ON fkc.parent_object_id = c1.object_id AND fkc.parent_column_id = c1.column_id "+
                                                        "INNER JOIN sys.columns c2 "+
                                                            "ON fkc.referenced_object_id = c2.object_id AND fkc.referenced_column_id = c2.column_id";

    }
}
