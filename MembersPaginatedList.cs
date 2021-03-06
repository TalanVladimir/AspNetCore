﻿using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AspNetCore
{
    public class MembersPaginatedList<T> : List<T>
    {
        public int PageIndex { get; private set; }
        public int TotalPages { get; private set; }
        public int LimitPages { get; private set; }

        public MembersPaginatedList(List<T> items, int count, int pageIndex, int pageSize, int pageLimit)
        {
            PageIndex = pageIndex;
            TotalPages = (int)Math.Ceiling(count / (double)pageSize);
            LimitPages = pageLimit;

            this.AddRange(items);
        }

        public bool HasPreviousPage
        {
            get
            {
                return (PageIndex > 1);
            }
        }

        public bool HasNextPage
        {
            get
            {
                return (PageIndex < TotalPages);
            }
        }

        public static async Task<MembersPaginatedList<T>> CreateAsync(IQueryable<T> source, int pageIndex, int pageSize, int pageLimit)
        {
            var count = await source.CountAsync();
            var items = await source.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToListAsync();
            return new MembersPaginatedList<T>(items, count, pageIndex, pageSize, pageLimit);
        }
    }
}
