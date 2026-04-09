using System;
using System.Collections.Generic;

namespace NYCTaxiData.Infrastructure;

public partial class SchemaMigration1
{
    public long Version { get; set; }

    public DateTime? InsertedAt { get; set; }
}
