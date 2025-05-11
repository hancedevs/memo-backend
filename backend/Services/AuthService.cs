// Services/AuthService.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BCrypt.Net;
using backend.Dto;
using backend.Models;
using backend;
using BCrypt.Net;

public class AuthService
{
    private readonly MemoDbContext _context;
    private readonly IConfiguration _configuration;

    public AuthService(MemoDbContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    public async Task<User> Authenticate(string username, string password)
    {
        var user = await _context.Users.SingleOrDefaultAsync(x => x.Username == username);
        if (user == null || !BCrypt.Net.BCrypt.Verify(password, user.Password))
            return null;

        return user;
    }

    public async Task<Guid> Register(RegisterDto dto)
    {
        if (await _context.Users.AnyAsync(p => p.Email == dto.Email))
            throw new InvalidOperationException("Email already exists.");

        var planner = new User
        {
            Name = dto.Name,
            Email = dto.Email,
            Password = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            Username= dto.Username,
            Role=dto.Role,
        };

        _context.Users.Add(planner);
        await _context.SaveChangesAsync();
        return planner.Id;
    }
}