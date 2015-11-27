using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Data.Entity.Migrations.Infrastructure;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using EntityFrameworkIndexSuppoter.Command;
using EntityFrameworkIndexSuppoter.Extension;
using EntityFrameworkIndexSuppoter.Index;
using EntityFrameworkIndexSuppoter.Mapping;
using EntityFrameworkIndexSuppoter.Storage;
using NLog;

namespace EntityFrameworkIndexSuppoter
{
    /// <summary>
    /// Обновлятор индексов
    /// </summary>
    public class IndexUpdater
    {
        /// <summary>
        /// Логер
        /// </summary>
        private ILogger _logger;

        public IndexUpdater()
        {
            _logger = LogManager.GetCurrentClassLogger();
        }

        /// <summary>
        /// Удалить индекс, в котором содержится столбцы, которые будут удалены в миграциях
        /// </summary>
        /// <param name="contextType">Тип контекста</param>
        public void DeleteIndexForDroppingColumns(Type contextType)
        {
            string connectionString = ConfigurationManager.ConnectionStrings[contextType.Name].ConnectionString;

            var configuration = GetMigrationsConfiguration();
            try
            {
                var indexInfoCollection = GetIndexInfoCollectionFromStorage(connectionString);
                var droppingColumns = GetDroppingColumnFromMigrationsConfiguration(configuration);

                var droppingIndexes = new List<IndexInfo>();
                foreach (var droppingColumn in droppingColumns)
                {
                    droppingIndexes.AddRange(indexInfoCollection.Where(i => i.TableInfo.Equals(droppingColumn.TableInfo) &&
                        (i.IndexIncludeInfoCollection.Select(ii => ii.ColumnInfo).Contains(droppingColumn) || i.IndexMemberInfoCollection.Select(ii => ii.ColumnInfo).Contains(droppingColumn))));
                }

                //Удаляем индексы в которых предполагается удаление столбца
                foreach (var droppingIndex in droppingIndexes)
                {
                    new DropIndexCommand() {IndexInfo = droppingIndex}.Execute(connectionString);
                }
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, ex);
            }
        }

        /// <summary>
        /// Получить список столбцов которые будут удалены
        /// </summary>
        /// <param name="configuration">Конфигурация миграций</param>
        /// <returns>Список столбцов которые будут удалены</returns>
        private IEnumerable<ColumnInfo> GetDroppingColumnFromMigrationsConfiguration(DbMigrationsConfiguration configuration)
        {
            var droppingColumn = new List<ColumnInfo>();

            if (configuration != null)
            {
                var migrator = new DbMigrator(configuration);

                var pendingMigrations = migrator.GetPendingMigrations();
                if (pendingMigrations.Any())
                {
                    var targetMigration = pendingMigrations.Last();

                    var scriptor = new MigratorScriptingDecorator(migrator);
                    string script = scriptor.ScriptUpdate(sourceMigration: null, targetMigration: targetMigration);
                    var parts = script.Split(new[] {"\r\n\r\n", "\r\n"}, StringSplitOptions.RemoveEmptyEntries).ToList();//разбиваем скрипт по миграциям
                    parts.RemoveAll(p => p.StartsWith("INSERT [dbo].[__MigrationHistory]") || p.StartsWith("VALUES"));//удаляем вставки в MigrationHistory

                    var dropColumnParts = parts.Where(p => p.Contains("DROP COLUMN"));//находим DROP COLUMN
                    foreach (var dropColumnPart in dropColumnParts)
                    {
                        Regex regex = new Regex("ALTER TABLE (?<schemaWithTable>.*) DROP COLUMN (?<column>.*)");
                        Match match = regex.Match(dropColumnPart);
                        string[] schemaWithTable = match.Groups["schemaWithTable"].Value.Split(new[] { '.', '[', ']' }, StringSplitOptions.RemoveEmptyEntries);
                        string schema = schemaWithTable.First();
                        string table = schemaWithTable.Last();
                        string column = match.Groups["column"].Value.Trim(new[] { '[', ']' });

                        droppingColumn.Add(new ColumnInfo(new TableInfo(schema, table), column));
                    }
                }
            }
            return droppingColumn;
        }

