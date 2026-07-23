using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using ProductManagement.Application.Common;
using ProductManagement.Application.DTOs;
using ProductManagement.Application.Interfaces;
using ProductManagement.Domain.Entities;
using ProductManagement.Domain.Interfaces;

namespace ProductManagement.Application.Features.Identity;

public record RegisterCommand(string Username, string Password, string Role) : IRequest<Result<AuthResponseDto>>;

public record LoginCommand(string Username, string Password) : IRequest<Result<AuthResponseDto>>;

public record RefreshTokenCommand(string RefreshToken) : IRequest<Result<AuthResponseDto>>;

public record LogoutCommand(string RefreshToken) : IRequest<Result>;

public class IdentityCommandHandler :
    IRequestHandler<RegisterCommand, Result<AuthResponseDto>>,
    IRequestHandler<LoginCommand, Result<AuthResponseDto>>,
    IRequestHandler<RefreshTokenCommand, Result<AuthResponseDto>>,
    IRequestHandler<LogoutCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITokenService _tokenService;

    public IdentityCommandHandler(IUnitOfWork unitOfWork, IPasswordHasher passwordHasher, ITokenService tokenService)
    {
        _unitOfWork = unitOfWork;
        _passwordHasher = passwordHasher;
        _tokenService = tokenService;
    }

    public async Task<Result<AuthResponseDto>> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        var existingUser = await _unitOfWork.Users.GetByUsernameAsync(request.Username, cancellationToken);
        if (existingUser != null)
        {
            return Result<AuthResponseDto>.Failure("Username is already taken.", 409);
        }

        var role = request.Role.Trim();
        if (role != "Admin" && role != "User")
        {
            role = "User";
        }

        var user = new User
        {
            Username = request.Username.Trim(),
            PasswordHash = _passwordHasher.HashPassword(request.Password),
            Role = role
        };

        await _unitOfWork.Users.AddAsync(user, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var accessToken = _tokenService.GenerateAccessToken(user, out var expiresOn);
        var refreshToken = _tokenService.GenerateRefreshToken(user.Id);

        user.RefreshTokens.Add(refreshToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var response = new AuthResponseDto
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken.Token,
            Username = user.Username,
            Role = user.Role,
            AccessTokenExpiresOn = expiresOn
        };

        return Result<AuthResponseDto>.Success(response, "User registered successfully.", 201);
    }

    public async Task<Result<AuthResponseDto>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var user = await _unitOfWork.Users.GetByUsernameAsync(request.Username, cancellationToken);
        if (user == null || !_passwordHasher.VerifyPassword(request.Password, user.PasswordHash))
        {
            return Result<AuthResponseDto>.Failure("Invalid username or password.", 401);
        }

        var accessToken = _tokenService.GenerateAccessToken(user, out var expiresOn);
        var refreshToken = _tokenService.GenerateRefreshToken(user.Id);

        user.RefreshTokens.Add(refreshToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var response = new AuthResponseDto
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken.Token,
            Username = user.Username,
            Role = user.Role,
            AccessTokenExpiresOn = expiresOn
        };

        return Result<AuthResponseDto>.Success(response, "Login successful.", 200);
    }

    public async Task<Result<AuthResponseDto>> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        var user = await _unitOfWork.Users.GetUserByRefreshTokenAsync(request.RefreshToken, cancellationToken);
        if (user == null)
        {
            return Result<AuthResponseDto>.Failure("Invalid refresh token.", 401);
        }

        var activeToken = user.RefreshTokens.FirstOrDefault(t => t.Token == request.RefreshToken);
        if (activeToken == null || !activeToken.IsActive)
        {
            return Result<AuthResponseDto>.Failure("Invalid or inactive refresh token.", 401);
        }

        activeToken.RevokedOn = DateTime.UtcNow;

        var newAccessToken = _tokenService.GenerateAccessToken(user, out var expiresOn);
        var newRefreshToken = _tokenService.GenerateRefreshToken(user.Id);

        user.RefreshTokens.Add(newRefreshToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var response = new AuthResponseDto
        {
            AccessToken = newAccessToken,
            RefreshToken = newRefreshToken.Token,
            Username = user.Username,
            Role = user.Role,
            AccessTokenExpiresOn = expiresOn
        };

        return Result<AuthResponseDto>.Success(response, "Token refreshed successfully.", 200);
    }

    public async Task<Result> Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        var user = await _unitOfWork.Users.GetUserByRefreshTokenAsync(request.RefreshToken, cancellationToken);
        if (user == null)
        {
            // Treat as idempotent if token is missing
            return Result.Success("User logged out successfully.", 200);
        }

        var token = user.RefreshTokens.FirstOrDefault(t => t.Token == request.RefreshToken);
        if (token != null && token.IsActive)
        {
            token.RevokedOn = DateTime.UtcNow;
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        return Result.Success("User logged out successfully.", 200);
    }
}

