namespace AuthTemplate.Exceptions;

public abstract class BaseException : Exception
{
    protected BaseException()
        : base() { }

    protected BaseException(string message)
        : base(message) { }

    protected BaseException(IEnumerable<string> errors)
        : base(string.Join(", ", errors)) { }
}
