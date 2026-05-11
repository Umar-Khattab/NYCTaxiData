using System;
using System.Collections.Generic;

namespace NYCTaxiData.Domain.Entities;

public partial class SchemaMigration1
{
    public long Version { get; set; }

    public DateTime? InsertedAt { get; set; }
}
