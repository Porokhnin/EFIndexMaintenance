using System;
using System.Reflection;

namespace EntityFrameworkIndexSuppoter.Mapping
{
    /// <summary>
    /// Mapping свойства
    /// </summary>
    public class PropertyMapping
    {
        /// <summary>
        /// Свойство
        /// </summary>
        public PropertyInfo Property { get; set; }

        /// <summary>
        /// Наименование столбца в базе данных для этого свойства
        /// </summary>
        public String ColumnName { get; set; }

        public override string ToString()
        {
            return string.Format("{0}->{1}", Property.Name, ColumnName);
        }
    }
}
