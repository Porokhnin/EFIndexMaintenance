using System;
using System.Collections.Generic;

namespace EntityFrameworkIndexSuppoter
{
    /// <summary>
    /// Индекс сущности
    /// </summary>
    public class IndexInfo
    {
        /// <summary>
        /// Имя индекса
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Создает уникальный индекс для таблицы или представления.Уникальным является индекс, в котором не может быть двух строк с одним и тем же значением ключа индекса.Кластеризованный индекс представления должен быть уникальным.
        /// </summary>
        public bool IsUnique { get; set; }

        /// <summary>
        /// Создает индекс, в котором логический порядок значений ключа определяет физический порядок соответствующих строк в таблице.На нижнем (конечном) уровне кластеризованного индекса хранятся действительные строки данных таблицы.Для таблицы или представления в каждый момент времени может существовать только один кластеризованный индекс.
        /// </summary>
        public bool IsClustered { get; set; }

        /// <summary>
        /// Информация о таблице в базе данных для этой сущности
        /// </summary>
        public TableInfo TableInfo { get; set; }

        /// <summary>
        /// Информация о свойствах по которым строится индекс
        /// </summary>
        public List<IndexMemberInfo> IndexMemberInfoCollection { get; set; }

        /// <summary>
        /// Информация о свойствах включающихся в индекс
        /// </summary>
        public List<IndexIncludeInfo> IndexIncludeInfoCollection { get; set; }

        /// <summary>
        /// Создать экземпляр IndexInfo
        /// </summary>
        public IndexInfo()
        {
            IndexMemberInfoCollection = new List<IndexMemberInfo>();
            IndexIncludeInfoCollection = new List<IndexIncludeInfo>();
        }

        public override bool Equals(object otherObject)
        {
            var other = otherObject as IndexInfo;

            if (other == null)
                return false;

            if (Object.ReferenceEquals(this, other))
                return true;

            return Name == other.Name && TableInfo.Name == other.TableInfo.Name;
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode() ^ TableInfo.Name.GetHashCode();
        }

        public override string ToString()
        {
            return string.Format("{0} {1}", TableInfo, Name);
        }
    }
}
