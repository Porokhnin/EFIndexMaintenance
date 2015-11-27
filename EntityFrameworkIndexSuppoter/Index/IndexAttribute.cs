using System;

namespace EntityFrameworkIndexSuppoter.Index
{
    /// <summary>
    /// Атрибут индекса.
    /// Отражает свойства индекса. 
    /// Атрибутом помечается класс.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = true)]
    public class IndexAttribute : Attribute
    {
        /// <summary>
        /// Имя индекса
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Создает уникальный индекс для таблицы или представления.Уникальным является индекс, в котором не может быть двух строк с одним и тем же значением ключа индекса.Кластеризованный индекс представления должен быть уникальным.
        /// </summary>
        public bool IsUnique { get; set; }

        /// <summary>
        /// Создает индекс, в котором логический порядок значений ключа определяет физический порядок соответствующих строк в таблице.На нижнем (конечном) уровне кластеризованного индекса хранятся действительные строки данных таблицы.Для таблицы или представления в каждый момент времени может существовать только один кластеризованный индекс.
        /// </summary>
        public bool IsClustered { get; set; }

        /// <summary>
        /// Создать атрибут
        /// </summary>
        /// <param name="name">Имя индекса</param>
        public IndexAttribute(string name)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentException("Имя индекса не может быть Null Or Empty");

            Name = name;

            IsUnique = false;
            IsClustered = false;
        }
    }
}
