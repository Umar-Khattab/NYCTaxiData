namespace NYCTaxiData.Application.Common.Exceptions
{
    /// <summary>
    /// Exception thrown when there is a conflict in the current state of a resource.
    /// </summary>
    public class ConflictException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ConflictException"/> class.
        /// </summary>
        public ConflictException()
            : base("A conflict occurred with the current state of the resource.")
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConflictException"/> class with a message.
        /// </summary>
        /// <param name="message">The error message.</param>
        public ConflictException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConflictException"/> class with a message and inner exception.
        /// </summary>
        /// <param name="message">The error message.</param>
        /// <param name="innerException">The inner exception.</param>
        public ConflictException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
