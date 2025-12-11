namespace AuthTemplate.Exceptions;

public sealed class BadRequestException : BaseException
{
    public BadRequestException()
        : base("There was an error processing your request.") { }

    public BadRequestException(string message)
        : base(message) { }

    public BadRequestException(IEnumerable<string> errors)
        : base(errors) { }
}
