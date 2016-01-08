using System.Collections.Generic;


namespace AnywayAnyday.GuestBook.Contract
{
    /// <summary>
    /// Represents a page of data.
    /// </summary>
    /// <typeparam name="T">Resultset entity type</typeparam>
    public class DataPage<T>
    {
        public DataPage(int page, int size, int total, IList<T> items)
        {
            PageNumber = page;
            PageSize = size;
            TotalRecordsCount = total;
            Items = items;
        }

        /// <summary>
        /// The number of page starting from 1.
        /// </summary>
        public int PageNumber { get; private set; }
        /// <summary>
        /// Page size in records. Should be > 0. -1 means unlimited size: i.e. all the records are returned.
        /// </summary>
        public int PageSize { get; private set; }
        /// <summary>
        /// Total records, which spans the query.
        /// </summary>
        public int TotalRecordsCount { get; private set; }
        /// <summary>
        /// Resulting page.
        /// </summary>
        public IList<T> Items { get; private set; }
}
}
