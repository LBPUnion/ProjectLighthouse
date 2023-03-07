#nullable enable
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LBPUnion.ProjectLighthouse.Configuration;
using LBPUnion.ProjectLighthouse.Database;
using LBPUnion.ProjectLighthouse.Extensions;
using LBPUnion.ProjectLighthouse.Types.Entities.Profile;
using LBPUnion.ProjectLighthouse.Types.Entities.Token;
using Microsoft.EntityFrameworkCore;

namespace LBPUnion.ProjectLighthouse.Helpers;

public partial class SMTPHelper
{
    // (User id, timestamp of last request + 30 seconds)
    private static readonly ConcurrentDictionary<int, long> recentlySentEmail = new();

    public static async Task<bool> SendVerificationEmail(DatabaseContext database, UserEntity user)
    {
        // Remove expired entries
        for (int i = recentlySentEmail.Count - 1; i >= 0; i--)
        {
            KeyValuePair<int, long> entry = recentlySentEmail.ElementAt(i);
            bool valueExists = recentlySentEmail.TryGetValue(entry.Key, out long timestamp);
            if (!valueExists)
            {
                recentlySentEmail.TryRemove(entry.Key, out _);
                continue;
            }

            if (TimeHelper.TimestampMillis > timestamp) recentlySentEmail.TryRemove(entry.Key, out _);
        }


        if (recentlySentEmail.ContainsKey(user.UserId))
        {
            bool valueExists = recentlySentEmail.TryGetValue(user.UserId, out long timestamp);
            if (!valueExists)
            {
                recentlySentEmail.TryRemove(user.UserId, out _);
            }
            else if (timestamp > TimeHelper.TimestampMillis)
            {
                return true;
            }
        }

        string? existingToken = await database.EmailVerificationTokens.Where(v => v.UserId == user.UserId)
            .Select(v => v.EmailToken)
            .FirstOrDefaultAsync();
        if (existingToken != null) database.EmailVerificationTokens.RemoveWhere(t => t.EmailToken == existingToken);

        EmailVerificationTokenEntity verifyToken = new()
        {
            UserId = user.UserId,
            User = user,
            EmailToken = CryptoHelper.GenerateAuthToken(),
            ExpiresAt = DateTime.Now.AddHours(6),
        };

        database.EmailVerificationTokens.Add(verifyToken);
        await database.SaveChangesAsync();

        string body = "Hello,\n\n" +
                      $"This email is a request to verify this email for your (likely new!) Project Lighthouse account ({user.Username}).\n\n" +
                      $"To verify your account, click the following link: {ServerConfiguration.Instance.ExternalUrl}/verifyEmail?token={verifyToken.EmailToken}\n\n\n" +
                      "If this wasn't you, feel free to ignore this email.";

        bool success = await SendEmailAsync(user.EmailAddress, "Project Lighthouse Email Verification", body);

        // Don't send another email for 30 seconds
        recentlySentEmail.TryAdd(user.UserId, TimeHelper.TimestampMillis + 30 * 1000);
        return success;
    }
}