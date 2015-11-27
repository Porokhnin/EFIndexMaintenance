using System;

namespace EntityFrameworkIndexSuppoter
{
    /// <summary>
    /// Ошибка одинакового порядка столбца в индексе
    /// </summary>
    public class SameIndexColumnOrderException : Exception
    {
        public SameIndexColumnOrderException(String message) : base(message)
        {
            
        }

        public SameIndexColumnOrderException(String message, Exception exception): base(message,exception)
        {
            
        }
    }
}
