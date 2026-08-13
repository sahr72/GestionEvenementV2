// EventManagement.Domain/Exceptions/DomainException.cs

namespace EventManagement.Domain.Common;

public class DomainException : Exception
{
    public DomainException(string message) : base(message) { }
}