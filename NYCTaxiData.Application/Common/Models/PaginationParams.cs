using System;

namespace NYCTaxiData.Application.Common.Models
{
    public record PaginationParams
    {
        private const int DefaultPageSize = 10;
        private const int MaxPageSize = 100;

        // الحقول الخاصة للحفاظ على الـ Validation logic
        private readonly int _pageNumber = 1;
        private readonly int _pageSize = DefaultPageSize;

        public int PageNumber
        {
            get => _pageNumber;
            init => _pageNumber = value < 1 ? 1 : value;
        }

        public int PageSize
        {
            get => _pageSize;
            init => _pageSize = value > MaxPageSize ? MaxPageSize
                               : value < 1 ? DefaultPageSize
                               : value;
        }

        // حساب الـ Offset تلقائياً
        public int Offset => (PageNumber - 1) * PageSize;

        // Factory Method للقيم الافتراضية
        public static PaginationParams Default => new();

        // Factory Method لإنشاء كائن بقيم محددة مع ضمان الـ Validation
        public static PaginationParams Of(int pageNumber, int pageSize)
            => new()
            {
                PageNumber = pageNumber,
                PageSize = pageSize
            };
    }
}