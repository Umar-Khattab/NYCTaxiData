using System;
using System.Collections.Generic;
using System.Text;

namespace NYCTaxiData.Application.DTOs.Trip
{
    public class LiveDispatchFeedResultDto
    {
        public List<DispatchFeedItemDto> Items { get; set; } = [];
        public int TotalCount { get; set; }
        public DateTime RetrievedAt { get; set; }
    }
}
