using DropSpace.Models.Data;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace DropSpace.DataManagers
{
    public class UsersManager(ApplicationContext context, IPasswordHasher<User> hasher) : IDataManager<User, Guid>
    {

        public async Task<User> CreateAsync(User entity)
        {
            ThrowIfInvalid(entity);

            var user = new User()
            {
                Email = entity.Email,

                PasswordHash = hasher.HashPassword(entity, entity.Password),

                Guid = Guid.NewGuid(),
            };

            await context.AddAsync(user);

            await context.SaveChangesAsync();

            return user;
        }

        public async Task DeleteAsync(Guid key)
        {
            var user = await context.FindAsync<User>(key);

            ArgumentNullException.ThrowIfNull(user);

            context.Remove(user);

            await context.SaveChangesAsync();

        }

        public async Task<User> GetAsync(Guid key)
        {
            return await context.FindAsync<User>(key) 
                ?? throw new NullReferenceException("User not found");
        }

        public async Task UpdateAsync(User entity)
        {
            ThrowIfInvalid(entity);

            context.Update(entity);

            await context.SaveChangesAsync();
        }

        private void ThrowIfInvalid(User entity)
        {
            var validationContext = new ValidationContext(entity);

            List<ValidationResult> results = [];

            if (!Validator.TryValidateObject(entity, validationContext, results))
            {
                throw new ValidationException(results.First().ErrorMessage);
            }
        }
    }
}
