using NYCTaxiData.Infrastructure;

namespace NYCTaxiData.Application.Common.Specifications.Managers;

public sealed class ManagerEmployeeIdExistsSpec : BaseSpecification<Manager>
{
    public ManagerEmployeeIdExistsSpec(string employeeId)
        : base(m => m.Employeeid == employeeId)
    {
    }
}