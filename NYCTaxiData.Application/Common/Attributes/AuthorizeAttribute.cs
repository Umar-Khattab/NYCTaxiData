using NYCTaxiData.Domain.Enums;

namespace NYCTaxiData.Application.Common.Attributes
{
    /// <summary>
    /// Marks a command or query as requiring authorization.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class AuthorizeAttribute : Attribute
    {
        /// <summary>
        /// Gets the list of roles allowed to execute this command/query.
        /// If empty, all authenticated users are allowed.
        /// </summary>
        public UserRole[] Roles { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthorizeAttribute"/> class.
        /// </summary>
        /// <param name="roles">The roles allowed to execute this command/query.</param>
        public AuthorizeAttribute(params UserRole[] roles)
        {
            Roles = roles;
        }
    }
}
