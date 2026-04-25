namespace NYCTaxiData.Application.Common.Exceptions;

public class NotFoundException : Exception
{
    public NotFoundException()
        : base("The requested resource was not found.") { }

    public NotFoundException(string message)
        : base(message) { }

    public NotFoundException(string message, Exception innerException)
        : base(message, innerException) { }

    // ✅ الجزء الجديد اللي إنت محتاجه
    public NotFoundException(string entity, object id)
        : base($"{entity} with id '{id}' was not found.") { }
}