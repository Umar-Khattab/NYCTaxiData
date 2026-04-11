using NYCTaxiData.Domain.Enums;

namespace NYCTaxiData.Application.Common.Interfaces
{
    /// <summary>
    /// Service to retrieve information about the current authenticated user.
    /// </summary>
    public interface ICurrentUserService
    {
        /// <summary>
        /// Gets the ID of the current user.
        /// </summary>
        Guid? UserId { get; }

        /// <summary>
        /// Gets the role of the current user.
        /// </summary>
        UserRole? UserRole { get; }

        /// <summary>
        /// Gets the phone number of the current user.
        /// </summary>
        string? PhoneNumber { get; }

        /// <summary>
        /// Gets a value indicating whether the current user is authenticated.
        /// </summary>
        bool IsAuthenticated { get; }
    }
}
