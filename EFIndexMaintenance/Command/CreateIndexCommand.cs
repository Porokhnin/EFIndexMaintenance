using System.Linq;
using System.Text;
using EFIndexMaintenance.Index;
using NLog;

namespace EFIndexMaintenance.Command
{
    /// <summary>
    /// Команда создания индекса
    /// </summary>
    public class CreateIndexCommand : IndexCommand
    {
        private readonly ILogger _logger;

        public CreateIndexCommand()
        {
            _logger = LogManager.GetCurrentClassLogger();
        }

        /// <summary>
        /// Получить скрипт команды
        /// </summary>
        /// <returns>Скрипт команды</returns>
        protected override string GetCommandScript()
        {
            StringBuilder commandBuilder = new StringBuilder();
            commandBuilder.Append(
                string.Format("IF NOT EXISTS (SELECT name FROM sys.indexes WHERE name = N'{0}' AND object_id = object_id(N'[{1}].[{2}]', N'U')) ",
                               IndexInfo.Name, IndexInfo.TableInfo.Schema, IndexInfo.TableInfo.Name));
            commandBuilder.Append(
                string.Format("CREATE {0}{1} INDEX [{2}] ON [{3}].[{4}]",
                               IndexInfo.IsUnique ? "UNIQUE " : null, IndexInfo.IsClustered ? "CLUSTERED" : "NONCLUSTERED", IndexInfo.Name, IndexInfo.TableInfo.Schema, IndexInfo.TableInfo.Name));

            if (IndexInfo.IndexMemberInfoCollection.Any())
            {
                commandBuilder.Append("(");
                foreach (var memberInfo in IndexInfo.IndexMemberInfoCollection)
                {
                    commandBuilder.Append(string.Format("[{0}] {1}", memberInfo.ColumnInfo.Name, memberInfo.SortDirection == SortDirection.Asc ? "ASC" : "DESC"));

                    if (!memberInfo.Equals(IndexInfo.IndexMemberInfoCollection.Last()))
                        commandBuilder.Append(",");
                }
                commandBuilder.Append(")");

                if (IndexInfo.IndexIncludeInfoCollection.Any())
                {
                    commandBuilder.Append("INCLUDE (");
                    foreach (var includeInfo in IndexInfo.IndexIncludeInfoCollection)
                    {
                        commandBuilder.Append(string.Format("[{0}]", includeInfo.ColumnInfo.Name));

                        if (!includeInfo.Equals(IndexInfo.IndexIncludeInfoCollection.Last()))
                            commandBuilder.Append(",");
                    }
                    commandBuilder.Append(") ");
                }
                commandBuilder.Append("WITH (SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF) ON [PRIMARY]");
            }
            return commandBuilder.ToString();
        }

        protected override void LogCommand()
        {
            _logger.Log(LogLevel.Info, "Создание индекса: {0} Таблица: {1}", IndexInfo.Name, IndexInfo.TableInfo.Name);
        }
    }
}
