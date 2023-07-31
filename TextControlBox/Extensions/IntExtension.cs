namespace TextControlBoxNS.Extensions
{
    internal static class IntExtension
    {
        public static bool IsInRange(this int value, int start, int count)
        {
            return value >= start && value <= start + count;
        }
    }
}
