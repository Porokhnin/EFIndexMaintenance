using System;
using System.Collections.Generic;

namespace EFIndexMaintenance.Mapping
{
    /// <summary>
    /// Mapping сущности
    /// </summary>
    public class EntityMapping
    {
        /// <summary>
        /// Сущность (класс)
        /// </summary>
        public Type Entity { get; set; }

        /// <summary>
        /// Информация о таблице в базе данных для этой сущности
        /// </summary>
        public TableInfo TableInfo { get; set; }

        /// <summary>
        /// Mapping свойств сущности
        /// </summary>
        public List<PropertyMapping> PropertyMappings { get; set; }
    }
}
