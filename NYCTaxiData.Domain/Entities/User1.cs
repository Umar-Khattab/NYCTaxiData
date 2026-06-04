using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
namespace NYCTaxiData.Domain.Entities;

public partial class User1
{
    public Guid Id { get; set; }

    public string FirstName { get; set; } = null!;

    public string LastName { get; set; } = null!;

    public string PhoneNumber { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public string Role { get; set; } = null!;

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual Driver? Driver { get; set; }

    public virtual Manager? Manager { get; set; }
}
