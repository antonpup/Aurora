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
        
        public delegate void FileReadCallback(int lineNumber, string lineValue);
        
        private readonly int READ_DELAY = 3;
        private Thread watcherThread;
        private string filename;
        private ReadMode readMode;
        private FileReadCallback readCallback;
        private long lastFileWrite = 0;
        private bool running;
        
        public FileWatcher(string filename, ReadMode readMode, FileReadCallback readCallback)
        {
            this.filename = filename;
            this.readMode = readMode;
            this.readCallback = readCallback;
        }

        private void TailFile()
        {
            FileStream stream = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            StreamReader reader = new StreamReader(stream, Encoding.UTF8);
            try
            {
                int currentLineNumber = 0;
                if (readMode == ReadMode.TAIL_END)
                {
                    reader.ReadToEnd();
                }
                while  (running)
                {
                    if(readMode == ReadMode.FULL) {
                        long writeTime = File.GetLastWriteTime(filename).Ticks;
                        if (lastFileWrite != writeTime)
                        {
                            stream.Position = 0;
                            reader.DiscardBufferedData();
                            lastFileWrite = writeTime;
                            
                            readCallback(currentLineNumber, reader.ReadToEnd());
                        }
                    }
                    else
                    {
                        string line;
                        while ((line = reader.ReadLine()) != null)
                        {
                            readCallback(currentLineNumber++, line);
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

        public static void ReadFileLines(string filename, FileReadCallback readCallback)
        {
            FileStream stream = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            StreamReader reader = new StreamReader(stream, Encoding.UTF8);
            
            try
            {
                string line;
                int currentLineNumber = 0;
                while ((line = reader.ReadLine()) != null)
                {
                    readCallback(currentLineNumber++, line);
                }
            }
            finally
            {
                reader.Close();
            }
        }

        public bool Start()
        {
            if (!File.Exists(filename))
            {
                return false;
            }
            
            if (watcherThread == null)
            {
                watcherThread = new Thread(TailFile);
                watcherThread.Name = "Elite Dangerous File Watcher";
                running = true;
                watcherThread.Start();
                return true;
            }

            return false;
        }

        public bool Stop()
        {
            if (watcherThread == null) return false;
            running = false;
            watcherThread.Join();
            watcherThread = null;

            return true;

        }
    }
    
}