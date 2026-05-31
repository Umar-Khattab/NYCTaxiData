using NYCTaxiData.Domain.Entities;
using NYCTaxiData.Domain.Enums;
using NYCTaxiData.Infrastructure.Services.Specifications;
using System;
using System.Collections.Generic;
using System.Text;

namespace NYCTaxiData.Domain.Specifications.Trips
{
    public class DispatchFeedSpec : BaseSpecification<Driver>
    {
        public DispatchFeedSpec(int limit)
        {
            // 1. الفلترة بناءً على حالة السائق الحالية (موجود ومضمون 100%)
            AddCriteria(d =>
                d.Status == CurrentStatus.Available ||
                d.Status == CurrentStatus.On_Trip);

            // 2. 🚀 الـ Includes السحرية لجلب البيانات الحقيقية من الـ Database ومنع الـ null تماماً
            AddInclude(d => d.User);
            AddInclude(d => d.Trips);

            // 3. ✨ التعديل هنا: الترتيب بناءً على الـ Status لمنع خناقة الـ ID والـ FullName نهائياً
            AddOrderBy(d => d.Status);

            // 4. تطبيق الـ Paging بناءً على الـ Limit المطلوب للـ Dashboard
            ApplyPaging(0, limit);
        }
    }
}