        /// <summary>
        /// Получить игформацию об индексах
        /// </summary>
        /// <returns>Коллекция информации об индексах</returns>
        private List<IndexInfo> GetIndexInfoCollectionFromStorage(string connectionString)
        {
            var indexInfoCollection = new List<IndexInfo>();
            var storageIndexItemCollection = new List<StorageIndexItem>();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand(SqlScripts.SelectIndexes, connection))
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            storageIndexItemCollection.Add(new StorageIndexItem()
                            {
                                Schema = reader.GetString(0),
                                TableName = reader.GetString(1),
                                IndexName = reader.GetString(2),
                                ColumnName = reader.GetString(3),
                                TableColumnOrder = reader.GetInt32(4),
                                IndexColumnOrder = reader.GetInt32(5),
                                IndexType = reader.GetString(6),
                                IsIdentity = reader.GetBoolean(7),
                                IsUnique = reader.GetBoolean(8),
                                IsPrimary = reader.GetBoolean(9),
                                IsDescending = reader.GetBoolean(10),
                                IsIncludedColumn = reader.GetBoolean(11),
                            });
                        }
                    }
                }
            }

            var tables = storageIndexItemCollection.Select(i => new TableInfo(i.Schema, i.TableName)).Distinct<TableInfo>();
            foreach (var tableInfo in tables)
            {
                indexInfoCollection.AddRange(GetStorageIndexInfo(storageIndexItemCollection, tableInfo));
            }

            return indexInfoCollection;
        }

        /// <summary>
        /// Получить конфигурацию миграций
        /// </summary>
        /// <returns>Конфигураци миграций</returns>
        private DbMigrationsConfiguration GetMigrationsConfiguration()
        {
            DbMigrationsConfiguration configuration = null;

            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies().Where(a => a.GetName().Name != "EntityFramework"))
            {
                var configurationTypes = assembly.GetTypes().Where(t => t.BaseType != null && t.BaseType.IsGenericType && t.BaseType.GetGenericTypeDefinition() == typeof(DbMigrationsConfiguration<>));
                if (configurationTypes.Any())
                {
                    configuration = (DbMigrationsConfiguration)Activator.CreateInstance(configurationTypes.First());
                    break;
                }
            }
            return configuration;
        }

        /// <summary>
        /// Обновить индексы
        /// </summary>
        /// <param name="context">Контекст</param>
        public void UpdateIndexes(DbContext context)
        {
            var entityModels = GetContextEntityModels(context);

            LogEntityIndexesInfo(entityModels);

            var storageForeignKeyInfoCollection = context.GetItemCollection<StorageForeignKeyInfo>(SqlScripts.SelectForeignKeyInfo);

            var indexCommands = AnalyzeIndexes(entityModels, storageForeignKeyInfoCollection);

            ExecuteIndexCommands(context, indexCommands);
        }

        /// <summary>
        /// Анализ контекста
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        private IEnumerable<EntityModelInfo> GetContextEntityModels(DbContext context)
        {
            var entityModels = new List<EntityModelInfo>();

            var entities = context.GetEntities();
            var storageIndexItemCollection = context.GetItemCollection<StorageIndexItem>(SqlScripts.SelectIndexes);

            foreach (var entity in entities)
            {
                var mapping = GetEntityMapping(context, entity);
                var modelIndexes = GetIndexInfoCollection(entity, mapping);
                var storageIndexes = GetStorageIndexInfo(storageIndexItemCollection, mapping.TableInfo);

                entityModels.Add(new EntityModelInfo(entity)
                {
                    Mapping = mapping,
                    ModelIndexes = modelIndexes,
                    StorageIndexes = storageIndexes
                });
            }

            return entityModels;
        }

        /// <summary>
        /// Проанализировать индексы и создать команды для обновления
        /// </summary>
        /// <param name="entityModels">Модели сущностей</param>
        /// <param name="storageForeignKeyInfoCollection">Информация о внешних ключах</param>
        private List<IndexCommand> AnalyzeIndexes(IEnumerable<EntityModelInfo> entityModels, IEnumerable<StorageForeignKeyInfo> storageForeignKeyInfoCollection)
        {
            var indexCommands = new List<IndexCommand>();
            foreach (var entityModel in entityModels)
            {
                var modelIndexes = entityModel.ModelIndexes;
                var storageIndexes = entityModel.StorageIndexes;
                var tableForeignKeyInfoCollection = storageForeignKeyInfoCollection.Where(fki => fki.TableName == entityModel.Mapping.TableInfo.Name);

                storageIndexes.RemoveAll(si => si.IsUnique);//исключаем уникальные (какой нибудь нужный)
                storageIndexes.RemoveAll(si => tableForeignKeyInfoCollection.Select(t => string.Format("IX_{0}", t.ColumnName)).Contains(si.Name)); //исключаем внешние ключи
                var storageIndexesToRemove = storageIndexes.Except(modelIndexes);//исключаем те которые есть в модели

                //Удаление
                foreach (var storageIndexeToRemove in storageIndexesToRemove)
                {
                    indexCommands.Add(new DropIndexCommand() { IndexInfo = storageIndexeToRemove });
                }

                //Создание
                foreach (var modelIndex in modelIndexes)
                {
                    var storageIndex = storageIndexes.FirstOrDefault(si => si.Name.Equals(modelIndex.Name));
                    if (storageIndex == null)
                    {
                        indexCommands.Add(new CreateIndexCommand() { IndexInfo = modelIndex });
                    }
                }
            }
            return indexCommands;
        }

        /// <summary>
        /// Выполнить все команды
        /// </summary>
        /// <param name="context">Контекст</param>
        /// <param name="indexCommands">Команды для работы с нидексами</param>
        private void ExecuteIndexCommands(DbContext context, IEnumerable<IndexCommand> indexCommands)
        {
            //Выполняем все команды
            foreach (var indexCommand in indexCommands)
            {
                try
                {
                    indexCommand.Execute(context);
                }
                catch (Exception ex)
                {
                    _logger.Log(LogLevel.Fatal, ex);
                }
            }
        }

        /// <summary>
        /// Получить mapping для сущности
        /// </summary>
        /// <param name="context">Контекст</param>
        /// <param name="entity">Сущность</param>
        /// <returns>Коллекция mapping</returns>
        private EntityMapping GetEntityMapping(DbContext context, Type entity)
        {
            var tableInfo = context.GetEntityTableNameWithSchema(entity);
            var propertyMappings = context.GetEntityPropertyMapping(entity);

            var entityMapping = new EntityMapping()
            {
                Entity = entity,
                TableInfo = tableInfo,
                PropertyMappings = propertyMappings
            };
            
            return entityMapping;
        }

        /// <summary>
        /// Получить индексы для сущности
        /// </summary>
        /// <param name="entity">Сущность</param>
        /// <param name="entityMapping">Mapping сущности</param>
        /// <exception cref="BadIndexColumnOrderException">Ошибка упарядочивания столбцов индекса</exception>
        /// <returns>Коллекия индексов</returns>
        private List<IndexInfo> GetIndexInfoCollection(Type entity, EntityMapping entityMapping)
        {
            var indexes = new List<IndexInfo>();
            var indexMemberInfoCollection = new List<IndexMemberInfo>();
            var indexIncludeInfoCollection = new List<IndexIncludeInfo>();

            var propertyMappings = entityMapping.PropertyMappings;
            var properties = entity.GetProperties();
            var indexAttributes = entity.GetCustomAttributes(typeof (IndexAttribute), inherit: false) as IEnumerable<IndexAttribute>;
            
            foreach (var propertyMapping in propertyMappings)
            {
                var property = properties.First(p => p.Name == propertyMapping.Property.Name);
                var indexMemberAttributes = property.GetCustomAttributes(typeof(IndexMemberAttribute), inherit: false) as IEnumerable<IndexMemberAttribute>;
                foreach (var indexMemberAttribute in indexMemberAttributes)
                {
                    indexMemberInfoCollection.Add(new IndexMemberInfo()
                    {
                        ColumnInfo = new ColumnInfo(entityMapping.TableInfo, propertyMapping.ColumnName),
                        IndexName = indexMemberAttribute.IndexName,
                        ColumnOrder = indexMemberAttribute.MemberOrder,
                        SortDirection = indexMemberAttribute.SortDirection
                    });
                }

                var indexIncludeAttributes = property.GetCustomAttributes(typeof(IndexIncludeAttribute), inherit: false) as IEnumerable<IndexIncludeAttribute>;
                foreach (var indexIncludeAttribute in indexIncludeAttributes)
                {
                    indexIncludeInfoCollection.Add(new IndexIncludeInfo()
                    {
                        ColumnInfo = new ColumnInfo(entityMapping.TableInfo, propertyMapping.ColumnName),
                        IndexName = indexIncludeAttribute.IndexName,
                        ColumnOrder = indexIncludeAttribute.IncludeOrder,
                    });
                }
            }

            foreach (var indexAttribute in indexAttributes)
            {
                var entityIndexInfo = new IndexInfo()
                {
                    Name = indexAttribute.Name,
                    IsClustered = indexAttribute.IsClustered,
                    IsUnique = indexAttribute.IsUnique,
                    TableInfo = entityMapping.TableInfo
                };

                var currentIndexMemberInfoCollection = indexMemberInfoCollection.Where(a => a.IndexName == indexAttribute.Name).ToList();
                var currentIndexIncludeInfoCollection = indexIncludeInfoCollection.Where(a => a.IndexName == indexAttribute.Name).ToList();

                try
                {
                    currentIndexMemberInfoCollection.Sort(CompareIndexInfoOrder);
                    currentIndexIncludeInfoCollection.Sort(CompareIndexInfoOrder);
                }
                catch (Exception ex)
                {
                    throw new BadIndexColumnOrderException("Ошибка упарядочивания столбцов индекса", ex);
                }
                
                entityIndexInfo.IndexMemberInfoCollection.AddRange(currentIndexMemberInfoCollection);
                entityIndexInfo.IndexIncludeInfoCollection.AddRange(currentIndexIncludeInfoCollection);

                indexes.Add(entityIndexInfo);
            }
            return indexes;
        }

        /// <summary>
        /// Получить информацию об индексах
        /// </summary>
        /// <param name="storageIndexItemInfoCollection">Информация об индексах из хранилища</param>
        /// <param name="tableInfo">Информация о таблице</param>
        /// <returns>Информацию об индексах</returns>
        private List<IndexInfo> GetStorageIndexInfo(IEnumerable<StorageIndexItem> storageIndexItemInfoCollection, TableInfo tableInfo)
        {
            var indexes = new List<IndexInfo>();

            //группируем по таблицам
            List<StorageIndexItem> tableIndexGroupInfo = storageIndexItemInfoCollection.Where(i => i.TableName == tableInfo.Name).ToList();

            //группируем по именам индексов
            var indexNameGroups = tableIndexGroupInfo.GroupBy(i => i.IndexName).ToList();

            foreach (var indexNameGroup in indexNameGroups)
            {
                var isClustered = false;
                var isUnique = false;

                var entityIndexInfo = new IndexInfo() { Name = indexNameGroup.Key, TableInfo = tableInfo };

                var indexMemberItems = indexNameGroup.Where(i => i.IsIncludedColumn == false).ToList();
                var indexIncludeItems = indexNameGroup.Where(i => i.IsIncludedColumn == true).ToList();

                indexMemberItems.Sort(CompareStorageIndexItemOrder);
                indexIncludeItems.Sort(CompareStorageIndexItemOrder);

                int memberColumnOrder = 0;
                int includeColumnOrder = 0;
                foreach (var indexMemberItem in indexMemberItems)
                {
                    isClustered = indexMemberItem.IndexType.Equals("CLUSTERED");
                    isUnique = indexMemberItem.IsUnique;

                    var indexMemberInfo = new IndexMemberInfo()
                    {
                        ColumnInfo = new ColumnInfo(tableInfo, indexMemberItem.ColumnName),
                        IndexName = indexMemberItem.IndexName,
                        ColumnOrder = memberColumnOrder,
                        SortDirection = indexMemberItem.IsDescending == true ? SortDirection.Desc : SortDirection.Asc
                    };
                    entityIndexInfo.IndexMemberInfoCollection.Add(indexMemberInfo);

                    memberColumnOrder += 1;
                }

                foreach (var indexIncludeItem in indexIncludeItems)
                {
                    isClustered = indexIncludeItem.IndexType.Equals("CLUSTERED");
                    isUnique = indexIncludeItem.IsUnique;

                    var indexMemberInfo = new IndexIncludeInfo()
                    {
                        ColumnInfo = new ColumnInfo(tableInfo, indexIncludeItem.ColumnName),
                        IndexName = indexIncludeItem.IndexName,
                        ColumnOrder = includeColumnOrder,
                    };
                    entityIndexInfo.IndexIncludeInfoCollection.Add(indexMemberInfo);

                    includeColumnOrder += 1;
                }
                entityIndexInfo.IsClustered = isClustered;
                entityIndexInfo.IsUnique = isUnique;

                indexes.Add(entityIndexInfo);
            }

            return indexes;
        }

        /// <summary>
        /// Залогировать информацию обо всех индексах
        /// </summary>
        /// <param name="entityModels">Коллекция моделей сущностей</param>
        private void LogEntityIndexesInfo(IEnumerable<EntityModelInfo> entityModels)
        {
            foreach (var entityModel in entityModels)
            {
                var modelIndexes = entityModel.ModelIndexes;
                var storageIndexes = entityModel.StorageIndexes;

                _logger.Log(LogLevel.Info, "Индексы таблицы {0}", entityModel.Mapping.TableInfo.Name);
                if (modelIndexes.Any())
                {
                    _logger.Log(LogLevel.Info, "Индексы модели");
                    foreach (var modelIndex in modelIndexes)
                    {
                        LogEntityIndexeInfo(modelIndex);
                    }
                }
                if (storageIndexes.Any())
                {
                    _logger.Log(LogLevel.Info, "Индексы в хранилище");
                    foreach (var storageIndex in storageIndexes)
                    {
                        LogEntityIndexeInfo(storageIndex);
                    }
                }
            }
        }

        /// <summary>
        /// Залогировать информацию обо всех индексах
        /// </summary>
        /// <param name="modelIndex">Информация об индексе сущности</param>
        private void LogEntityIndexeInfo(IndexInfo modelIndex)
        {
            _logger.Log(LogLevel.Info, "Индекс: {0} IsClustered: {1} IsUnique: {2}",
                                          modelIndex.Name, modelIndex.IsClustered, modelIndex.IsUnique);

            var indexColumnBuilder = new StringBuilder();
            if (modelIndex.IndexMemberInfoCollection.Any())
            {
                indexColumnBuilder.Append("ON [");
                foreach (var memberInfo in modelIndex.IndexMemberInfoCollection)
                {
                    indexColumnBuilder.Append(string.Format("{0} {1} {2}",
                        memberInfo.ColumnInfo.Name, memberInfo.ColumnOrder, memberInfo.SortDirection == SortDirection.Asc ? "ASC" : "DESC"));

                    if (!memberInfo.Equals(modelIndex.IndexMemberInfoCollection.Last()))
                        indexColumnBuilder.Append(",");
                }
                indexColumnBuilder.Append("]");
            }

            if (modelIndex.IndexIncludeInfoCollection.Any())
            {
                indexColumnBuilder.Append(" INCLUDE [");
                foreach (var includeInfo in modelIndex.IndexIncludeInfoCollection)
                {
                    indexColumnBuilder.Append(string.Format("{0} {1}", includeInfo.ColumnInfo.Name, includeInfo.ColumnOrder));

                    if (!includeInfo.Equals(modelIndex.IndexIncludeInfoCollection.Last()))
                        indexColumnBuilder.Append(",");
                }
                indexColumnBuilder.Append("]");
            }

            _logger.Log(LogLevel.Info, indexColumnBuilder.ToString());
        }

        /// <summary>
        /// Сравнить порядковые номера столбцов индекса
        /// </summary>
        /// <param name="x">Информация об индексе</param>
        /// <param name="y">Информация об индексе</param>
        /// <exception cref="SameIndexColumnOrderException">Ошибка</exception>
        /// <returns>Результат сравнения</returns>
        private int CompareIndexInfoOrder(IndexColumnInfo x, IndexColumnInfo y)
        {
            var result = x.ColumnOrder.CompareTo(y.ColumnOrder);

            if (result == 0) // равенство
                throw new SameIndexColumnOrderException("Одинаковый порядок столбцов индекса");

            return result;
        }

        /// <summary>
        /// Сравнение
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        private int CompareStorageIndexItemOrder(StorageIndexItem x, StorageIndexItem y)
        {
            return x.IndexColumnOrder.CompareTo(y.IndexColumnOrder);
        }
    }
}
