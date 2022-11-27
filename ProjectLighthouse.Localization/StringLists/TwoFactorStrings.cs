namespace LBPUnion.ProjectLighthouse.Localization.StringLists;

public static class TwoFactorStrings
{
    public static readonly TranslatableString EnableTwoFactor = create("enable_2fa");
    public static readonly TranslatableString DisableTwoFactor = create("disable_2fa");

    public static readonly TranslatableString TwoFactor = create("2fa");
    public static readonly TranslatableString TwoFactorDescription = create("2fa_description");
    public static readonly TranslatableString TwoFactorBackup = create("2fa_backup_description");

    public static readonly TranslatableString TwoFactorRequired = create("2fa_required");

    public static readonly TranslatableString DisableTwoFactorDescription = create("disable_2fa_description");

    public static readonly TranslatableString InvalidCode = create("invalid_code");
    public static readonly TranslatableString InvalidBackupCode = create("invalid_backup");

    public static readonly TranslatableString BackupCodeTitle = create("backup_title");
    public static readonly TranslatableString BackupCodeDescription = create("backup_description");
    public static readonly TranslatableString BackupCodeDescription2 = create("backup_description2");
    public static readonly TranslatableString BackupCodeConfirmation = create("backup_confirmation");
    public static readonly TranslatableString DownloadBackupCodes = create("backup_download");

    public static readonly TranslatableString QrTitle = create("qr_title");
    public static readonly TranslatableString QrDescription = create("qr_description");

    private static TranslatableString create(string key) => new(TranslationAreas.TwoFactor, key);
}