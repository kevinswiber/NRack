namespace Rack
{
    public class RackVersion
    {
        public static readonly int[] Version = new[] {1, 1};

        public static string Get()
        {
            return string.Join(".", Version);
        }
    }
}