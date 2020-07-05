using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aurora.Utils
{
    public static class FileUtils
    {
        /// <summary>
        /// Tries to write a given byte array to disk at the given location. Returns true if successfull
        /// </summary>
        /// <param name="path"></param>
        /// <param name="file"></param>
        /// <returns></returns>
        public static bool TryWrite(string path, byte[] file)
        {
            try
            {
                using (BinaryWriter bw = new BinaryWriter(new FileStream(path, FileMode.Create)))
                {
                    bw.Write(file);
                }
                return true;
            }
            catch (Exception e)
            {
                Global.logger.Error($"Error writing file \"{file}\": {e.Message}");
                return false;
            }
        }

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
                Global.logger.Error($"Error deleting file \"{file}\": {e.Message}");
                return false;
            }
        }

        /// <summary>
        /// Tried restoring a previously disabled file by renaming it. Returns true if successful
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public static bool TryEnable(string file)
        {
            //if the original file exists, it's already enabled
            if (File.Exists(file))
                return false;

            var disabled = file + ".disabled";

            //if the disabled file doesnt exist, it can't be enabled
            if (!File.Exists(disabled))
                return false;

            //otherwise, enable it
            try
            {
                File.Move(disabled, file);
                return true;
            }
            catch (Exception e)
            {
                Global.logger.Error($"Error enabling file \"{file}\": {e.Message}");
                return false;
            }
        }

        /// <summary>
        /// Disables a file by appending ".disabled" to its name. Returns true if successful
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public static bool TryDisable(string file)
        {
            //if the file we want to disable doesnt exist, return
            if (!File.Exists(file))
                return false;

            var disabled = file + ".disabled";
            try
            {
                //if the disabled file is already there, delete the old disabled file
                if (File.Exists(disabled))
                    File.Delete(disabled);

                //rename the original file
                File.Move(file, disabled);
                return true;
            }
            catch (Exception e)
            {
                Global.logger.Error($"Error disabling file \"{file}\": {e.Message}");
                return false;
            }
        }
    }
}
