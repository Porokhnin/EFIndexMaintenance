namespace EntityFrameworkIndexSuppoter.Storage
{
    /// <summary>
    /// Информация о индексах из хранилища
    /// </summary>
    public class StorageForeignKeyIndexItem
    {
        /// <summary>
        /// Схема
        /// </summary>
        public string Schema { get; set; }

        /// <summary>
        /// Наименование таблицы
        /// </summary>
        public string TableName { get; set; }

        /// <summary>
        /// Наименование внешнего ключа
        /// </summary>
        public string ForeignKeyName { get; set; }

        /// <summary>
        /// Наименование таблицы внешнего ключа
        /// </summary>
        public string ReferencedTableName { get; set; }

        /// <summary>
        /// Наименование столбца
        /// </summary>
        public string ColumnName { get; set; }
        /// <summary>
        /// Наименование индекса
        /// </summary>
        public string IndexName { get; set; }
    }
}
