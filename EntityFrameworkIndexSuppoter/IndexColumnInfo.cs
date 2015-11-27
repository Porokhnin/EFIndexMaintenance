using System;

namespace EntityFrameworkIndexSuppoter
{
    /// <summary>
    /// Информаия об участии свойства в индексе
    /// </summary>
    public abstract class IndexColumnInfo
    {        
        /// <summary>
        /// Информация о столбце столбца в базе данных
        /// </summary>
        public ColumnInfo ColumnInfo { get; set; }

        /// <summary>
        /// Имя индекса
        /// </summary>
        public String IndexName { get; set; }

        /// <summary>
        /// Порядковый номер столбца в индексе.
        /// Отдельная нумерация для столбцов по которым строится индекс и которые включаются в индекс
        /// </summary>
        public int ColumnOrder { get; set; }

        public override string ToString()
        {
            return string.Format("{0} {1} {2}", IndexName, ColumnInfo.Name, ColumnOrder);
        }
    }
}
