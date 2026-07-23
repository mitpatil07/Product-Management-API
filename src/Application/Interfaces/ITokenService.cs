using System;
using ProductManagement.Domain.Entities;

namespace ProductManagement.Application.Interfaces
{
    /// <summary>
    /// Contract for generating security tokens (JWT and Refresh Tokens).
    /// </summary>
    public interface ITokenService
    {
        /// <summary>
        /// Generates a JSON Web Token (JWT) access token for the specified user.
        /// </summary>
        /// <param name="user">The user entity.</param>
        /// <param name="expiresOn">Out parameter capturing token expiry time.</param>
        /// <returns>A string representing the JWT.</returns>
        string GenerateAccessToken(User user, out DateTime expiresOn);

        /// <summary>
        /// Generates a cryptographically strong refresh token for the specified user ID.
        /// </summary>
        /// <param name="userId">The unique ID of the user.</param>
        /// <returns>A RefreshToken entity.</returns>
        RefreshToken GenerateRefreshToken(int userId);
    }
}
