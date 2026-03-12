using EventsTrackerApi.Data;
using EventsTrackerApi.DTOs.User;
using EventsTrackerApi.Models;
using EventsTrackerApi.Repositories;
using EventsTrackerApi.Repositories.mappers;
using EventsTrackerApi.Service.Interfaces;
using EventsTrackerApi.Utils;
using Microsoft.EntityFrameworkCore;

namespace EventsTrackerApi.Service;

/// <summary>
/// Implementation of user operations including CRUD, profile management, and avatar handling.
/// </summary>
public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IUserImageRepository _userImageRepository;
    private readonly IPasswordService _passwordService;
    private readonly IEmailSender _emailSender;
    private readonly AppDbContext _dbContext;
    private readonly ILogger<UserService> _logger;

    private const int IsHost = 1;

    public UserService(
        IUserRepository userRepository,
        IUserImageRepository userImageRepository,
        IPasswordService passwordService,
        IEmailSender emailSender,
        AppDbContext dbContext,
        ILogger<UserService> logger)
    {
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        _userImageRepository = userImageRepository ?? throw new ArgumentNullException(nameof(userImageRepository));
        _passwordService = passwordService ?? throw new ArgumentNullException(nameof(passwordService));
        _emailSender = emailSender ?? throw new ArgumentNullException(nameof(emailSender));
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public async Task<IEnumerable<User>> GetAllAsync()
    {
        return await _userRepository.GetAllAsync();
    }

    /// <inheritdoc />
    public async Task<User?> GetByIdAsync(int id)
    {
        return await _userRepository.GetByIdAsync(id);
    }

    /// <inheritdoc />
    public async Task<User?> GetByEmailAsync(string email)
    {
        return await _userRepository.GetByEmailAsync(email);
    }

    /// <inheritdoc />
    public async Task<User> CreateAsync(User user, string? plainPassword = null)
    {
        var password = string.IsNullOrEmpty(user.PasswordHash) 
            ? _passwordService.GeneratePassword(12) 
            : user.PasswordHash;
        
        user.PasswordHash = _passwordService.HashPassword(password);
        user.Dni ??= (await GetNextDniAsync()).ToString();
        user.IsHost = IsHost;
        user.FechaActualizacion = DateTime.UtcNow;

        if (user.FlagUpdateData != 0)
        {
            await _emailSender.SendUserDataChangeAsync(user.Email, user.FirstName, user.Dni, password);
        }

        await _userRepository.AddAsync(user);
        return user;
    }

    /// <inheritdoc />
    public async Task<User> UpdateAsync(int id, UserUpdateDto userUpdate)
    {
        var existingUser = await _userRepository.GetByIdAsync(id);
        if (existingUser == null)
        {
            throw new KeyNotFoundException($"User with ID {id} not found.");
        }

        var updatedUser = UserMapper.MapUpdateDtoToUser(userUpdate, existingUser);
        await _userRepository.ApplyChanges(existingUser, updatedUser);
        await _userRepository.UpdateUserAsync(existingUser);

        return existingUser;
    }

    /// <inheritdoc />
    public async Task<bool> DeleteAsync(int id)
    {
        return await _userRepository.DeleteAsync(id);
    }

    /// <inheritdoc />
    public async Task<string> UploadAvatarAsync(int userId, IFormFile file, CancellationToken ct = default)
    {
        var user = await _userRepository.FirstOrDefaultAsync(u => u.ID == userId, ct);
        if (user == null)
        {
            throw new KeyNotFoundException($"User with ID {userId} not found.");
        }

        string newUrl = await ImageFilesUtils.SaveUserAvatarAsync(file);
        
        if (!string.IsNullOrWhiteSpace(user.AvatarUrl))
        {
            ImageFilesUtils.DeleteImageInBackground(user.AvatarUrl);
        }

        user.AvatarUrl = newUrl;
        await _userRepository.SaveChangesAsync(ct);

        _logger.LogInformation("Avatar uploaded for user {UserId}", userId);
        return newUrl;
    }

    /// <inheritdoc />
    public async Task<int> GetLastUserIdAsync()
    {
        return await _userRepository.GetLastUserIdAsync();
    }

    /// <inheritdoc />
    public async Task<bool> ExistsByEmailAsync(string email)
    {
        var user = await _userRepository.GetByEmailAsync(email);
        return user != null;
    }

    private async Task<int> GetNextDniAsync()
    {
        var connection = _dbContext.Database.GetDbConnection();
        await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandText = "SELECT NEXT VALUE FOR eventstracker.DniSequence";
        var result = await command.ExecuteScalarAsync();

        return Convert.ToInt32(result);
    }
}
