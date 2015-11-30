using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Core.Mapping;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Infrastructure;
using System.Linq;
using PropertyMapping = EFIndexMaintenance.Mapping.PropertyMapping;

namespace EFIndexMaintenance.Extension
{
    public static class DbContextExtensions
    {
        /// <summary>
        /// Получить сущности контекста
        /// </summary>
        /// <param name="context">Контекст</param>
        public static IEnumerable<Type> GetEntities(this DbContext context)
        {
            var entityTypes = new List<Type>();
            foreach (var property in context.GetType().GetProperties().Where(f => f.PropertyType.BaseType != null && f.PropertyType.BaseType.Name == "DbQuery`1"))
            {
                var entityType = property.PropertyType.GetGenericArguments().FirstOrDefault();
                if (entityType != null && entityType.BaseType != null && entityType.BaseType.FullName == "System.Object")
                {
                    entityTypes.Add(entityType);
                }
            }
            return entityTypes;
        }

        /// <summary>
        /// Получить mapping свойст сущности
        /// </summary>
        /// <param name="context">Контекст</param>
        /// <param name="entity">Сущность</param>
        /// <returns>Коллекция mapping свойст сущности</returns>
        public static List<PropertyMapping> GetEntityPropertyMapping(this DbContext context, Type entity)
        {
            List<PropertyMapping> propertyMappings = new List<PropertyMapping>();

            var properties = entity.GetProperties();
            var metadata = ((IObjectContextAdapter)context).ObjectContext.MetadataWorkspace;

            // Получаем EntitySet для сущности
            var entitySet = metadata
                .GetItems<EntityContainer>(DataSpace.CSpace)
                .Single()
                .EntitySets
                .Single(s => s.ElementType.Name == entity.Name);

            // Mapping между концептуальной моделью и моделью хранения(база данных)
            var mapping = metadata.GetItems<EntityContainerMapping>(DataSpace.CSSpace)
                .Single()
                .EntitySetMappings
                .Single(s => s.EntitySet == entitySet);

            foreach (var property in properties)
            {
                // Поиск для каждого свойства столбца в базе данных
                var propertyMapping = mapping
                    .EntityTypeMappings.Single()
                    .Fragments.Single()
                    .PropertyMappings
                    .OfType<ScalarPropertyMapping>()
                    .SingleOrDefault(m => m.Property.Name == property.Name);

                if (propertyMapping != null)
                {
                    var columnName = propertyMapping.Column.Name;
                    propertyMappings.Add(new PropertyMapping() { Property = property, ColumnName = columnName });
                }
            }
            return propertyMappings;
        }

        /// <summary>
        /// Получить коллекцию элементов
        /// </summary>
        public static List<T> GetItemCollection<T>(this DbContext context, String script)
        {
            return context.Database.SqlQuery<T>(script).ToList();
        }
    }
}
