namespace ProductManagement.Application.Interfaces
{
    /// <summary>
    /// Contract for hashing and verifying passwords securely.
    /// </summary>
    public interface IPasswordHasher
    {
        /// <summary>
        /// Hashes a plain-text password.
        /// </summary>
        string HashPassword(string password);

        /// <summary>
        /// Verifies a plain-text password against a stored hash.
        /// </summary>
        bool VerifyPassword(string password, string hashedPassword);
    }
}
