using System;

namespace Communicate.Serialization
{
    internal static class InformationSerializer
    {
        public class Information
        {
            public string Name { get; set; }
            public int VersionId { get; set; }
            public int PlatformId { get; set;}
        }

        public static byte[] ToData(object value)
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }
            var connectionInformation = (ConnectionInformation)value;
            var information = new Information();
            information.Name = connectionInformation.Name;
            information.VersionId = connectionInformation.Version.Identifier;
            information.PlatformId = connectionInformation.Platform.Identifier;
            return JsonObjectSerializer<Information>.ToData(information);
        }

        public static object FromData(byte[] data)
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            var information = (Information)JsonSerializer.FromData(data, typeof(Information));

            var connectionInformation = new ConnectionInformation();
            connectionInformation.Name = information.Name;
            connectionInformation.Platform = new Platform(information.PlatformId);
            connectionInformation.Version = new CommunicatorVersion(information.VersionId);
            return connectionInformation;
        }
    }
}
