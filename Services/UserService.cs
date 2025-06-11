using EnFoco_new.Data;
using EnFoco_new.Models;
using Microsoft.EntityFrameworkCore;
using BCrypt.Net;
using Org.BouncyCastle.Crypto.Generators;
using Microsoft.Extensions.Logging;
using Serilog.Core;

namespace EnFoco_new.Services
{
    public class UserService : IUserService
    {
        private readonly EnFocoDb _context;
        private readonly ILogger<UserService> _logger;

        public UserService(EnFocoDb context, ILogger<UserService> logger)
        {
            _context = context;
            _logger = logger;
        }






        public async Task<User?> GetUserByNameAsync(string name)
        {
            _logger.LogInformation("Inicio de GetUserByNameAsync: Buscando usuario por nombre: {UserName}", name);
            try
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Name == name);
                if (user == null)
                {
                    _logger.LogWarning("Fin de GetUserByNameAsync: Usuario '{UserName}' no encontrado.", name);
                }
                else
                {
                    _logger.LogInformation("Fin de GetUserByNameAsync: Usuario '{UserName}' (ID: {UserId}) encontrado.", user.Name, user.Id);
                }
                return user;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en GetUserByNameAsync: Falló al buscar usuario '{UserName}'.", name);
                throw; // Re-lanza la excepción para que el controlador o la capa superior la manejen
            }
        }







        public async Task<bool> VerifyPasswordAsync(User user, string password)
        {
            _logger.LogInformation("Inicio de VerifyPasswordAsync: Verificando contraseña para usuario: {UserName}.", user?.Name);
            if (user == null)
            {
                _logger.LogWarning("Fin de VerifyPasswordAsync: Intento de verificación de contraseña con usuario nulo.");
                return false;
            }

            try
            {
                // NO loguear la contraseña real por motivos de seguridad
                bool isPasswordValid = BCrypt.Net.BCrypt.Verify(password, user.PasswordHash);
                if (isPasswordValid)
                {
                    _logger.LogInformation("Fin de VerifyPasswordAsync: Contraseña verificada exitosamente para usuario '{UserName}' (ID: {UserId}).", user.Name, user.Id);
                }
                else
                {
                    _logger.LogWarning("Fin de VerifyPasswordAsync: Fallo de verificación de contraseña para usuario '{UserName}' (ID: {UserId}).", user.Name, user.Id);
                }
                return isPasswordValid;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en VerifyPasswordAsync: Falló la verificación de contraseña para usuario '{UserName}' (ID: {UserId}).", user.Name, user.Id);
                throw;
            }
        }







        public async Task<User?> CreateUserAsync(string name, string password)
        {
            _logger.LogInformation("Inicio de CreateUserAsync: Creando nuevo usuario con nombre: {UserName}.", name);
            try
            {
                // Verifica si el usuario ya existe para evitar duplicados y lanzar un error más claro
                var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Name == name);
                if (existingUser != null)
                {
                    _logger.LogWarning("Fin de CreateUserAsync: Fallo al crear usuario. Ya existe un usuario con el nombre '{UserName}'.", name);
                    // Opcionalmente, lanzar una excepción específica o devolver un nulo
                    throw new InvalidOperationException($"Ya existe un usuario con el nombre '{name}'.");
                }

                // NO loguear la contraseña real por motivos de seguridad
                string passwordHash = BCrypt.Net.BCrypt.HashPassword(password);

                var newUser = new User
                {
                    Name = name,
                    PasswordHash = passwordHash,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Users.Add(newUser);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Fin de CreateUserAsync: Usuario '{UserName}' (ID: {UserId}) creado exitosamente.", newUser.Name, newUser.Id);
                return newUser;
            }
            catch (InvalidOperationException ex) // Para cuando el usuario ya existe
            {
                _logger.LogWarning(ex, "Fin de CreateUserAsync: Fallo de operación al crear usuario '{UserName}'. Mensaje: {ErrorMessage}", name, ex.Message);
                throw; // Re-lanza la excepción para ser manejada por la capa superior
            }
            catch (DbUpdateException ex) // Para errores de base de datos
            {
                _logger.LogError(ex, "Error en CreateUserAsync: Falló al guardar el nuevo usuario '{UserName}' en la base de datos.", name);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado en CreateUserAsync: Falló al crear el usuario '{UserName}'.", name);
                throw;
            }
        }
    }
}