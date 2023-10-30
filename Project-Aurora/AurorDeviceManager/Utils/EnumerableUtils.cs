namespace AurorDeviceManager.Utils;

public static class EnumerableUtils
{
    public static T Next<T>(this IEnumerator<T> enumerator)
    {
        enumerator.MoveNext();
        return enumerator.Current;
    }
}