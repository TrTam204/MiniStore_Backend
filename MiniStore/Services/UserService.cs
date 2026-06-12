using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;
using MiniStore.Data;
using MiniStore.DTOs.User;
using MiniStore.Models;
using MiniStore.Services.Interfaces;

namespace MiniStore.Services
{
    public class UserService : IUserService
    {
        private readonly AppDbContext _context;
        public UserService(AppDbContext context)
        {
            _context = context;
        }
        public async Task<UserResponseDto> CreateAsync(UserCreateDto dto)
        {
            var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);

            if (existingUser != null)
            {
                throw new Exception("Email already exists");
            }
            var existingPhone = await _context.Users
            .FirstOrDefaultAsync(u => u.Phone == dto.Phone);

            if (existingPhone != null)
            {
                throw new Exception("Phone already exists");
            }
            var user = new User
            {
                FullName = dto.FullName,
                Email = dto.Email,
                Phone = dto.Phone,
                Address = dto.Address,
                PasswordHash = dto.Password,
                Role = string.IsNullOrWhiteSpace(dto.Role) ? "User" : dto.Role
            };

            _context.Users.Add(user);

            await _context.SaveChangesAsync();

            var response = new UserResponseDto
            {
                Id = user.Id,
                FullName= user.FullName,
                Email= user.Email,
                Phone= user.Phone,
                Address= user.Address,
                Role = user.Role,
            };
            return response;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return false;
            }

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<List<UserResponseDto>> GetAllAsync()
        {
            var users = await _context.Users.ToListAsync();
            var response = users.Select(user => new UserResponseDto
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                Phone = user.Phone,
                Address = user.Address,
                Role = user.Role,
            }).ToList();
            return response;
        }
        public async Task<UserResponseDto?> GetByIdAsync(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return null;
            }
            var response = new UserResponseDto
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                Phone = user.Phone,
                Address = user.Address,
                Role = user.Role
            };
            return response;
        }

        public async Task<UserResponseDto?> GetUserByEmailAsync(string email)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower());
            if (user == null) return null;
            return new UserResponseDto
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                Phone = user.Phone,
                Address = user.Address,
                Role = user.Role,
            };
        }

        public async Task<UserResponseDto?> UpdateAsync(int id, UserUpdateDto dto)
        {
            var user = await _context.Users.FindAsync(id);

            if (user == null)
            {
                return null;
            }

            var emailExists = await _context.Users.AnyAsync(x =>
                x.Email == dto.Email && x.Id != id
            );

            if (emailExists)
            {
                throw new Exception("Email already exists");
            }

            var phoneExists = await _context.Users.AnyAsync(x =>
                x.Phone == dto.Phone && x.Id != id
            );

            if (phoneExists)
            {
                throw new Exception("Phone already exists");
            }
            user.FullName = dto.FullName;
            user.Email = dto.Email;
            user.Phone = dto.Phone;
            user.Address = dto.Address;
            user.Role = string.IsNullOrWhiteSpace(dto.Role) ? "User" : dto.Role;
            await _context.SaveChangesAsync();
            var response = new UserResponseDto
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                Phone = user.Phone,
                Address = user.Address,
                Role = user.Role
            };
            return response;
        }
    }
}