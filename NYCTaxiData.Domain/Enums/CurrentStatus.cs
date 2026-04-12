using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace NYCTaxiData.Domain.Enums
{
    public enum CurrentStatus
    {
        [EnumMember(Value = "Available")]
        Available,

        [EnumMember(Value = "On_Trip")] // بنأكد عليه ياخد القيمة دي بالظبط
        On_Trip,

        [EnumMember(Value = "Offline")]
        Offline
    }
}
