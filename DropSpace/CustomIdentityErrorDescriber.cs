using Microsoft.AspNetCore.Identity;

namespace DropSpace
{
    public class CustomIdentityErrorDescriber : IdentityErrorDescriber
    {
        public override IdentityError PasswordMismatch()
        {
            return new IdentityError()
            {
                Code = nameof(PasswordMismatch),
                Description = "Введенные пароли не совпадают"
            };
        }
    }
}
