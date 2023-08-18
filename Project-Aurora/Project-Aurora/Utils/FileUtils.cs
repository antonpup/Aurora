using System;
using System.IO;

namespace Aurora.Utils;

public static class FileUtils
{
    /// <summary>
    /// Tries to delete a given file. Returns true if successful
    /// </summary>
    /// <param name="file"></param>
    /// <returns></returns>
    public static bool TryDelete(string file)
    {
        if (!File.Exists(file))
            return false;

        try
        {
            File.Delete(file);
            return true;
        }
        catch (Exception e)
        {
            Global.logger.Error(e, "Error deleting file \\\"{File}\\\"", file);
            return false;
        }
    }
}