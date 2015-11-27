namespace EntityFrameworkIndexSuppoter
{
    /// <summary>
    /// Информация о столбце
    /// </summary>
    public class ColumnInfo
    {
        /// <summary>
        /// Информация о таблице
        /// </summary>
        public TableInfo TableInfo { get; private set; }

        /// <summary>
        /// Наименование столбца
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Информация о столбце
        /// </summary>
        /// <param name="tableInfo">Информация о таблице</param>
        /// <param name="columnName">Наименование столбца</param>
        public ColumnInfo(TableInfo tableInfo, string columnName)
        {
            TableInfo = tableInfo;
            Name = columnName;
        }

        public override bool Equals(object otherObject)
        {
            var other = otherObject as ColumnInfo;

            if (other == null)
                return false;

            return TableInfo.Equals(other.TableInfo) && Name == other.Name;
        }

        public override int GetHashCode()
        {
            return TableInfo.GetHashCode() ^ Name.GetHashCode();
        }

        public override string ToString()
        {
            return string.Format("{0}.{1}", TableInfo, Name);
        }
    }
}
