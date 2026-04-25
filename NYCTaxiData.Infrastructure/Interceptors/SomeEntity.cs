using System;
using System.Collections.Generic;
using System.Text;

namespace NYCTaxiData.Infrastructure.Interceptors
{
    public class SomeEntity : BaseAuditableEntity
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty; 
    }
}
