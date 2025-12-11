namespace AuthTemplate.Exceptions;

public sealed class UnprocessableEntityException : BaseException
{
    public UnprocessableEntityException()
        : base("Your request body is missing or unprocessable.") { }

    public UnprocessableEntityException(string message)
        : base(message) { }

    public UnprocessableEntityException(IEnumerable<string> errors)
        : base(errors) { }
}
