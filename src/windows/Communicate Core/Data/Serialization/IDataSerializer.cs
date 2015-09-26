using System;

namespace Communicate.Serialization
{
    public interface IDataSerializer
    {
    }

    public interface IDataSerializer<TInput, TOuput, TExtra> : IDataSerializer
    {
        byte[] ToData(TInput toEncode, TExtra extra);
        TOuput FromData(byte[] toDecode, TExtra extra);
    }

    public interface IGenericSerializer : IDataSerializer<object, object, object>
    {
    }

    public interface IObjectSerializer : IDataSerializer<object, object, Type>
    {
    }
}
