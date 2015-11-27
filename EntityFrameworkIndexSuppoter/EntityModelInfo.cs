using System;
using System.Collections.Generic;
using EntityFrameworkIndexSuppoter.Mapping;

namespace EntityFrameworkIndexSuppoter
{
    /// <summary>
    /// Модель сущности
    /// </summary>
    public class EntityModelInfo
    {
        private readonly Type _entity;

        /// <summary>
        /// Сущность
        /// </summary>
        public Type Entity
        {
            get { return _entity; }
        }

        /// <summary>
        /// Mapping сущности
        /// </summary>
        public EntityMapping Mapping { get; set; }

        /// <summary>
        /// Индексы сущности
        /// </summary>
        public List<IndexInfo> ModelIndexes { get; set; }

        /// <summary>
        /// Индексы сущности в хранилище
        /// </summary>
        public List<IndexInfo> StorageIndexes { get; set; }

        /// <summary>
        /// Конструктор модели сущности
        /// </summary>
        /// <param name="entity">Сущность</param>
        public EntityModelInfo(Type entity)
        {
            _entity = entity;

            ModelIndexes = new List<IndexInfo>();
            StorageIndexes = new List<IndexInfo>();
        }
    }
}
