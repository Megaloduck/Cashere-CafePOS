using Microsoft.EntityFrameworkCore;
using CafePOS.API.Data;
using CafePOS.API.Models;
using CafePOS.API.DTOs;

namespace CafePOS.API.Services
{
    public interface IAdminUserService
    {
        Task<List<UserDto>> GetAllUsersAsync();
        Task<UserDto> GetUserAsync(int id);
        Task<UserDto> CreateUserAsync(CreateUserRequest request);
        Task<UserDto> UpdateUserAsync(int id, UpdateUserRequest request);
        Task<DeleteUserResult> DeleteUserAsync(int id, int performedById);
        Task ResetPasswordAsync(int id, string newPassword);
    }

    public class AdminUserService : IAdminUserService
    {
        private readonly CafePOSContext _context;

        public AdminUserService(CafePOSContext context)
        {
            _context = context;
        }

        public async Task<List<UserDto>> GetAllUsersAsync()
        {
            return await _context.Users
                .OrderBy(u => u.Username)
                .Select(u => new UserDto
                {
                    Id = u.Id,
                    Username = u.Username,
                    Email = u.Email,
                    Role = u.Role.ToString(),
                    IsActive = u.IsActive,
                    CreatedAt = u.CreatedAt
                })
                .ToListAsync();
        }

        public async Task<UserDto> GetUserAsync(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
                throw new KeyNotFoundException($"User {id} not found");

            return new UserDto
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                Role = user.Role.ToString(),
                IsActive = user.IsActive,
                CreatedAt = user.CreatedAt
            };
        }

        public async Task<UserDto> CreateUserAsync(CreateUserRequest request)
        {
            // Check if username already exists
            if (await _context.Users.AnyAsync(u => u.Username == request.Username))
                throw new InvalidOperationException("Username already exists");

            // Parse role
            if (!Enum.TryParse<UserRole>(request.Role, out var role))
                throw new InvalidOperationException("Invalid role");

            var user = new User
            {
                Username = request.Username,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                Email = request.Email,
                Role = role,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return new UserDto
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                Role = user.Role.ToString(),
                IsActive = user.IsActive,
                CreatedAt = user.CreatedAt
            };
        }

        public async Task<UserDto> UpdateUserAsync(int id, UpdateUserRequest request)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
                throw new KeyNotFoundException($"User {id} not found");

            // Check if new username conflicts with another user
            if (await _context.Users.AnyAsync(u => u.Username == request.Username && u.Id != id))
                throw new InvalidOperationException("Username already exists");

            // Parse role
            if (!Enum.TryParse<UserRole>(request.Role, out var role))
                throw new InvalidOperationException("Invalid role");

            user.Username = request.Username;
            user.Email = request.Email;
            user.Role = role;
            user.IsActive = request.IsActive;
            user.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return new UserDto
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                Role = user.Role.ToString(),
                IsActive = user.IsActive,
                CreatedAt = user.CreatedAt
            };
        }

        public async Task<DeleteUserResult> DeleteUserAsync(int id, int performedById)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
                throw new KeyNotFoundException($"User {id} not found");

            var performingAdmin = await _context.Users.FindAsync(performedById);
            if (performingAdmin == null || performingAdmin.Role != UserRole.Admin)
                throw new UnauthorizedAccessException("Only admins can delete users");

            // Prevent deleting self
            if (id == performedById)
                throw new InvalidOperationException("You cannot delete your own account");

            var result = new DeleteUserResult { Success = true };

            // If target user is Admin
            if (user.Role == UserRole.Admin)
            {
                var activeAdminCount = await _context.Users.CountAsync(u => u.Role == UserRole.Admin && u.IsActive);

                // Prevent deleting the last active admin
                if (user.IsActive && activeAdminCount <= 1)
                    throw new InvalidOperationException("Cannot delete the last admin user");

                // If the admin is already inactive (soft-deleted), perform hard delete
                if (!user.IsActive)
                {
                    _context.Users.Remove(user);
                    await _context.SaveChangesAsync();
                    result.IsHardDelete = true;
                    result.Message = $"Admin user '{user.Username}' has been permanently deleted";
                    return result;
                }
                else
                {
                    // First deletion: soft delete (mark as inactive)
                    user.IsActive = false;
                    user.UpdatedAt = DateTime.UtcNow;
                    await _context.SaveChangesAsync();
                    result.IsSoftDelete = true;
                    result.Message = $"Admin user '{user.Username}' has been deactivated. Delete again to permanently remove";
                    return result;
                }
            }

            // For non-admin users: soft delete on first attempt
            if (user.IsActive)
            {
                user.IsActive = false;
                user.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
                result.IsSoftDelete = true;
                result.Message = $"User '{user.Username}' has been deactivated. Delete again to permanently remove";
            }
            else
            {
                // If already inactive, hard delete
                _context.Users.Remove(user);
                await _context.SaveChangesAsync();
                result.IsHardDelete = true;
                result.Message = $"User '{user.Username}' has been permanently deleted";
            }

            return result;
        }

        public async Task ResetPasswordAsync(int id, string newPassword)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
                throw new KeyNotFoundException($"User {id} not found");

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
            user.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
        }
    }
}
