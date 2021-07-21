using LinuxProxyChanger.Models;
using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace LinuxProxyChanger
{
    class Program
    {
        /// <summary>
        /// Settings json file
        /// </summary>
        private static Settings settings;

        /// <summary>
        /// Check if os version is equal to a unix system
        /// </summary>
        private static bool IsLinux
        {
            get
            {
                int p = (int)Environment.OSVersion.Platform;
                return (p == 4) || (p == 6) || (p == 128);
            }
        }

        private static IPStatus status = IPStatus.Unknown;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            settings = JsonConvert.DeserializeObject<Settings>(File.ReadAllText("settings.json"));

            if (!string.IsNullOrEmpty(settings.ProxyIp))
            {
                status = PingTest();
            }

            Clear();
            if (settings.SetProxyOnStartUp)
            {
                if (status == IPStatus.Success)
                {
                    EnableProxy();
                }
                else
                {
                    DisableProxy();
                }
            }

            ConsoleKeyInfo cki;
            do
            {
                cki = Console.ReadKey(true);

                switch (cki.KeyChar)
                {
                    case '1':
                        ToggleNetworkchange();
                        Clear();
                        break;
                    case '2':
                        Clear();
                        EnableProxy();
                        break;
                    case '3':
                        Clear();
                        DisableProxy();
                        break;
                }
            } while (cki.Key != ConsoleKey.Escape);
        }

        /// <summary>
        /// Set default welcome messages
        /// </summary>
        static void Clear()
        {
            Console.Clear();

            if (string.IsNullOrEmpty(settings.ProxyIp))
            {
                WriteColor(@"[//--Warning------------------------------------------------------]", ConsoleColor.Yellow);
                WriteColor($"[// ProxyIp:] Value is not set", ConsoleColor.Yellow);
                WriteColor(@"[//---------------------------------------------------------------]", ConsoleColor.Yellow);
            }

            Console.WriteLine(Environment.NewLine);
            WriteColor(@"[$$$$$$$$\ $$\                       $$\            $$$$$$\                  $$\]", ConsoleColor.DarkGreen);
            WriteColor(@"[$$  _____|\__|                      $$ |          $$  __$$\                 $$ |]", ConsoleColor.DarkGreen);
            WriteColor(@"[$$ |      $$\  $$$$$$\   $$$$$$$\ $$$$$$\         $$ /  \__| $$$$$$\   $$$$$$$ | $$$$$$\   $$$$$$\]", ConsoleColor.DarkGreen);
            WriteColor(@"[$$$$$\    $$ |$$  __$$\ $$  _____|\_$$  _|$$$$$$\ $$ |      $$  __$$\ $$  __$$ |$$  __$$\ $$  __$$\]", ConsoleColor.DarkGreen);
            WriteColor(@"[$$  __|   $$ |$$ |  \__|\$$$$$$\    $$ |  \______|$$ |      $$ /  $$ |$$ /  $$ |$$$$$$$$ |$$ |  \__|]", ConsoleColor.DarkGreen);
            WriteColor(@"[$$ |      $$ |$$ |       \____$$\   $$ |$$\       $$ |  $$\ $$ |  $$ |$$ |  $$ |$$   ____|$$ |]", ConsoleColor.DarkGreen);
            WriteColor(@"[$$ |      $$ |$$ |      $$$$$$$  |  \$$$$  |      \$$$$$$  |\$$$$$$  |\$$$$$$$ |\$$$$$$$\ $$ |]", ConsoleColor.DarkGreen);
            WriteColor(@"[\__|      \__|\__|      \_______/    \____/        \______/  \______/  \_______| \_______|\__|]", ConsoleColor.DarkGreen);
            Console.WriteLine(Environment.NewLine);

            WriteColor(@"[//--Informationen------------------------------------------------]", ConsoleColor.DarkGreen);
            WriteColor($"[// Title:] {Assembly.GetEntryAssembly().GetName().Name}", ConsoleColor.DarkGreen);
            WriteColor($"[// Version:] {Assembly.GetEntryAssembly().GetCustomAttribute<AssemblyFileVersionAttribute>().Version}", ConsoleColor.DarkGreen);
            WriteColor($"[// Autor:] {Assembly.GetEntryAssembly().GetCustomAttribute<AssemblyCopyrightAttribute>().Copyright}", ConsoleColor.DarkGreen);
            WriteColor(@"[//--Settings-----------------------------------------------------]", ConsoleColor.DarkGreen);
            WriteColor($"[// Call on Networkchange:] {settings.CallOnNetworkchange}", ConsoleColor.DarkGreen);
            WriteColor($"[// Set proxy on Autostart:] {settings.SetProxyOnStartUp}", ConsoleColor.DarkGreen);
            WriteColor($"[// Proxy status:] {status}", ConsoleColor.DarkGreen);
            WriteColor(@"[//--Options------------------------------------------------------]", ConsoleColor.DarkGreen);
            WriteColor($"[// 1:] Toggle \"Call on Networkchange\"", ConsoleColor.DarkGreen);
            WriteColor($"[// 2:] Enable proxy", ConsoleColor.DarkGreen);
            WriteColor($"[// 3:] Disable proxy", ConsoleColor.DarkGreen);
            WriteColor($"[// ESC:] Close application", ConsoleColor.DarkGreen);
            WriteColor(@"[//---------------------------------------------------------------]", ConsoleColor.DarkGreen);
            Console.WriteLine(Environment.NewLine);

            if (!IsLinux)
            {
                WriteColor(@"[//--OS is not supported------------------------------------------]", ConsoleColor.DarkRed);
                WriteColor($"[//:] This tool is designed for linux. You can't use it for other platforms", ConsoleColor.DarkRed);
                WriteColor(@"[//---------------------------------------------------------------]", ConsoleColor.DarkRed);
                if (!Debugger.IsAttached)
                {
                    return;
                }
                else
                {
                    Console.WriteLine(Environment.NewLine);
                }
            }

            if (string.IsNullOrEmpty(settings.NetworkChangeAdapters))
            {
                WriteColor(@"[//--No Networkadapters-------------------------------------------]", ConsoleColor.DarkRed);
                WriteColor($"[//:] Please insert Networkadapters (\"NetworkChangeAdapters\") in the settings.json", ConsoleColor.DarkRed);
                WriteColor(@"[//---------------------------------------------------------------]", ConsoleColor.DarkRed);
                if (!Debugger.IsAttached)
                {
                    return;
                }
                else
                {
                    Console.WriteLine(Environment.NewLine);
                }
            }
        }

        /// <summary>
        /// Event to set proxy files if networkadapter rise up
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        static void AddressChangedCallback(object sender, EventArgs e)
        {
            NetworkInterface[] adapters = NetworkInterface.GetAllNetworkInterfaces();

            if(adapters == null) // No networkadapters found
            {
                var networkChangeAdapterList = settings.NetworkChangeAdapters.Split(",");

                foreach (NetworkInterface n in adapters)
                {
                    if (n.OperationalStatus == OperationalStatus.Up && networkChangeAdapterList.Contains(n.Id))
                    {
                        status = PingTest();

                        // Update staus text in console
                        Clear();

                        if (status == IPStatus.Success)
                        {
                            EnableProxy();
                        }
                        else
                        {
                            DisableProxy();
                        }
                    }
                    //Console.WriteLine("   {0} is {1}", n.Name, n.OperationalStatus);
                    //Console.WriteLine("Description is {0} [{1}]", n.Description, n.Id);
                }
            }
        }

        /// <summary>
        /// Ping to the proxy ip to check if the user is in a vpn
        /// </summary>
        /// <returns>Return status of the request</returns>
        static IPStatus PingTest()
        {
            Ping sender = new Ping();
            PingOptions options = new PingOptions();

            options.DontFragment = true;
            string data = "aaaaaaaaaaaaaaaaaaaaaaaaaa";
            byte[] buffer = Encoding.ASCII.GetBytes(data);

            PingReply reply = sender.Send(settings.ProxyIp, settings.Timeout, buffer, options);
            return reply.Status;
        }

        /// <summary>
        /// Toggle the event that will call automaticly on network changes
        /// </summary>
        static void ToggleNetworkchange()
        {
            if (settings.CallOnNetworkchange)
            {
                NetworkChange.NetworkAddressChanged -= new NetworkAddressChangedEventHandler(AddressChangedCallback);
            }
            else
            {
                NetworkChange.NetworkAddressChanged += new NetworkAddressChangedEventHandler(AddressChangedCallback);
            }
            settings.CallOnNetworkchange = !settings.CallOnNetworkchange;
        }

        static void ConfirmProxy(bool isEnable)
        {
            if (!IsLinux)
            {
                return;
            }

            var arg = isEnable ? settings.BashCommandEnable : settings.BashCommandDisable;

            try
            {
                Process proc = new Process();
                proc.StartInfo.FileName = settings.BashPath;
                proc.StartInfo.Arguments = "-c \" " + arg + " \"";
                proc.StartInfo.RedirectStandardError = true;
                proc.StartInfo.UseShellExecute = false;
                proc.Start();

                string err = proc.StandardError.ReadToEnd();
                proc.WaitForExit();

                if (proc.ExitCode != 0)
                {
                    WriteColor($"[// Linux Bash:] Command exit with code {proc.ExitCode}; Error {err}", ConsoleColor.DarkRed);
                }
                else
                {
                    WriteColor($"[// Linux Bash:] Command exit with code {proc.ExitCode}", ConsoleColor.DarkYellow);
                }
            }
            catch(Exception e)
            {
                WriteColor($"[// Exception:] {e.Message}", ConsoleColor.DarkRed);
            }
        }

        #region Enable / Disable Proxy
        /// <summary>
        /// Write all proxy lines to the given files
        /// </summary>
        static void EnableProxy()
        {
            foreach(var file in settings.Files)
            {
                WriteColor(@$"[// ### Enable Proxy for {file.Path}]", ConsoleColor.DarkGreen);
                WriteColor($"[// #] Check file {file.Path} for old entrys", ConsoleColor.DarkGreen);
                if (!RemoveProxyFromFile(file))
                {
                    WriteColor($"[// # Error:] Check file {file.Path} failed", ConsoleColor.DarkRed);
                    return;
                }

                WriteColor($"[// #] Setup file {file.Path}", ConsoleColor.DarkGreen);
                if (!File.Exists(file.Path))
                {
                    WriteColor($"[// # Error:] File path {file.Path} could not be found", ConsoleColor.DarkRed);
                    return;
                }

                File.AppendAllLines(file.Path, new[] { settings.UniquePrefixLine });
                File.AppendAllLines(file.Path, file.Proxy);
                File.AppendAllLines(file.Path, new[] { settings.UniqueSuffixLine });
                WriteColor(@$"[// ### Done.]", ConsoleColor.DarkGreen);
            }
            ConfirmProxy(true);
        }

        /// <summary>
        /// Remove all proxy lines to the given files
        /// </summary>
        static void DisableProxy()
        {
            foreach (var file in settings.Files)
            {
                WriteColor(@$"[// ### Disable Proxy for {file.Path}]", ConsoleColor.DarkGreen);
                RemoveProxyFromFile(file);
                WriteColor(@$"[// ### Done.]", ConsoleColor.DarkGreen);
            }
            ConfirmProxy(false);
        }

        /// <summary>
        /// Delete all automaticly added lines from given file
        /// </summary>
        /// <param name="file">Path of the file</param>
        /// <returns>Success of the action</returns>
        static bool RemoveProxyFromFile(FileSettings file)
        {
            WriteColor($"[// #] Setup file {file.Path}", ConsoleColor.DarkGreen);
            if (!File.Exists(file.Path))
            {
                WriteColor($"[// # Error:] File path {file.Path} could not be found", ConsoleColor.DarkRed);
                return false;
            }

            string tmpFile = Path.GetTempFileName();

            using (StreamWriter sw = new StreamWriter(tmpFile))
            using (StreamReader sr = new StreamReader(file.Path))
            {
                string line;
                bool skipper = false;

                while ((line = sr.ReadLine()) != null)
                {
                    if (line.Equals(settings.UniquePrefixLine))
                    {
                        skipper = true;
                    }
                    if (!skipper)
                    {
                        sw.WriteLine(line);
                    }
                    if (line.Equals(settings.UniqueSuffixLine))
                    {
                        skipper = false;
                    }
                }
            }

            WriteColor($"[// #] Override file {file.Path}", ConsoleColor.DarkGreen);
            var attributes = File.GetAttributes(file.Path);
            File.Delete(file.Path);
            if (File.Exists(file.Path))
            {
                WriteColor($"[// # Error:] File {file.Path} could not be deleted", ConsoleColor.DarkRed);
                return false;
            }
            File.Move(tmpFile, file.Path);

            WriteColor($"[// #] Set file permissions", ConsoleColor.DarkGreen);
            File.SetAttributes(file.Path, attributes);

            return true;
        }
        #endregion

        /// <summary>
        /// Write some coloring console messages for the user
        /// https://stackoverflow.com/questions/2743260/is-it-possible-to-write-to-the-console-in-colour-in-net
        /// </summary>
        /// <param name="message">Message to write</param>
        /// <param name="color">ConsoleColor value of the color</param>
        static void WriteColor(string message, ConsoleColor color)
        {
            var pieces = Regex.Split(message, @"(\[[^\]]*\])");

            for (int i = 0; i < pieces.Length; i++)
            {
                string piece = pieces[i];

                if (piece.StartsWith("[") && piece.EndsWith("]"))
                {
                    Console.ForegroundColor = color;
                    piece = piece.Substring(1, piece.Length - 2);
                }

                Console.Write(piece);
                Console.ResetColor();
            }

            Console.WriteLine();
        }
    }
}
