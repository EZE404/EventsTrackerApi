using EventsTrackerApi.Data;
using EventsTrackerApi.Models;
using EventsTrackerApi.Repositories;
using Microsoft.EntityFrameworkCore;

namespace EventsTrackerApi.Repositories
{
    public class UserRepository(AppDbContext context) : Repository<User>(context), IUserRepository
    {
        public async Task<bool> VerifyNumberStatusAsync(string email, string verificationNumber)
        {
            if (string.IsNullOrWhiteSpace(verificationNumber) || string.IsNullOrWhiteSpace(email))
            {
                return (false);
            }

            var propietario = await context.Users
                                    .FirstOrDefaultAsync(u => u.ResetToken == verificationNumber && u.Email == email);

            if (propietario == null)
            {
                return (false);
            }

            var status = true;
            var now = DateTime.UtcNow;
            if (propietario.ResetTokenExpires.HasValue && propietario.ResetTokenExpires.Value < now)
            {
                status = false;
            }

            return (status);
        }

        public async Task<User> UpdateUserAsync(User userUpdate)
        {
            var existingUser = await _context.Users.FindAsync(userUpdate.ID);
            if (existingUser == null)
                throw new Exception("El usuario no existe.");

            var excludedProps = new[] { "ID", "FechaCreacion" };
            var properties = typeof(User).GetProperties();

            foreach (var prop in properties)
            {
                if (excludedProps.Contains(prop.Name)) continue;

                var newValue = prop.GetValue(userUpdate);
                if (newValue != null)
                {
                    prop.SetValue(existingUser, newValue);
                }
            }
            if (existingUser.FlagUpdateData == 1 && IsProfileComplete(existingUser))
            {
                existingUser.FlagUpdateData = 0;
            }

            existingUser.FechaActualizacion = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return existingUser;
        }

        public Task<User?> GetByEmailAsync(string email)
        {
            //throw new NotImplementedException();
            return _context.Set<User>().FirstOrDefaultAsync(u => u.Email == email);
        }

        public Task<bool> UserExists(int id)
        {
            return _context.Users.AnyAsync(e => e.ID == id);
        }

        public async Task<int> GetLastUserIdAsync()
        {
            //return await _context.Users
            //  .OrderByDescending(u => u.ID)
            //.Select(u => u.ID)
            //.FirstOrDefaultAsync();
            var connection = _context.Database.GetDbConnection();

            if (connection.State != System.Data.ConnectionState.Open)
                await connection.OpenAsync();

            using var command = connection.CreateCommand();
            command.CommandText = @"SELECT AUTO_INCREMENT
                                    FROM information_schema.TABLES
                                    WHERE TABLE_SCHEMA = 'eventstracker'
                                    AND TABLE_NAME = 'Users'";

            var result = await command.ExecuteScalarAsync();
            return Convert.ToInt32(result);
        }

        public async Task<User> ApplyChanges(User existingUser, User userDto)
        {
            // Aplicar solo los campos modificados (si no son null o valores vacíos)
            if (!string.IsNullOrEmpty(userDto.Dni))
                existingUser.Dni = userDto.Dni;

            if (!string.IsNullOrEmpty(userDto.FirstName))
                existingUser.FirstName = userDto.FirstName;

            if (!string.IsNullOrEmpty(userDto.LastName))
                existingUser.LastName = userDto.LastName;

            if (!string.IsNullOrEmpty(userDto.Email))
                existingUser.Email = userDto.Email;

            if (!string.IsNullOrEmpty(userDto.Direccion))
                existingUser.Direccion = userDto.Direccion;

            if (!string.IsNullOrEmpty(userDto.TelefonoArea))
                existingUser.TelefonoArea = userDto.TelefonoArea;

            if (!string.IsNullOrEmpty(userDto.TelefonoNumero))
                existingUser.TelefonoNumero = userDto.TelefonoNumero;

            if (!string.IsNullOrEmpty(userDto.Bio))
                existingUser.Bio = userDto.Bio;

            // Actualizar la fecha de modificación
            existingUser.FechaActualizacion = DateTime.Now;

            return existingUser;
        }

       private static bool IsProfileComplete(User u)
{
    bool dniOk = !string.IsNullOrWhiteSpace(u.Dni);
    bool dirOk = !string.IsNullOrWhiteSpace(u.Direccion);
    bool telOk = !string.IsNullOrWhiteSpace(u.TelefonoArea) &&
                 !string.IsNullOrWhiteSpace(u.TelefonoNumero);

    return dniOk && dirOk && telOk;
}

    }
}
