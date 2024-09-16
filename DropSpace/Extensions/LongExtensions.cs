namespace DropSpace.Extensions
{
    public static class LongExtensions
    {
        public static double ToMBytes(this long value)
        {
            return Math.Round((double)value / (1024 * 1024), 2);
        }
    }
}
