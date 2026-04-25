using System.Linq;
using NYCTaxiData.Application.Common.Models;

namespace NYCTaxiData.Application.DTOs.Trip;

public class TripHistoryResultDto
{
    public int CurrentPage { get; set; }
    public int TotalPages { get; set; }
    public int TotalCount { get; set; }
    public List<TripHistoryItemDto> Items { get; set; } = new();
    
    // Null-safe factory accepting the paginated result
    public static TripHistoryResultDto FromPaginatedList(PaginatedList<TripHistoryItemDto>? list)
    {
        if (list == null)
            return new TripHistoryResultDto();

        return new TripHistoryResultDto
        {
            CurrentPage = list.PageNumber,
            TotalPages = list.TotalPages,
            TotalCount = list.TotalCount,
            Items = list.Items?.ToList() ?? new List<TripHistoryItemDto>()
        };
    }
}
