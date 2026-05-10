// NYCTaxiData.API/Controllers/AdminController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NYCTaxiData.Domain.Interfaces;
using NYCTaxiData.Infrastructure.Data.Contexts;

namespace NYCTaxiData.API.Controllers;

[ApiController]
[Route("api/v1/admin")]
[Authorize(Roles = "Manager")]
public class AdminController(
    IDailyAggregationService _aggregationService,
    TaxiDbContext _context, // احقنه مباشرة أفضل من الـ scope اليدوي
    ILogger<AdminController> _logger, IWebHostEnvironment _env) : ControllerBase
{

    [HttpPost("aggregate/today")]
    public async Task<IActionResult> AggregateTodayAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("[AdminController] Manual today's aggregation triggered at {Time}", DateTime.UtcNow);

        try
        {
            // تنفيذ العملية
            await _aggregationService.AggregateAsync(DateTime.UtcNow, cancellationToken);

            // لو الكود وصل هنا، يبقى العملية نجحت
            return Ok(new
            {
                Success = true,
                Message = "Aggregation completed successfully",
                Timestamp = DateTime.UtcNow
            });
        }
        catch (OperationCanceledException)
        {
            // في حالة لو اليوزر كنسل الـ Request وهو شغال
            _logger.LogWarning("[AdminController] Aggregation was cancelled by the user.");
            return StatusCode(499, new { Message = "Request was cancelled." });
        }
        catch (DbUpdateException dbEx)
        {
            // مشكلة في الداتابيز (Supabase Connection أو Constraints)
            _logger.LogError(dbEx, "[AdminController] Database error during aggregation.");
            return StatusCode(503, new { Message = "Database is currently unavailable. Please try again later." });
        }
        catch (Exception ex)
        {
            // أي مصيبة تانية غير متوقعة
            _logger.LogError(ex, "[AdminController] Unexpected error during today's aggregation.");

            return StatusCode(500, new
            {
                Success = false,
                Message = "An internal error occurred while calculating statistics.",
                Details = _env.IsDevelopment() ? ex.Message : null // بنظهر التفاصيل بس في الـ Development
            });
        }
    }

    // ✅ تشغيل الـ Aggregation يدوي
    [HttpPost("aggregate/{date}")]
    public async Task<IActionResult> AggregateByDateAsync(
    [FromRoute] DateTime date,
    CancellationToken cancellationToken)
    {
        // 1. Validation: منع التواريخ المستقبلية
        if (date.Date > DateTime.UtcNow.Date)
        {
            return BadRequest(new { Message = "Cannot aggregate future dates. Time travel is not supported yet! 🚀" });
        }

        _logger.LogInformation("[AdminController] Manual backfilling triggered for date: {Date}", date.ToString("yyyy-MM-dd"));

        try
        {
            await _aggregationService.AggregateAsync(date, cancellationToken);

            return Ok(new
            {
                Success = true,
                Message = $"Aggregation for {date:yyyy-MM-dd} completed successfully."
            });
        }
        catch (OperationCanceledException)
        {
            return StatusCode(499, new { Message = "Aggregation process was cancelled by the user." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during manual aggregation for {Date}", date);

            // استخدام الـ _env (لو حقنته) عشان تظهر التفاصيل في الـ Dev فقط
            return StatusCode(500, new
            {
                Message = "An internal error occurred during aggregation.",
                Details = _env.IsDevelopment() ? ex.Message : "Please check server logs."
            });
        }
    }

    // ✅ جيب الإحصائيات مع Handling للـ Dates
    [HttpGet("stats")]
    public async Task<IActionResult> GetStatsAsync(
    [FromQuery] DateTime? from,
    [FromQuery] DateTime? to,
    CancellationToken cancellationToken)
    {
        try
        {
            // 1. تحديد القيم الافتراضية بذكاء
            var fromDate = from?.Date ?? DateTime.UtcNow.Date.AddDays(-30);
            var toDate = to?.Date ?? DateTime.UtcNow.Date;

            // 2. Validation: منطقية التواريخ
            if (fromDate > toDate)
            {
                return BadRequest(new { Message = "The 'from' date cannot be later than the 'to' date." });
            }

            // 3. تحديد حد أقصى للنطاق (مثلاً سنة واحدة) عشان البروفورمانس
            if ((toDate - fromDate).TotalDays > 365)
            {
                return BadRequest(new { Message = "Date range cannot exceed one year for performance reasons." });
            }

            var stats = await _context.DailyStats
                .AsNoTracking()
                .Where(s => s.Date >= fromDate && s.Date <= toDate)
                .OrderByDescending(s => s.Date)
                .ToListAsync(cancellationToken);

            // 4. الرد بـ 200 حتى لو الـ stats فاضية (Empty List)
            return Ok(new
            {
                From = fromDate.ToString("yyyy-MM-dd"),
                To = toDate.ToString("yyyy-MM-dd"),
                Count = stats.Count,
                Stats = stats
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching stats from {From} to {To}", from, to);
            return StatusCode(500, new { Message = "An error occurred while fetching dashboard statistics." });
        }
    }
}