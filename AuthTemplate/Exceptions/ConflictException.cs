namespace AuthTemplate.Exceptions;

public sealed class ConflictException : BaseException
{
    public ConflictException()
        : base("Your request conflicts with server state.") { }

    public ConflictException(string message)
        : base(message) { }

    public ConflictException(IEnumerable<string> errors)
        : base(errors) { }
}
