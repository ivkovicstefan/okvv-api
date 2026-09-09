using OkVolleyVibes.Domain.Common.Exceptions;

namespace OkVolleyVibes.Domain.Identity;

/// <summary>Registration or email-change was attempted with an address that already has an account.</summary>
public sealed class EmailAlreadyInUseException()
    : ConflictException("account.email_in_use", "An account with this email address already exists.");
