namespace AuthTemplate.Exceptions;

public sealed class NotFoundException : BaseException
{
    public NotFoundException()
        : base("Your requested resource was not found.") { }

    public NotFoundException(string message)
        : base(message) { }

    public NotFoundException(IEnumerable<string> errors)
        : base(errors) { }
}
