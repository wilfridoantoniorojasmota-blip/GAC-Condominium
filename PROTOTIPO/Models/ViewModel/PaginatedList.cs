using System;
using System.Collections.Generic;
using System.Linq;

namespace PROTOTIPO.Models.ViewModel
{
    public class PaginatedList<T>
    {
        public List<T> Items { get; set; }
        public int TotalCount { get; set; }
        public int PageSize { get; set; }
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }

        public PaginatedList(List<T> items, int totalCount, int pageSize, int currentPage)
        {
            Items = items;
            TotalCount = totalCount;
            PageSize = pageSize;
            CurrentPage = currentPage;
            TotalPages = (int)Math.Ceiling((double)totalCount / pageSize);
        }

        public bool HasPreviousPage => CurrentPage > 1;
        public bool HasNextPage => CurrentPage < TotalPages;
        public int PreviousPage => CurrentPage - 1;
        public int NextPage => CurrentPage + 1;
    }
}
