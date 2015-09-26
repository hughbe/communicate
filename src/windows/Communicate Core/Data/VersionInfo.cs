namespace Communicate
{
    public class VersionInfo : RegisteredObject<VersionInfo>
    {
        internal VersionInfo(int versionNumber, string name = null) : base(versionNumber, name ?? versionNumber.ToString())
        {
        }

        public static VersionInfo CurrentVersion => new VersionInfo(1, "Current Version");
    }
}
