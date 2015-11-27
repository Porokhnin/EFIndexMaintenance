using System;
using System.Data.Entity;
using System.Data.SqlClient;

namespace EntityFrameworkIndexSuppoter.Command
{
    /// <summary>
    /// Команда для работы с индексами
    /// </summary>
    public abstract class IndexCommand
    {
        /// <summary>
        /// Индекс сущности
        /// </summary>
        public IndexInfo IndexInfo { get; set; }

        /// <summary>
        /// Получить скрипт команды
        /// </summary>
        /// <returns>Скрипт команды</returns>
        protected abstract String GetCommandScript();

        /// <summary>
        /// Залогировать команду
        /// </summary>
        protected abstract void LogCommand();

        /// <summary>
        /// Выполнить
        /// </summary>
        public void Execute(DbContext context)
        {
            LogCommand();
            string commandScript = GetCommandScript();
            context.Database.ExecuteSqlCommand(TransactionalBehavior.DoNotEnsureTransaction, commandScript);
        }

        /// <summary>
        /// Выполнить
        /// </summary>
        public void Execute(string connectionString)
        {
            LogCommand();
            string commandScript = GetCommandScript();

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                using (SqlCommand command = new SqlCommand(commandScript, connection))
                {
                    command.ExecuteNonQuery();                  
                }
            }
        }

        public override string ToString()
        {
            return string.Format("{0} {1}", IndexInfo.Name, IndexInfo.TableInfo.Name);
        }
    }
}
