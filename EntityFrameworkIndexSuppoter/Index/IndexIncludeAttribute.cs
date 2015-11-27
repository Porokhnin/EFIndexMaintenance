using System;

namespace EntityFrameworkIndexSuppoter.Index
{
    /// <summary>
    /// Атрибут столбца который включается в индекс.
    /// Отражает свойства столбца который включается в индекс. 
    /// Атрибутом помечаются свойства которые включаются в индекс.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, Inherited = true, AllowMultiple = true)]
    public class IndexIncludeAttribute : Attribute
    {
        /// <summary>
        /// Имя индекса
        /// </summary>
        public string IndexName { get; private set; }

        /// <summary>
        /// Порядковый номер столбца для включения в индекс.
        /// Отдельная нумерация для столбцов по которым строится индекс и которые включаются в индес
        /// </summary>
        public int IncludeOrder { get; private set; }

        /// <summary>
        /// Создать атрибут
        /// </summary>
        /// <param name="indexName">Имя индекса</param>
        /// <param name="includeOrder">Порядковый номер столбца для включения в индекс</param>
        public IndexIncludeAttribute(string indexName, int includeOrder)
        {
            if (string.IsNullOrEmpty(indexName))
                throw new ArgumentException("Имя индекса не может быть Null Or Empty");

            IndexName = indexName;
            IncludeOrder = includeOrder;
        }
    }
}
