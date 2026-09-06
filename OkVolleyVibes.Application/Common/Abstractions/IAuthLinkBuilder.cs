namespace OkVolleyVibes.Application.Common.Abstractions;

/// <summary>Builds the user-facing links embedded in auth emails. Base URL comes from configuration.</summary>
public interface IAuthLinkBuilder
{
    string EmailConfirmationLink(Guid userId, string token);
}
