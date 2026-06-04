using MediatR;
using NYCTaxiData.Application.Common.Interfaces;
using NYCTaxiData.Application.Common.Plumbing;
using NYCTaxiData.Domain.Interfaces;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace NYCTaxiData.Application.Features.Trips.Commands.DeleteTrip
{
    public class DeleteTripCommandHandler(IUnitOfWork _unitOfWork, ICurrentUserService _currentUserService)
        : IRequestHandler<DeleteTripCommand, Result<bool>>
    {
        public async Task<Result<bool>> Handle(
            DeleteTripCommand request,
            CancellationToken cancellationToken)
        {
            var trip = await _unitOfWork.Trips.GetByIdAsync(request.TripId);
            if (trip == null)
                return Result<bool>.Failure($"Trip with ID {request.TripId} not found", "NotFound");
             

            await _unitOfWork.Trips.UpdateAsync(trip);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<bool>.Success(true, "Trip soft-deleted successfully");
        }
    }
}
