namespace Sho8lana.Paging
{
    public class pagination
    {
        public int TotalItems { get;private set; }
        public int PageSize { get; private set; }
        public int CurrentPage { get; private set; }
        public int TotalPages { get; private set; }
        public int StartPage { get; private set; }
        public int EndPage { get; private set; }
        public pagination()
        {

        }
        public pagination(int totalItems,int Page,int pageSize = 10)
        {
            int totalPages = (int)Math.Ceiling((decimal)totalItems/(decimal) pageSize);
            int currentPage = Page;
            int startPage = currentPage-5;
            int endPage= currentPage+5;
            if (startPage <= 0)
            {
                endPage=endPage-(StartPage-1);
                startPage = 1;
            }
            if (endPage > totalPages)
            {
                endPage = totalPages;
                if (endPage > 10)
                {
                    startPage = endPage - 9;
                }
            }
            TotalItems = totalItems;
            CurrentPage = currentPage;
            PageSize= pageSize;
            TotalPages = totalPages;
            StartPage = startPage;
            EndPage = endPage;


        }
    }
}
