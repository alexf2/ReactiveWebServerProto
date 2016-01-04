using System.Collections.Generic;


namespace AnywayAnyday.GuestBook.Contract
{
    public class DataPage<T>
    {
        public DataPage(int page, int size, int total, IList<T> items)
        {
            PageNumber = page;
            PageSize = size;
            TotalRecordsCount = total;
            Items = items;
        }

        public int PageNumber { get; private set; }
        public int PageSize { get; private set; }
        public int TotalRecordsCount { get; private set; }
        public IList<T> Items { get; private set; }
}
}
