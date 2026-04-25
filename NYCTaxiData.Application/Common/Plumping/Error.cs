using System;
using System.Collections.Generic;
using System.Text;

namespace NYCTaxiData.Application.Common.Plumping;

public sealed record Error(string Code, string Message)
{
    public static readonly Error None
        = new(string.Empty, string.Empty);

    public static readonly Error NullValue
        = new("Error.NullValue", "The specified result value is null.");

    // ===== Factory Methods =====
    public static Error NotFound(string entity, object id)
        => new($"{entity}.NotFound", $"{entity} with id '{id}' was not found.");

    public static Error Validation(string field, string message)
        => new($"Validation.{field}", message);

    public static Error Conflict(string message)
        => new("Error.Conflict", message);

    public static Error Unauthorized(string message = "Unauthorized access.")
        => new("Error.Unauthorized", message);

    public static Error BusinessRule(string rule, string message)
        => new($"BusinessRule.{rule}", message);

    public override string ToString() => $"[{Code}] {Message}";
}
