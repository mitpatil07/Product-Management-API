namespace ProductManagement.Application.Interfaces
{
    /// <summary>
    /// Decoupled interface to access the current authenticated user details.
    /// </summary>
    public interface ICurrentUserService
    {
        /// <summary>
        /// Gets the username of the current authenticated user.
        /// </summary>
        string? Username { get; }

        /// <summary>
        /// Gets a value indicating whether the current user is in the Admin role.
        /// </summary>
        bool IsAdmin { get; }
    }
}
