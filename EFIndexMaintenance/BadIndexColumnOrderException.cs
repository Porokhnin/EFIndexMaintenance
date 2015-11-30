using System;

namespace EFIndexMaintenance
{
    /// <summary>
    /// Ошибка упарядочивания столбцов индекса
    /// </summary>
    public class BadIndexColumnOrderException : Exception
    {
        public BadIndexColumnOrderException(String message) : base(message)
        {
            
        }

        public BadIndexColumnOrderException(String message, Exception exception) : base(message, exception)
        {
            
        }
    }
}
