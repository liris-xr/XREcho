using System;

public enum ActionType
{
    POSITION,
    ROTATION,
    POS_AND_ROT,
    CAMERA
}

public enum SpecialChars
{
    EMPTY_VECTOR3
}

public static class EnumExtensions
{

    public static T Next<T>(this T src) where T : struct
    {
        if (!typeof(T).IsEnum) throw new ArgumentException(String.Format("Argument {0} is not an Enum", typeof(T).FullName));

        T[] Arr = (T[])Enum.GetValues(src.GetType());
        int j = Array.IndexOf<T>(Arr, src) + 1;
        return (Arr.Length == j) ? Arr[0] : Arr[j];
    }
}