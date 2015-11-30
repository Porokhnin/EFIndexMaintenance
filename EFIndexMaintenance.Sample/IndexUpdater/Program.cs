using System;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Reflection;
using EFIndexMaintenance.Database;
using EFIndexMaintenance.Database.Migrations;
using NLog;

namespace EFIndexMaintenance.EFIndexUpdater
{
    class Program
    {
        private static ILogger _logger;

        private static void Main(string[] args)
        {
            _logger = LogManager.GetCurrentClassLogger();
            _logger.Log(LogLevel.Info, "Версия: {0}", Assembly.GetEntryAssembly().GetName().Version);

            try
            {
                IndexUpdater indexUpdater = new IndexUpdater();
                indexUpdater.DeleteIndexForDroppingColumns(typeof(BloggingContext));

                using (BloggingContext context = new BloggingContext())
                {
                    if (context.Database.CompatibleWithModel(false))
                        _logger.Log(LogLevel.Info, "База данных успешно обновлена");
                    else
                        _logger.Log(LogLevel.Error, "Ошибка обновления базы данных: модель данных не соответствует базе данных");

                    indexUpdater.UpdateIndexes(context);
                }
                var dbMigrator = new DbMigrator(new MigrationsConfiguration());
                var databaseMigrations = dbMigrator.GetDatabaseMigrations().Reverse().ToList();
                if (databaseMigrations.Any())
                {
                    _logger.Log(LogLevel.Info, "Миграции в базе данных:");
                    foreach (var databaseMigration in databaseMigrations)
                    {
                        _logger.Log(LogLevel.Info, databaseMigration);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Fatal, ex);
            }

            Console.ReadLine();
        }
    }
}
