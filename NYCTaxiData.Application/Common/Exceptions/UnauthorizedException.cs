namespace NYCTaxiData.Application.Common.Exceptions
{
    /// <summary>
    /// Exception thrown when a user is not authorized to perform an action.
    /// </summary>
    public class UnauthorizedException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UnauthorizedException"/> class.
        /// </summary>
        public UnauthorizedException()
            : base("You are not authorized to perform this action.")
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UnauthorizedException"/> class with a message.
        /// </summary>
        /// <param name="message">The error message.</param>
        public UnauthorizedException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UnauthorizedException"/> class with a message and inner exception.
        /// </summary>
        /// <param name="message">The error message.</param>
        /// <param name="innerException">The inner exception.</param>
        public UnauthorizedException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
