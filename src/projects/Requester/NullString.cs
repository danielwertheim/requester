namespace Requester
{
    public static class NullString
    {
        public static readonly string Value = "<NULL>";

        public static string IfNull(string value)
        {
            return value ?? Value;
        }
    }
}