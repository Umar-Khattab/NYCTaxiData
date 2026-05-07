using System;
using System.Collections.Generic;
using System.Text;

namespace NYCTaxiData.Application.Common.Exceptions
{
    public class ConcurrencyException : Exception
    {
        public string EntityName { get; }
        public object EntityId { get; }

        public ConcurrencyException(string entityName, object entityId)
            : base($"{entityName} with id '{entityId}' was modified by another user. Please refresh and try again.")
        {
            EntityName = entityName;
            EntityId = entityId;
        }
    }
}
