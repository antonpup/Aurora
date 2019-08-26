using System;
using System.IO;
using System.Text;
using System.Threading;
using IronPython.Modules;

namespace Aurora.Profiles.EliteDangerous.Helpers
{
    public class FileWatcher
    {
        public enum ReadMode {
            TAIL, TAIL_END, FULL
        };
        
        public interface FileReadCallback {
            void OnRead(int lineNumber, string lineValue);
        }

        private readonly int READ_DELAY = 3;
        private Thread watcherThread;
        private string filename;
        private ReadMode readMode;
        private FileReadCallback readCallback;
        private long lastFileWrite = 0;
        
        public FileWatcher(string filename, ReadMode readMode, FileReadCallback readCallback)
        {
            this.filename = filename;
            this.readMode = readMode;
            this.readCallback = readCallback;
        }

        private void ReadFile()
        {
            FileStream stream = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            StreamReader reader = new StreamReader(stream, Encoding.UTF8);
            try
            {
                if (readMode == ReadMode.TAIL_END)
                {
                    reader.ReadToEnd();
                }
                while (true)
                {
                    if(readMode == ReadMode.FULL) {
                        long writeTime = File.GetLastWriteTime(filename).Ticks;
                        if (lastFileWrite != writeTime)
                        {
                            stream.Position = 0;
                            reader.DiscardBufferedData();
                            lastFileWrite = writeTime;
                            
                            readCallback.OnRead(0, reader.ReadToEnd());
                        }
                    }
                    else
                    {
                        string line;
                        while ((line = reader.ReadLine()) != null)
                        {
                            readCallback.OnRead(0, line);
                        }
                    }

                    Thread.Sleep(READ_DELAY);
                }
            }
            finally
            {
                reader.Close();
            }
        }

        public bool Start()
        {
            if (watcherThread == null)
            {
                watcherThread = new Thread(ReadFile);
                watcherThread.Start();
                return true;
            }

            return false;
        }

        public bool Stop()
        {
            if (watcherThread != null)
            {
                watcherThread.Abort();
                watcherThread.Join();
                watcherThread = null;

                return true;
            }

            return false;
        }
    }
    
}