namespace Communicate
{
    public class Platform : RegisteredObject<Platform>
    {
        internal Platform(int identifier, string name) : base(identifier, name)
        {
        }

        public static Platform Windows => new Platform(0, "Windows").Register();
        public static Platform Mac => new Platform(100, "Mac").Register();
        public static Platform IPhone => new Platform(200, "iOS").Register();
    }
}
