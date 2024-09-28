using System.Security.Claims;

namespace DropSpace.FrontEnd.Extensions
{
    public static class ClaimListExtensions
    {
        public static T GetValue<T>(this List<Claim> claims, string type)
        {
            var claim = claims.FirstOrDefault(claim => claim.Type == type)
                            ?? throw new NullReferenceException("Claim не найден!");
            
            var castedType = (T)Convert.ChangeType(claim.Value, typeof(T))
                   ?? throw new InvalidCastException("Невозможно преобразование Claim");

            return castedType;
        }
    }
}
