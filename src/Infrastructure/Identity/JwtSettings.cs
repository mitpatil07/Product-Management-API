namespace ProductManagement.Infrastructure.Security
{
    /// <summary>
    /// Configuration mapping model for JWT settings in appsettings.json.
    /// </summary>
    public class JwtSettings
    {
        public const string SectionName = "JwtSettings";

        public string Secret { get; set; } = string.Empty;
        public string Issuer { get; set; } = string.Empty;
        public string Audience { get; set; } = string.Empty;
        public int ExpiryMinutes { get; set; } = 15;
        public int RefreshTokenExpiryDays { get; set; } = 7;
    }
}
