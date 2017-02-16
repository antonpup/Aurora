using Aurora_SkypeIntegration.Integration;
using Newtonsoft.Json;
using SKYPE4COMLib;
using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Threading;

namespace Aurora_SkypeIntegration
{
    class Program
    {
        private static Skype skype;
        private static bool isRunning;

        private static bool wasUpdated = false;
        private static SkypeIntegration integration_data = new SkypeIntegration();
        private static int missedmessages
        {
            get { return integration_data.MissedMessagesCount; }
            set {
                if(value != integration_data.MissedMessagesCount)
                {
                    integration_data.MissedMessagesCount = value;
                    wasUpdated = true;
                }
            }
        }

        private static bool iscalled
        {
            get { return integration_data.IsCalled; }
            set
            {
                if (value != integration_data.IsCalled)
                {
                    integration_data.IsCalled = value;
                    wasUpdated = true;
                }
            }
        }

        static int Main(string[] args)
        {

            if (Process.GetProcessesByName(Process.GetCurrentProcess().ProcessName).Length > 1)
            {
                //Skype Integration is already running, quitting.
                Environment.Exit(0);
            }

            try
            {
                skype = new Skype();
                skype.Attach(7, false);

                //Mark any previous messages are already read
                foreach (ChatMessage mmsg in skype.MissedMessages)
                {
                    mmsg.Seen = true;
                }

                isRunning = true;

                while (isRunning)
                {
                    if (Process.GetProcessesByName("Aurora").Length == 0)
                        break;

                    missedmessages = skype.MissedMessages.Count;

                    if (skype.ActiveCalls.Count > 0)
                    {
                        bool isCalled = false;

                        foreach(Call call in skype.ActiveCalls)
                        {
                            if (call.Status == TCallStatus.clsRinging && (call.Type == TCallType.cltIncomingP2P || call.Type == TCallType.cltIncomingPSTN) && !isCalled)
                                isCalled = true;
                        }

                        iscalled = isCalled;
                    }
                    else
                        iscalled = false;

                    if(wasUpdated)
                    {
                        string content = JsonConvert.SerializeObject(integration_data, Formatting.None);

                        var httpWebRequest = (HttpWebRequest)WebRequest.Create("http://127.0.0.1:9088/");
                        httpWebRequest.ContentType = "application/json";
                        httpWebRequest.Method = "POST";

                        using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
                        {
                            streamWriter.Write(content);
                            streamWriter.Flush();
                            streamWriter.Close();
                        }

                        var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                        using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                        {
                            var result = streamReader.ReadToEnd();
                        }

                        wasUpdated = false;
                    }

                    Thread.Sleep(500);
                }

            }
            catch (Exception exc)
            {
                return -1;
            }

            return 0;
        }
    }
}
