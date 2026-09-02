namespace OkVolleyVibes.Domain.Common.Exceptions;

/// <summary>
/// The request was well-formed but violates a domain invariant / business rule
/// (e.g. checking in outside your membership period). Maps to <c>422 Unprocessable Entity</c>.
/// Thrown from domain entities and use-case handlers as guard clauses.
/// </summary>
public class BusinessRuleException : AppException
{
    public BusinessRuleException(string message)
        : base("business_rule.violation", message)
    {
    }

    protected BusinessRuleException(string errorCode, string message)
        : base(errorCode, message)
    {
    }
}
