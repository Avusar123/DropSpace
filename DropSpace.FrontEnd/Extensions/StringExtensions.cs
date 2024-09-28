using System.Collections.Generic;
using System.Text.Json;

namespace DropSpace.FrontEnd.Extensions
{
    public static class StringExtensions
    {
        public static Dictionary<string, string[]> ParseErrors(this string message)
        {
            Dictionary<string, string[]> errors = JsonSerializer.Deserialize<Dictionary<string, string[]>>(message)
                                                        ?? throw new JsonException("Ошибка десериализации ошиб");

            return errors;
        }
    }
}
