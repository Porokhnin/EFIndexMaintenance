using EntityFrameworkIndexSuppoter.Index;

namespace EntityFrameworkIndexSuppoter
{
    /// <summary>
    /// Информаия о свойстве, по которому строится индекс
    /// </summary>
    public class IndexMemberInfo : IndexColumnInfo
    {
        /// <summary>
        /// Направление сортировки базовых столбцов
        /// </summary>
        public SortDirection SortDirection { get; set; }

        public override string ToString()
        {
            return string.Format("{0} {1} {2} {3}", IndexName, ColumnInfo.Name, SortDirection, ColumnOrder);
        }
    }
}
