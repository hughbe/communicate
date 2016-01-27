using System.Globalization;

namespace Communicate
{
    public class CommunicatorVersion : RegisteredObject<CommunicatorVersion>
    {
        internal CommunicatorVersion(int versionNumber, string name = null) : base(versionNumber, name ?? versionNumber.ToString(CultureInfo.InvariantCulture))
        {
        }

        public static CommunicatorVersion CurrentVersion => new CommunicatorVersion(1, "Current Version");
    }
}
