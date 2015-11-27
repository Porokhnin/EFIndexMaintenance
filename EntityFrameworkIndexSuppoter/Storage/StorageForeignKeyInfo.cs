namespace EntityFrameworkIndexSuppoter.Storage
{
    /// <summary>
    /// Информация о внешних ключах из хранилища
    /// </summary>
    public class StorageForeignKeyInfo
    {        
        /// <summary>
        /// Наименование внешнего ключа
        /// </summary>
        public string ForeignKeyName { get; set; }

        /// <summary>
        /// Наименование таблицы
        /// </summary>
        public string TableName { get; set; }

        /// <summary>
        /// Наименование столбца
        /// </summary>
        public string ColumnName { get; set; }

        /// <summary>
        /// Наименование таблицы внешнего ключа
        /// </summary>
        public string ReferencedTableName { get; set; }

        /// <summary>
        /// Наименование столбца
        /// </summary>
        public string ReferencedColumnName { get; set; }
    }
}
