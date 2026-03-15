using Microsoft.EntityFrameworkCore;
using Qtemplate.Domain.Entities;
using Qtemplate.Domain.Interfaces.Repositories;
using Qtemplate.Infrastructure.Data;

namespace Qtemplate.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly AppDbContext _context;

    public UserRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<User?> GetByIdAsync(Guid id)
        => await _context.Users.FindAsync(id);

    public async Task<User?> GetByEmailAsync(string email)
        => await _context.Users.FirstOrDefaultAsync(u => u.Email == email.ToLower().Trim());
    public async Task<User?> GetActiveByIdAsync(Guid id)
    {
        return await _context.Users
            .Where(x => x.Id == id && x.IsActive)
            .FirstOrDefaultAsync();
    }
    public async Task<User?> GetByResetTokenAsync(string token)
        => await _context.Users.FirstOrDefaultAsync(u => u.ResetPasswordToken == token);

    public async Task<User?> GetByEmailVerifyTokenAsync(string token)
        => await _context.Users.FirstOrDefaultAsync(u => u.EmailVerifyToken == token);

    public async Task<bool> EmailExistsAsync(string email)
        => await _context.Users.AnyAsync(u => u.Email == email.ToLower().Trim());

    public async Task AddAsync(User user)
    {
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(User user)
    {
        _context.Users.Update(user);
        await _context.SaveChangesAsync();
    }
    public async Task<(List<User> Items, int Total)> GetPagedAsync(
    string? search, string? role, bool? isActive, int page, int pageSize)
    {
        var query = _context.Users.AsQueryable();

        if (!string.IsNullOrEmpty(search))
            query = query.Where(u =>
                u.FullName.Contains(search) ||
                u.Email.Contains(search));

        if (!string.IsNullOrEmpty(role))
            query = query.Where(u => u.Role == role);

        if (isActive.HasValue)
            query = query.Where(u => u.IsActive == isActive.Value);

        var total = await query.CountAsync();
        var items = await query
            .OrderByDescending(u => u.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, total);
    }
    public async Task<List<Guid>> GetAllActiveUserIdsAsync()
    => await _context.Users
        .Where(u => u.IsActive)
        .Select(u => u.Id)
        .ToListAsync();
    public async Task AddRefreshTokenAndUpdateUserAsync(User user, RefreshToken token)
    {
        _context.Users.Update(user);
        await _context.RefreshTokens.AddAsync(token);
        await _context.SaveChangesAsync(); // 1 round-trip duy nhất
    }
}