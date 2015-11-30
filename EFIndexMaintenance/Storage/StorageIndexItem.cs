namespace EFIndexMaintenance.Storage
{
    /// <summary>
    /// Информация о индексах из хранилища
    /// </summary>
    public class StorageIndexItem
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
        /// Наименование индекса
        /// </summary>
        public string IndexName { get; set; }

        /// <summary>
        /// Наименование столбца
        /// </summary>
        public string ColumnName { get; set; }

        /// <summary>
        /// Порядковый номер столбца в таблице
        /// </summary>
        public int TableColumnOrder { get; set; }

        /// <summary>
        /// Порядковый номер столбца в индексе
        /// </summary>
        public int IndexColumnOrder { get; set; }

        /// <summary>
        /// Тип индекса (кластеризованный или нет)
        /// </summary>
        public string IndexType { get; set; }

        /// <summary>
        /// Identity
        /// </summary>
        public bool IsIdentity { get; set; }

        /// <summary>
        /// Unique
        /// </summary>
        public bool IsUnique { get; set; }

        /// <summary>
        /// Флаг первичного ключа
        /// </summary>
        public bool IsPrimary { get; set; }

        /// <summary>
        /// Сортировка
        /// </summary>
        public bool IsDescending { get; set; }

        /// <summary>
        /// Является ли столбец включенным в индекс
        /// </summary>
        public bool IsIncludedColumn { get; set; }
    }
}
