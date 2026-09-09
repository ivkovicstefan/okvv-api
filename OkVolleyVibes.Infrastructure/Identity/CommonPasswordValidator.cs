using Microsoft.AspNetCore.Identity;

namespace OkVolleyVibes.Infrastructure.Identity;

/// <summary>Rejects passwords that appear on the embedded common/breached-password blocklist.</summary>
internal sealed class CommonPasswordValidator : IPasswordValidator<User>
{
    private static readonly HashSet<string> Blocklist = Load();

    public Task<IdentityResult> ValidateAsync(UserManager<User> manager, User user, string? password)
    {
        if (password is not null && Blocklist.Contains(password.Trim()))
        {
            return Task.FromResult(IdentityResult.Failed(new IdentityError
            {
                Code = "PasswordIsCommon",
                Description = "This password is too common. Choose something less predictable.",
            }));
        }

        return Task.FromResult(IdentityResult.Success);
    }

    private static HashSet<string> Load()
    {
        var set = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        using Stream? stream = typeof(CommonPasswordValidator).Assembly
            .GetManifestResourceStream("OkVolleyVibes.Infrastructure.Identity.common-passwords.txt");
        if (stream is null)
        {
            return set;
        }

        using var reader = new StreamReader(stream);
        while (reader.ReadLine() is { } line)
        {
            string trimmed = line.Trim();
            if (trimmed.Length > 0 && !trimmed.StartsWith('#'))
            {
                set.Add(trimmed);
            }
        }

        return set;
    }
}
