using System;

namespace EFIndexMaintenance
{
    /// <summary>
    /// Информация о таблице
    /// </summary>
    public class TableInfo 
    {
        /// <summary>
        /// Наименование схемы
        /// </summary>
        public String Schema { get; private set; }

        /// <summary>
        /// Наименование таблицы
        /// </summary>
        public String Name { get; private set; }

        /// <summary>
        /// Информация о таблице
        /// </summary>
        /// <param name="schema">Наименование схемы</param>
        /// <param name="name">Наименование таблицы</param>
        public TableInfo(string schema, string name)
        {            
            Schema = schema;
            Name = name;
        }

        public override bool Equals(object otherObject)
        {
            var other = otherObject as TableInfo;

            if (other == null)
                return false;

            return Schema == other.Schema && Name == other.Name;
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode() ^ Schema.GetHashCode();
        }

        public override string ToString()
        {
            return string.Format("{0}.{1}", Schema, Name);
        }
    }
}
