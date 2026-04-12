using NYCTaxiData.Application.Features.Trips.Queries.GetTripHistory;
using System;
using System.Collections.Generic;
using System.Text;
namespace NYCTaxiData.Application.DTOs.Trip
{
    public class TripHistoryResultDto
    {
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public int TotalCount { get; set; }
        public List<Features.Trips.Queries.GetTripHistory.TripHistoryItemDto> Items { get; set; } = [];
    }
}
