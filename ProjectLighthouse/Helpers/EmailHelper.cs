#nullable enable
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using LBPUnion.ProjectLighthouse.Configuration;
using LBPUnion.ProjectLighthouse.Database;
using LBPUnion.ProjectLighthouse.Extensions;
using LBPUnion.ProjectLighthouse.Logging;
using LBPUnion.ProjectLighthouse.Types.Entities.Profile;
using LBPUnion.ProjectLighthouse.Types.Entities.Token;
using LBPUnion.ProjectLighthouse.Types.Logging;
using LBPUnion.ProjectLighthouse.Types.Mail;
using Microsoft.EntityFrameworkCore;

namespace LBPUnion.ProjectLighthouse.Helpers;

public static class SMTPHelper
{
    private static readonly string blacklistFilePath = ServerConfiguration.Instance.EmailEnforcement.BlacklistFilePath;

    // Null check blacklistFilePath and read into array
    private static readonly string[] blacklistFile =
        !string.IsNullOrWhiteSpace(blacklistFilePath) ? File.ReadAllLines(blacklistFilePath) : [];

    // (User id, timestamp of last request + 30 seconds)
    private static readonly ConcurrentDictionary<int, long> recentlySentMail = new();

    private const long emailCooldown = 1000 * 30;

    private static readonly HashSet<string> blacklistedDomains = new(blacklistFile);

    private static bool CanSendMail(UserEntity user)
    {
        // Remove expired entries
        for (int i = recentlySentMail.Count - 1; i >= 0; i--)
        {
            KeyValuePair<int, long> entry = recentlySentMail.ElementAt(i);
            if (recentlySentMail.TryGetValue(entry.Key, out long expiration) &&
                TimeHelper.TimestampMillis > expiration)
            {
                recentlySentMail.TryRemove(entry.Key, out _);
            }
        }

        if (recentlySentMail.TryGetValue(user.UserId, out long userExpiration))
        {
            return TimeHelper.TimestampMillis > userExpiration;
        }
        // If they don't have an entry in the dictionary then they can't be on cooldown
        return true;
    }

    public static async Task SendPasswordResetEmail(DatabaseContext database, IMailService mail, UserEntity user)
    {
        if (!CanSendMail(user)) return;

        if (await database.PasswordResetTokens.CountAsync(t => t.UserId == user.UserId) > 0) return;

        PasswordResetTokenEntity token = new()
        {
            Created = DateTime.UtcNow,
            UserId = user.UserId,
            ResetToken = CryptoHelper.GenerateUrlToken(),
        };

        database.PasswordResetTokens.Add(token);
        await database.SaveChangesAsync();

        string messageBody = $"Hello, {user.Username}.\n\n" +
                             "A request to reset your account's password was issued. If this wasn't you, this can probably be ignored.\n\n" +
                             $"If this was you, your {ServerConfiguration.Instance.Customization.ServerName} password can be reset at the following link:\n" +
                             $"{ServerConfiguration.Instance.ExternalUrl}/passwordReset?token={token.ResetToken}\n\n" + 
                             "This link will expire in 24 hours";

        await mail.SendEmailAsync(user.EmailAddress, $"Project Lighthouse Password Reset Request for {user.Username}", messageBody);

        recentlySentMail.TryAdd(user.UserId, TimeHelper.TimestampMillis + emailCooldown);
    }
    
    // Accumulate checks to determine email validity
    public static bool IsValidEmail(DatabaseContext database, string email)
    {
        // Email should not be empty, should be an actual email, and shouldn't already be used by an account
        if (string.IsNullOrWhiteSpace(email) || !emailValidator.IsValid(email) || EmailIsUsed(database, email).Result)
            return false;

        // Don't even bother if there are no domains in blacklist (AKA file path is empty/invalid, or file itself is empty)
        if (!ServerConfiguration.Instance.EmailEnforcement.EnableEmailBlacklist || blacklistedDomains.Count <= 0) 
            return true;
        
        // Get domain by splitting at '@' character
        string domain = email.Split('@')[1];

        // Return false if domain is found in blacklist
        if (blacklistedDomains.Contains(domain))
        {
            Logger.Info($"Invalid email address {email} submitted by user.", LogArea.Email);
            return false;
        }

        return true;
    }

    // Don't want to allocate every single time we call EmailAddressAttribute.IsValidEmail()
    private static readonly EmailAddressAttribute emailValidator = new();

    // Check if email is already in use by an account
    private static async Task<bool> EmailIsUsed(DatabaseContext database, string email)
    {
        return await database.Users.AnyAsync(u => u.EmailAddress != null && u.EmailAddress.ToLower() == email.ToLower());
    }

    public static void SendRegistrationEmail(IMailService mail, UserEntity user)
    {
        // There is intentionally no cooldown here because this is only used for registration
        // and a user can only be registered once, i.e. this should only be called once per user
        string body = "An account for Project Lighthouse has been registered with this email address.\n\n" +
                      $"You can login at {ServerConfiguration.Instance.ExternalUrl}.";

        mail.SendEmail(user.EmailAddress, "Project Lighthouse Account Created: " + user.Username, body);
    }

    public static async Task<bool> SendVerificationEmail(DatabaseContext database, IMailService mail, UserEntity user)
    {
        if (!CanSendMail(user)) return false;

        string? existingToken = await database.EmailVerificationTokens.Where(v => v.UserId == user.UserId)
            .Select(v => v.EmailToken)
            .FirstOrDefaultAsync();
        if (existingToken != null) await database.EmailVerificationTokens.RemoveWhere(t => t.EmailToken == existingToken);

        EmailVerificationTokenEntity verifyToken = new()
        {
            UserId = user.UserId,
            User = user,
            EmailToken = CryptoHelper.GenerateUrlToken(),
            ExpiresAt = DateTime.UtcNow.AddHours(6),
        };

        database.EmailVerificationTokens.Add(verifyToken);
        await database.SaveChangesAsync();

        string body = "Hello,\n\n" +
                      $"This email is a request to verify this email for your (likely new!) Project Lighthouse account ({user.Username}).\n\n" +
                      $"To verify your account, click the following link: {ServerConfiguration.Instance.ExternalUrl}/verifyEmail?token={verifyToken.EmailToken}\n\n\n" +
                      "If this wasn't you, feel free to ignore this email.";

        bool success = await mail.SendEmailAsync(user.EmailAddress, "Project Lighthouse Email Verification", body);

        // Don't send another email for 30 seconds
        recentlySentMail.TryAdd(user.UserId, TimeHelper.TimestampMillis + emailCooldown);
        return success;
    }
}