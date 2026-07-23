using System;

namespace ProductManagement.Domain.Entities
{
    /// <summary>
    /// RefreshToken entity issued to a User for maintaining session longevity securely.
    /// </summary>
    public class RefreshToken
    {
        /// <summary>
        /// Unique identifier for the refresh token.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// The secure, cryptographically random token string.
        /// </summary>
        public string Token { get; set; } = string.Empty;

        /// <summary>
        /// Expiry date and time of the refresh token.
        /// </summary>
        public DateTime ExpiresOn { get; set; }

        /// <summary>
        /// Creation date and time of the refresh token.
        /// </summary>
        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Revocation date and time if it was cancelled or refreshed.
        /// </summary>
        public DateTime? RevokedOn { get; set; }

        /// <summary>
        /// True if the token has expired.
        /// </summary>
        public bool IsExpired => DateTime.UtcNow >= ExpiresOn;

        /// <summary>
        /// True if the token has been revoked.
        /// </summary>
        public bool IsRevoked => RevokedOn != null;

        /// <summary>
        /// True if the token is active (neither expired nor revoked).
        /// </summary>
        public bool IsActive => !IsExpired && !IsRevoked;

        /// <summary>
        /// ID of the user who owns this refresh token.
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// User entity navigation property.
        /// </summary>
        public virtual User? User { get; set; }
    }
}
