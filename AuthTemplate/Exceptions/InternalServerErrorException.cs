namespace AuthTemplate.Exceptions;

public sealed class InternalServerErrorException : BaseException
{
    public InternalServerErrorException()
        : base("Internal Server Error.") { }

    public InternalServerErrorException(string message)
        : base(message) { }

    public InternalServerErrorException(IEnumerable<string> errors)
        : base(errors) { }
}
