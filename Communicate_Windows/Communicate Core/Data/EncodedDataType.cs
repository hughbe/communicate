namespace Communicate
{
    public enum EncodedDataType
    {
        Json, //Universally supported
        Xml, //Supported only with the .NET Framework on Windows
        Soap, //Supported only with the .NET Framework on Windows
        Binary, //Suported only with the .NET Framework on Windows
        Keyed //Supported only with Foundation on iOS/OSX
    }
}
