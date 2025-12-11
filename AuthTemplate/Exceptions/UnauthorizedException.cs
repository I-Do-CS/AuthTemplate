namespace AuthTemplate.Exceptions;

public sealed class UnauthorizedException : BaseException
{
    public UnauthorizedException()
        : base("Request made by unauthorized agent. Login or refresh your access token.") { }

    public UnauthorizedException(string message)
        : base(message) { }

    public UnauthorizedException(IEnumerable<string> errors)
        : base(errors) { }
}
