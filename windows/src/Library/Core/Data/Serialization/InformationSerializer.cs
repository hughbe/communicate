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
            var information = new Information
            {
                Name = connectionInformation.Name,
                VersionId = connectionInformation.Version.Identifier,
                PlatformId = connectionInformation.Platform.Identifier
            };
            return JsonObjectSerializer<Information>.ToData(information);
        }

        public static object FromData(byte[] data)
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            var information = (Information)JsonSerializer.FromData(data, typeof(Information));

            var connectionInformation = new ConnectionInformation
            {
                Name = information.Name,
                Platform = new Platform(information.PlatformId),
                Version = new CommunicatorVersion(information.VersionId)
            };
            return connectionInformation;
        }
    }
}
