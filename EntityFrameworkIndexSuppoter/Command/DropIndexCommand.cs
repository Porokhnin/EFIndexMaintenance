using System;
using System.Text;
using NLog;

namespace EntityFrameworkIndexSuppoter.Command
{
    /// <summary>
    /// Команда удаления индекса
    /// </summary>
    public class DropIndexCommand : IndexCommand
    {
        private readonly ILogger _logger;

        public DropIndexCommand()
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
            commandBuilder.Append(String.Format(@"IF EXISTS (SELECT name FROM sys.indexes WHERE name = N'{0}' AND object_id = object_id(N'[{1}].[{2}]', N'U')) " +
                                                 "DROP INDEX [{0}] ON [{1}].[{2}]", IndexInfo.Name, IndexInfo.TableInfo.Schema, IndexInfo.TableInfo.Name));
            return commandBuilder.ToString();
        }

        protected override void LogCommand()
        {
            _logger.Log(LogLevel.Info, "Удаление индекса: {0} Таблица: {1}", IndexInfo.Name, IndexInfo.TableInfo.Name);
        }
    }
}
