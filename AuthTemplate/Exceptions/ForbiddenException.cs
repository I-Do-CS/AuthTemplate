namespace AuthTemplate.Exceptions;

public sealed class ForbiddenException : BaseException
{
    public ForbiddenException()
    : base("You're not allowed to perform this operation.") { }

    public ForbiddenException(string message)
        : base(message) { }

    public ForbiddenException(IEnumerable<string> errors)
        : base(errors) { }
}
