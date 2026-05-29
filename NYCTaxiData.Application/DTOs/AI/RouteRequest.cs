using System;
using System.ComponentModel.DataAnnotations;

namespace NYCTaxiData.Application.DTOs.AI;

/// <summary>
/// Minimal request parameters sent by the frontend client to query route-based ETA predictions.
/// </summary>
public record RouteRequest(
    [Range(1, 265)] int PickupZoneId,
    [Range(1, 265)] int DropoffZoneId,
    DateTime TargetTime
);
