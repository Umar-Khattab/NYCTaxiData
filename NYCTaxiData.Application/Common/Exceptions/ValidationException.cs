namespace NYCTaxiData.Application.Common.Exceptions
{
    /// <summary>
    /// Exception thrown when request validation fails.
    /// </summary>
    public class ValidationException : Exception
    {
        /// <summary>
        /// Gets the validation errors.
        /// </summary>
        public IDictionary<string, string[]> Errors { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ValidationException"/> class.
        /// </summary>
        public ValidationException()
            : base("One or more validation failures have occurred.")
        {
            Errors = new Dictionary<string, string[]>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ValidationException"/> class with validation failures.
        /// </summary>
        /// <param name="failures">The validation failures.</param>
        public ValidationException(IEnumerable<FluentValidation.Results.ValidationFailure> failures)
            : base("One or more validation failures have occurred.")
        {
            Errors = failures
                .GroupBy(e => e.PropertyName, e => e.ErrorMessage)
                .ToDictionary(
                    failureGroup => failureGroup.Key,
                    failureGroup => failureGroup.ToArray());
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ValidationException"/> class with a message.
        /// </summary>
        /// <param name="message">The error message.</param>
        public ValidationException(string message)
            : base(message)
        {
            Errors = new Dictionary<string, string[]>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ValidationException"/> class with a message and inner exception.
        /// </summary>
        /// <param name="message">The error message.</param>
        /// <param name="innerException">The inner exception.</param>
        public ValidationException(string message, Exception innerException)
            : base(message, innerException)
        {
            Errors = new Dictionary<string, string[]>();
        }
    }
}
