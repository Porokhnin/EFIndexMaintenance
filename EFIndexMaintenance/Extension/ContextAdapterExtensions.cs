using System;
using System.Data.Entity.Core.Mapping;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Infrastructure;
using System.Linq;

namespace EFIndexMaintenance.Extension
{
    public static class ContextAdapterExtensions
    {
        /// <summary>
        /// Получить имя таблицы для сущности
        /// </summary>
        /// <param name="context">Контекст</param>
        /// <param name="entity">Сущность</param>
        /// <returns>Имя таблицы</returns>
        public static string GetEntityTableName(this IObjectContextAdapter context, Type entity)
        {
            var metadata = context.ObjectContext.MetadataWorkspace;
            var mappingItemCollection = (StorageMappingItemCollection)metadata.GetItemCollection(DataSpace.CSSpace);
            var storeContainer = ((EntityContainerMapping)mappingItemCollection[0]).StoreEntityContainer;
            var baseEntitySet = storeContainer.BaseEntitySets.Single(es => es.Name == entity.Name);

            return baseEntitySet.Table;
        }

        /// <summary>
        /// Получить имя таблицы со схемой для сущности
        /// </summary>
        /// <param name="context">Контекст</param>
        /// <param name="entity">Сущность</param>
        /// <returns>Имя таблицы</returns>
        public static TableInfo GetEntityTableNameWithSchema(this IObjectContextAdapter context, Type entity) 
        {
            var metadata = context.ObjectContext.MetadataWorkspace;
            var mappingItemCollection = (StorageMappingItemCollection)metadata.GetItemCollection(DataSpace.CSSpace);
            var storeContainer = ((EntityContainerMapping)mappingItemCollection[0]).StoreEntityContainer;
            var baseEntitySet = storeContainer.BaseEntitySets.Single(es => es.Name == entity.Name);
            return new TableInfo(baseEntitySet.Schema, baseEntitySet.Table);
        }

        /// <summary>
        /// Получить имя таблицы для сущности
        /// </summary>
        /// <param name="context">Контекст</param>
        /// <param name="entity">Сущность</param>
        /// <returns>Имя таблицы</returns>
        public static string GetTableName2(this IObjectContextAdapter context, Type entity)
        {
            var metadata = context.ObjectContext.MetadataWorkspace;

            var entitySet = metadata.GetItems<EntityContainer>(DataSpace.CSpace)
                                    .Single()
                                    .EntitySets
                                    .Single(item => item.ElementType.Name == entity.Name);

            var entityMapping = metadata.GetItems<EntityContainerMapping>(DataSpace.CSSpace)
                                  .Single()
                                  .EntitySetMappings
                                  .Single(item => item.EntitySet == entitySet);

            var entityMappingTables = entityMapping.EntityTypeMappings.Single().Fragments;

            return entityMappingTables.Select(mt => (string)mt.StoreEntitySet.MetadataProperties["Table"].Value ?? mt.StoreEntitySet.Name).FirstOrDefault();
        }
    }
}
