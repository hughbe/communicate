using System;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace Communicate
{
    [Serializable]
    public class CommunicatorException : Exception
    {
        public CommunicatorException(string message) : base(message)
        {
        }

        public CommunicatorException()
        {

        }

        public CommunicatorException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected CommunicatorException(SerializationInfo serializationInfo, StreamingContext streamingContext) : base(serializationInfo, streamingContext)
        {
            ErrorCode = (CommunicatorErrorCode)serializationInfo.GetInt32(nameof(ErrorCode));
        }

        public CommunicatorException(CommunicatorErrorCode errorCode, Exception innerException) : base("", innerException)
        {
            ErrorCode = errorCode;
        }

        public CommunicatorErrorCode ErrorCode { get; } = CommunicatorErrorCode.None;

        [SecurityPermissionAttribute(SecurityAction.Demand, SerializationFormatter = true)]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
            {
                throw new ArgumentNullException(nameof(info));
            }

            info.AddValue(nameof(ErrorCode), (int)ErrorCode);

            base.GetObjectData(info, context);
        }
    }
}
