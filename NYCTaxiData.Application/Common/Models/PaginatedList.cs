using System;
using System.Collections.Generic;
using System.Linq;

namespace NYCTaxiData.Application.Common.Models
{
    public sealed class PaginatedList<T>
    {
        // استخدام IReadOnlyList أفضل للمشاريع الكبيرة لضمان عدم تعديل البيانات بعد جلبها
        public IReadOnlyList<T> Items { get; init; }
        public int PageNumber { get; init; }
        public int PageSize { get; init; }
        public int TotalCount { get; init; }
        public int TotalPages { get; init; }
        public int Offset { get; init; }
        public bool HasPrevious => PageNumber > 1;
        public bool HasNext => PageNumber < TotalPages;

        // Constructor خاص لضمان استخدام الـ Factory Methods فقط
        private PaginatedList(
            IReadOnlyList<T> items,
            int totalCount,
            int pageNumber,
            int pageSize)
        {
            Items = items;
            TotalCount = totalCount;
            PageNumber = pageNumber;
            PageSize = pageSize;
            TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
            Offset = (pageNumber - 1) * pageSize;
        }

        // 1. إنشاء من IEnumerable (بيعمل هو الـ Skip والـ Take)
        public static PaginatedList<T> Create(
            IEnumerable<T> source,
            int pageNumber,
            int pageSize)
        {
            var itemsList = source.ToList();
            var count = itemsList.Count;
            var paged = itemsList
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList()
                .AsReadOnly();

            return new PaginatedList<T>(paged, count, pageNumber, pageSize);
        }

        // 2. إنشاء من بيانات مقسمة جاهزة (الاستخدام الأكثر شيوعاً مع الـ Repositories)
        public static PaginatedList<T> Create(
            IEnumerable<T> pagedItems,
            int totalCount,
            int pageNumber,
            int pageSize)
        {
            return new PaginatedList<T>(pagedItems.ToList().AsReadOnly(), totalCount, pageNumber, pageSize);
        }

        // 3. ميزة الـ Mapping الاحترافية (لتحويل Entity لـ DTO)
        public PaginatedList<TResult> Map<TResult>(Func<T, TResult> mapper)
        {
            var mappedItems = Items.Select(mapper).ToList().AsReadOnly();
            return PaginatedList<TResult>.Create(mappedItems, TotalCount, PageNumber, PageSize);
        }

        // 4. نتيجة فارغة (Safe Empty Result)
        public static PaginatedList<T> Empty(int pageNumber = 1, int pageSize = 10)
        {
            return new PaginatedList<T>(new List<T>().AsReadOnly(), 0, pageNumber, pageSize);
        }
    }
}