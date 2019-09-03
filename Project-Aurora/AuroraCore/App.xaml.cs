using Aurora.Devices;
using Aurora.Profiles;
using Aurora.Settings;
using IronPython.Hosting;
using Microsoft.Scripting.Hosting;
using Microsoft.Win32;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using SharpDX.RawInput;
using NLog;
using System.Reflection;
using System.Text;
using Aurora.Devices.Layout;
using RazerSdkWrapper;
using RazerSdkWrapper.Utils;
using RazerSdkWrapper.Data;

namespace Aurora
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : System.Windows.Application
    {
        private static readonly Mutex mutex = new Mutex(true, "{C88D62B0-DE49-418E-835D-CE213D58444C}");


        /// <summary>
        /// Output logger for errors, warnings, and information
        /// </summary>
        public static NLog.Logger logger = LogManager.GetLogger("global");

        public static bool isSilent = false;
        private static bool isDelayed = false;
        private static int delayTime = 5000;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            if (mutex.WaitOne(TimeSpan.Zero, true))
            {
#if DEBUG
                isDebug = true;
#endif
                StartLog();

                bool ignore_update = false;
                string arg = "";

                for (int arg_i = 0; arg_i < e.Args.Length; arg_i++)
                {
                    arg = e.Args[arg_i];

                    switch (arg)
                    {
                        case ("-debug"):
                            isDebug = true;
                            logger.Info("Program started in debug mode.");
                            break;
                        case ("-silent"):
                            isSilent = true;
                            logger.Info("Program started with '-silent' parameter");
                            break;
                        case ("-ignore_update"):
                            ignore_update = true;
                            logger.Info("Program started with '-ignore_update' parameter");
                            break;
                        case ("-delay"):
                            isDelayed = true;

                            if (arg_i + 1 < e.Args.Length && int.TryParse(e.Args[arg_i + 1], out delayTime))
                                arg_i++;
                            else
                                delayTime = 5000;

                            logger.Info("Program started with '-delay' parameter with delay of " + delayTime + " ms");

                            break;
                        case ("-install_logitech"):
                            logger.Info("Program started with '-install_logitech' parameter");

                            try
                            {
                                InstallLogitech();
                            }
                            catch (Exception exc)
                            {
                                System.Windows.MessageBox.Show("Could not patch Logitech LED SDK. Error: \r\n\r\n" + exc, "Aurora Error");
                            }

                            Environment.Exit(0);
                            break;
                    }
                }

                AppDomain currentDomain = AppDomain.CurrentDomain;
                if (!isDebug)
                    currentDomain.UnhandledException += CurrentDomain_UnhandledException;

                if (isDelayed)
                    System.Threading.Thread.Sleep((int)delayTime);

                this.ShutdownMode = ShutdownMode.OnExplicitShutdown;
                //AppDomain.CurrentDomain.ProcessExit += new EventHandler(OnProcessExit);

                if (Environment.Is64BitProcess)
                    currentDomain.AppendPrivatePath("x64");
                else
                    currentDomain.AppendPrivatePath("x86");

                if (!Core.Initialize(ignore_update))
                {
                    throw new Exception("Could not initialize the core");
                }
            }
            else
            {
                try
                {
                    NamedPipeClientStream client = new NamedPipeClientStream(".", "aurora\\interface", PipeDirection.Out);
                    client.Connect(30);
                    if (!client.IsConnected)
                        throw new Exception();
                    byte[] command = System.Text.Encoding.ASCII.GetBytes("restore");
                    client.Write(command, 0, command.Length);
                    client.Close();
                }
                catch
                {
                    //Global.logger.LogLine("Aurora is already running.", Logging_Level.Error);
                    System.Windows.MessageBox.Show("Aurora is already running.\r\nExiting.", "Aurora - Error");
                }
            }
        }


        private void StartLog()
        {
            string path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            Directory.SetCurrentDirectory(path);
            string logiDll = Path.Combine(path, "LogitechLed.dll");
            if (File.Exists(logiDll))
                File.Delete(logiDll);
            StringBuilder systeminfo_sb = new StringBuilder(string.Empty);
            systeminfo_sb.Append("\r\n========================================\r\n");

            try
            {
                var win_reg = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion");
                string productName = (string)win_reg.GetValue("ProductName");

                systeminfo_sb.AppendFormat("Operation System: {0}\r\n", productName);
            }
            catch (Exception exc)
            {
                systeminfo_sb.AppendFormat("Operation System: Could not be retrieved. [Exception: {0}]\r\n", exc.Message);
            }

            systeminfo_sb.AppendFormat("System Architecture: " + (Environment.Is64BitOperatingSystem ? "64 bit" : "32 bit") + "\r\n");

            systeminfo_sb.AppendFormat("Environment OS Version: {0}\r\n", Environment.OSVersion);

            systeminfo_sb.AppendFormat("System Directory: {0}\r\n", Environment.SystemDirectory);
            systeminfo_sb.AppendFormat("Executing Directory: {0}\r\n", ExecutingDirectory);
            systeminfo_sb.AppendFormat("Launch Directory: {0}\r\n", Directory.GetCurrentDirectory());
            systeminfo_sb.AppendFormat("Processor Count: {0}\r\n", Environment.ProcessorCount);
            //systeminfo_sb.AppendFormat("User DomainName: {0}\r\n", Environment.UserDomainName);
            //systeminfo_sb.AppendFormat("User Name: {0}\r\n", Environment.UserName);

            systeminfo_sb.AppendFormat("SystemPageSize: {0}\r\n", Environment.SystemPageSize);
            systeminfo_sb.AppendFormat("Environment Version: {0}\r\n", Environment.Version);

            System.Security.Principal.WindowsIdentity identity = System.Security.Principal.WindowsIdentity.GetCurrent();
            System.Security.Principal.WindowsPrincipal principal = new System.Security.Principal.WindowsPrincipal(identity);
            systeminfo_sb.AppendFormat("Is Elevated: {0}\r\n", principal.IsInRole(System.Security.Principal.WindowsBuiltInRole.Administrator));
            systeminfo_sb.AppendFormat("Aurora Assembly Version: {0}\r\n", System.Reflection.Assembly.GetExecutingAssembly().GetName().Version);
            systeminfo_sb.AppendFormat("Aurora File Version: {0}\r\n", System.Diagnostics.FileVersionInfo.GetVersionInfo(System.Reflection.Assembly.GetExecutingAssembly().Location).FileVersion);

            systeminfo_sb.Append("========================================\r\n");

            logger.Info(systeminfo_sb.ToString());
        }

        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);
            Core.Save();
            Core.Dispose();
            LogManager.Shutdown();
            
            Environment.Exit(0);
        }

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Exception exc = (Exception)e.ExceptionObject;
            logger.Fatal("Fatal Exception caught : " + exc);
            logger.Fatal(String.Format("Runtime terminating: {0}", e.IsTerminating));
            LogManager.Flush();

            
            System.Windows.MessageBox.Show("Aurora fatally crashed. Please report the follow to author: \r\n\r\n" + exc, "Aurora has stopped working");
            //Perform exit operations
            System.Windows.Application.Current.Shutdown();
        }

        private void App_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            Exception exc = (Exception)e.Exception;
            logger.Fatal("Fatal Exception caught : " + exc);
            LogManager.Flush();
            if (!isDebug)
                e.Handled = true;
            else
                throw exc;
            System.Windows.MessageBox.Show("Aurora fatally crashed. Please report the follow to author: \r\n\r\n" + exc, "Aurora has stopped working");
            //Perform exit operations
            System.Windows.Application.Current.Shutdown();
        }

        public static void InstallLogitech()
        {
            //Check for Admin
            bool isElevated;
            WindowsIdentity identity = WindowsIdentity.GetCurrent();
            WindowsPrincipal principal = new WindowsPrincipal(identity);
            isElevated = principal.IsInRole(WindowsBuiltInRole.Administrator);

            if (!isElevated)
            {
                logger.Error("Program does not have admin rights");
                System.Windows.MessageBox.Show("Program does not have admin rights");
                Environment.Exit(1);
            }

            //Patch 32-bit
            string logitech_path = (string)Microsoft.Win32.Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Classes\WOW6432Node\CLSID\{a6519e67-7632-4375-afdf-caa889744403}\ServerBinary", null, null);//null gets the default value
            if (logitech_path == null || logitech_path == @"C:\Program Files\LGHUB\sdk_legacy_led_x86.dll")
            {
                logitech_path = @"C:\Program Files\Logitech Gaming Software\SDK\LED\x86\LogitechLed.dll";

                if (!Directory.Exists(Path.GetDirectoryName(logitech_path)))
                    Directory.CreateDirectory(Path.GetDirectoryName(logitech_path));

                RegistryKey key = Registry.LocalMachine.OpenSubKey("SOFTWARE", true);

                key.CreateSubKey("Classes");
                key = key.OpenSubKey("Classes", true);

                key.CreateSubKey("WOW6432Node");
                key = key.OpenSubKey("WOW6432Node", true);

                key.CreateSubKey("CLSID");
                key = key.OpenSubKey("CLSID", true);

                key.CreateSubKey("{a6519e67-7632-4375-afdf-caa889744403}");
                key = key.OpenSubKey("{a6519e67-7632-4375-afdf-caa889744403}", true);

                key.CreateSubKey("ServerBinary");
                key = key.OpenSubKey("ServerBinary", true);

                key.SetValue(null, logitech_path);//null to set the default value
            }

            if (File.Exists(logitech_path) && !File.Exists(logitech_path + ".aurora_backup"))
                File.Move(logitech_path, logitech_path + ".aurora_backup");

            using (BinaryWriter logitech_wrapper_86 = new BinaryWriter(new FileStream(logitech_path, FileMode.Create, FileAccess.Write)))
            {
                logitech_wrapper_86.Write(global::Aurora.Properties.Resources.Aurora_LogiLEDWrapper86);
            }

            //Patch 64-bit
            string logitech_path_64 = (string)Microsoft.Win32.Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Classes\CLSID\{a6519e67-7632-4375-afdf-caa889744403}\ServerBinary", null, null);
            if (logitech_path_64 == null || logitech_path_64 == @"C:\Program Files\LGHUB\sdk_legacy_led_x64.dll")
            {
                logitech_path_64 = @"C:\Program Files\Logitech Gaming Software\SDK\LED\x64\LogitechLed.dll";

                if (!Directory.Exists(Path.GetDirectoryName(logitech_path_64)))
                    Directory.CreateDirectory(Path.GetDirectoryName(logitech_path_64));

                RegistryKey key = Registry.LocalMachine.OpenSubKey("SOFTWARE", true);

                key.CreateSubKey("Classes");
                key = key.OpenSubKey("Classes", true);

                key.CreateSubKey("CLSID");
                key = key.OpenSubKey("CLSID", true);

                key.CreateSubKey("{a6519e67-7632-4375-afdf-caa889744403}");
                key = key.OpenSubKey("{a6519e67-7632-4375-afdf-caa889744403}", true);

                key.CreateSubKey("ServerBinary");
                key = key.OpenSubKey("ServerBinary", true);

                key.SetValue(null, logitech_path_64);
            }

            if (File.Exists(logitech_path_64) && !File.Exists(logitech_path_64 + ".aurora_backup"))
                File.Move(logitech_path_64, logitech_path_64 + ".aurora_backup");

            using (BinaryWriter logitech_wrapper_64 = new BinaryWriter(new FileStream(logitech_path_64, FileMode.Create, FileAccess.Write)))
            {
                logitech_wrapper_64.Write(global::Aurora.Properties.Resources.Aurora_LogiLEDWrapper64);
            }

            logger.Info("Logitech LED SDK patched successfully");
            System.Windows.MessageBox.Show("Logitech LED SDK patched successfully");

            //Environment.Exit(0);
        }
    }
}