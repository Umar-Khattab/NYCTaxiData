using NYCTaxiData.Domain.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace NYCTaxiData.Infrastructure;

/// <summary>
/// تم دمج حقول الـ Audit والـ Soft Delete مباشرة لضمان رؤية الـ Controller لها
/// </summary>
public partial class Trip
{
    // --- الحقول الأساسية (الناتجة عن الـ Scaffold) ---
    public int TripId { get; set; }

    public int? SimulationId { get; set; }

    public Guid? DriverId { get; set; }

    public int? PickupLocationId { get; set; }

    public int? DropoffLocationId { get; set; }

    public decimal? ActualFare { get; set; }

    public DateTime? StartedAt { get; set; }

    public DateTime? EndedAt { get; set; }

    // --- حقول الـ Soft Delete (تمت إضافتها يدوياً) ---
    [Column("IsDeleted")] // تأكد من الاسم في Supabase
    public bool IsDeleted { get; set; }

    [Column("DeletedAt")]
    public DateTime? DeletedAt { get; set; }

    [Column("DeletedBy")]
    public string? DeletedBy { get; set; }

    [Column("CreatedAt")]
    public DateTime? CreatedAt { get; set; }

    [Column("CreatedBy")]
    public string? CreatedBy { get; set; }

    [Column("LastUpdatedAt")]
    public DateTime? LastUpdatedAt { get; set; }

    [Column("LastUpdatedBy")]
    public string? LastUpdatedBy { get; set; }

    // --- العلاقات (Navigation Properties) ---
    public virtual Driver? Driver { get; set; }

    public virtual Location? DropoffLocation { get; set; }

    public virtual Location? PickupLocation { get; set; }

    public virtual Simulationrequest? Simulation { get; set; }
}