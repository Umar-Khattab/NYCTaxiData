using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NYCTaxiData.Application.Features.Analytics.Commands.UpdateSystemThresholds;
using NYCTaxiData.Application.Features.Analytics.Queries.GetDemandVelocityChart;
using NYCTaxiData.Application.Features.Analytics.Queries.GetSystemThresholds;
using NYCTaxiData.Application.Features.Analytics.Queries.GetTopLevelKpis;
using NYCTaxiData.Application.Common;
using NYCTaxiData.Application.Common.Interfaces.MarkerInterfaces;

namespace NYCTaxiData.API.Controllers
{
    [Authorize(Roles = "Admin,Dispatcher")] // هذه البيانات استراتيجية ويجب حمايتها
    [ApiController]
    [Route("api/v1/[controller]")]
    public class AnalyticsController : ControllerBase
    {
        private readonly ISender _mediator;

        public AnalyticsController(ISender mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// يجلب المؤشرات الرئيسية للداشبورد (عدد السائقين، الإيرادات، وقت الانتظار)
        /// </summary>
        [HttpGet("kpis")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetTopLevelKpis()
        {
            var query = new GetTopLevelKpisQuery();
            var result = await _mediator.Send(query);

            return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
        }

        /// <summary>
        /// يجلب بيانات الرسم البياني لتوقعات الذكاء الاصطناعي لحجم الطلب
        /// </summary>
        [HttpGet("demand-velocity")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetDemandVelocity([FromQuery] GetDemandVelocityChartQuery query)
        {
            // نستخدم FromQuery لأن هذا الطلب سيحمل فلاتر (مثل ZoneId أو DateRange)
            var result = await _mediator.Send(query);

            return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
        }

        /// <summary>
        /// يجلب إعدادات النظام الحالية (مثل مضاعفات الأسعار ونطاق البحث)
        /// </summary>
        [HttpGet("thresholds")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetSystemThresholds()
        {
            var query = new GetSystemThresholdsQuery();
            var result = await _mediator.Send(query);

            return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
        }

        /// <summary>
        /// يحدّث إعدادات النظام (مثل مضاعفات الأسعار ونطاق البحث)
        /// </summary>
        [HttpPut("thresholds")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdateSystemThresholds([FromBody] UpdateSystemThresholdsCommand command)
        {
            var result = await _mediator.Send(command);

            return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
        }
    }
} 