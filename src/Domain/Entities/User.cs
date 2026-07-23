using System.Collections.Generic;
using ProductManagement.Domain.Common;

namespace ProductManagement.Domain.Entities
{
    /// <summary>
    /// User entity representing registered system users with basic credentials and roles.
    /// </summary>
    public class User : BaseEntity
    {
        /// <summary>
        /// Unique login name for the user.
        /// </summary>
        public string Username { get; set; } = string.Empty;

        /// <summary>
        /// Securely hashed password.
        /// </summary>
        public string PasswordHash { get; set; } = string.Empty;

        /// <summary>
        /// Security role of the user (e.g. Admin, User).
        /// </summary>
        public string Role { get; set; } = "User";

        /// <summary>
        /// List of refresh tokens issued to this user.
        /// </summary>
        public virtual ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
    }
}
