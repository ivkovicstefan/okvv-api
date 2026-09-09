using System.Globalization;
using OkVolleyVibes.Application.Common.Abstractions;
using OkVolleyVibes.Domain.Identity;
using OkVolleyVibes.Domain.Players;
using OkVolleyVibes.Mediator;

namespace OkVolleyVibes.Application.Authentication.Register;

internal sealed class RegisterHandler(
    IIdentityService identity,
    IAppDbContext db,
    IEmailSender email,
    ITranslator translator,
    IAuthLinkBuilder links,
    IClock clock) : IRequestHandler<RegisterCommand, RegisterResponse>
{
    public async Task<RegisterResponse> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        string culture = CultureInfo.CurrentUICulture.Name;

        Guid userId = await identity.CreateUserAsync(
            new NewUser(
                request.Email,
                request.FirstName.Trim(),
                request.LastName.Trim(),
                request.PhoneNumber.Trim(),
                request.Password,
                culture),
            cancellationToken);

        await identity.AddToRoleAsync(userId, Roles.Player, cancellationToken);

        db.PlayerProfiles.Add(PlayerProfile.Create(userId, clock.UtcNow));
        await db.SaveChangesAsync(cancellationToken);

        string token = await identity.GenerateEmailConfirmationTokenAsync(userId, cancellationToken);
        string link = links.EmailConfirmationLink(userId, token);

        await email.SendAsync(
            new EmailMessage(
                request.Email,
                translator["Email.Verify.Subject"],
                translator.Format("Email.Verify.Body", link)),
            cancellationToken);

        return new RegisterResponse(userId, request.Email);
    }
}
