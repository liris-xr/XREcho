using System.Runtime.InteropServices;

public enum RequestType
{
    CALIBRATE_EYE_TRACKING,
}

[StructLayout(LayoutKind.Sequential)]
public struct Request
{
    public RequestType type;
}