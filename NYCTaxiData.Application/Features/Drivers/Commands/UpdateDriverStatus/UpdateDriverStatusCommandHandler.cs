using MediatR;
using NYCTaxiData.Application.Common;
using NYCTaxiData.Domain.Enums;
using NYCTaxiData.Domain.Interfaces;

namespace NYCTaxiData.Application.Features.Drivers.Commands.UpdateDriverStatus
{
    public sealed class UpdateDriverStatusCommandHandler : IRequestHandler<UpdateDriverStatusCommand, Result<Unit>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public UpdateDriverStatusCommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<Unit>> Handle(UpdateDriverStatusCommand request, CancellationToken cancellationToken)
        {
            var driver = await _unitOfWork.Drivers.GetByIdAsync(request.DriverId);
            if (driver is null)
            {
                return Result<Unit>.Failure($"Driver with id '{request.DriverId}' was not found.");
            }

            if (!Enum.TryParse<CurrentStatus>(request.Status, true, out var newStatus))
            {
                return Result<Unit>.Failure("Invalid status value.");
            }

            driver.Status = newStatus;

            await _unitOfWork.Drivers.UpdateAsync(driver);

            return Result<Unit>.Success(Unit.Value);
        }
    }
}
