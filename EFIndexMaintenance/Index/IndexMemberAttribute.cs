using System;

namespace EFIndexMaintenance.Index
{
    /// <summary>
    /// Атрибут столбца по которому строится индекс.
    /// Отражает свойства столбца по которому строится индекс. 
    /// Атрибутом помечаются свойства по которым сторится индекс.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, Inherited = true, AllowMultiple = true)]
    public class IndexMemberAttribute : Attribute
    {
        /// <summary>
        /// Имя индекса
        /// </summary>
        public string IndexName { get; private set; }

        /// <summary>
        /// Порядковый номер столбца в индексе.
        /// Отдельная нумерация для столбцов по которым строится индекс и которые включаются в индес
        /// </summary>
        public int MemberOrder { get; private set; }

        /// <summary>
        /// Направление сортировки базовых столбцов
        /// </summary>
        public SortDirection SortDirection { get; set; }

        /// <summary>
        /// Создать атрибут
        /// </summary>
        /// <param name="indexName">Имя индекса</param>
        /// <param name="memberOrde">Порядковый номер столбца в индексе</param>
        public IndexMemberAttribute(string indexName, int memberOrde)
        {
            if (string.IsNullOrEmpty(indexName))
                throw new ArgumentException("Имя индекса не может быть Null Or Empty");

            IndexName = indexName;
            MemberOrder = memberOrde;
            SortDirection = SortDirection.Asc;
        }
    }
}
