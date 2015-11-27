namespace EntityFrameworkIndexSuppoter
{
    /// <summary>
    /// Информаия о свойстве, которое включается в индекс
    /// </summary>
    public class IndexIncludeInfo : IndexColumnInfo
    {
        public override string ToString()
        {
            return string.Format("{0} {1} {2}", IndexName, ColumnInfo.Name, ColumnOrder);
        }
    }
}
