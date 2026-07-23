using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using ProductManagement.Application.Interfaces;

namespace ProductManagement.API.Services
{
    /// <summary>
    /// Implementation of ICurrentUserService reading details from the active HttpContext User claims principal.
    /// </summary>
    public class CurrentUserService : ICurrentUserService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CurrentUserService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        /// <summary>
        /// Reads the Name claim from the current HttpContext User.
        /// </summary>
        public string? Username => _httpContextAccessor.HttpContext?.User?.Identity?.Name;

        /// <summary>
        /// Evaluates whether the current User is in the Admin role.
        /// </summary>
        public bool IsAdmin => _httpContextAccessor.HttpContext?.User?.IsInRole("Admin") ?? false;
    }
}
