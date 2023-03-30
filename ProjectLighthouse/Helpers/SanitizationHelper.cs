#nullable enable
using System.ComponentModel.DataAnnotations;

namespace LBPUnion.ProjectLighthouse.Helpers;

public static class SanitizationHelper
{
    public static bool IsValidEmail(string? email) => !string.IsNullOrWhiteSpace(email) && new EmailAddressAttribute().IsValid(email);
}