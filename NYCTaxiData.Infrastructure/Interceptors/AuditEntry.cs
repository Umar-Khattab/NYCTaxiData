using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace NYCTaxiData.Infrastructure.Interceptors
{
    public class AuditEntry
    {
        public string TableName { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public Dictionary<string, object?> OldValues { get; set; } = new();
        public Dictionary<string, object?> NewValues { get; set; } = new();
        public List<string> ChangedColumns { get; set; } = new();

        public string ToJson() => JsonSerializer.Serialize(new
        {
            Table = TableName,
            Action,
            UserId,
            Timestamp,
            OldValues,
            NewValues,
            ChangedColumns
        });
    }
}
