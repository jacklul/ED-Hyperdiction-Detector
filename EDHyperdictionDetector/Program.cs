/*
 * Elite: Dangerous Hyperdiction Detector
 * This app will warn you if the jump will end up with Thargoid hyperdiction.
 * 
 * Created by CMDR Jack'lul <jacklul.github.io>
 */

using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace EDHyperdictionDetector
{
    internal class Program
    {
        private static string _path;
        private static string _currentJournalFile;

        public static void Main(string[] args)
        {
            Console.Title = "Elite: Dangerous Hyperdiction Detector by CMDR Jack'lul";

            if (args.Length == 0)
            {
                _path = GetSavedGamesDir();
            }
            else
            {
                _path = args[0];
            }

            if (!File.Exists(_path + "\\Status.json"))
            {
                Console.WriteLine("Couldn't autodetect path to journals, please provide it as an argument!");
                Console.WriteLine("Make sure you started the game at least once.");
                return;
            }

            Console.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " | " + "Monitoring for hyperdictions...");

            while (true)
            {
                _currentJournalFile = GetNewestLogInDirectory(_path);
                if (_currentJournalFile == null)
                {
                    Console.WriteLine("Journal file couldn't be located! Try entering game first!");
                    return;
                }

                Console.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " | " + "Using journal file: " + _currentJournalFile);
                MonitorTailOfFile(_currentJournalFile);
            }
        }

        private static string GetNewestLogInDirectory(string path)
        {
            DirectoryInfo info = new DirectoryInfo(path);
            FileInfo[] files = info.GetFiles().OrderByDescending(p => p.LastWriteTime).ToArray();

            foreach (FileInfo file in files)
            {
                var fileName = file.Name;
                if (fileName.StartsWith("Journal") && fileName.EndsWith(".log"))
                {
                    return file.FullName;
                }
            }

            return null;
        }

        public static void MonitorTailOfFile(string filePath)
        {
            var initialFileSize = new FileInfo(filePath).Length;
            var lastReadLength = initialFileSize - 1024;
            if (lastReadLength < 0) lastReadLength = 0;
            string jumpTarget = "";

            while (true)
            {
                var fileSize = new FileInfo(filePath).Length;
                if (fileSize > lastReadLength)
                {
                    using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                    {
                        fs.Seek(lastReadLength, SeekOrigin.Begin);
                        var buffer = new byte[1024];
                        var text = "";

                        while (true)
                        {
                            var bytesRead = fs.Read(buffer, 0, buffer.Length);
                            lastReadLength += bytesRead;

                            if (bytesRead == 0)
                            {
                                break;
                            }

                            text = text + Encoding.ASCII.GetString(buffer, 0, bytesRead);
                        }

                        string[] lines = text.Split(new[] {"\r\n", "\r", "\n"}, StringSplitOptions.None);
                        foreach (string line in lines)
                        {
                            if (line.Length == 0)
                                continue;

                            try
                            {
                                var res = line.Split(new char[] {','});
                                if (res.Length > 2)
                                {
                                    string[] date = res[0].Split(new string[] {"\":\""}, StringSplitOptions.None);
                                    var dateNow = date[1].Trim(new char[] {'"'});
                                    dateNow = dateNow.Replace("T", " ");
                                    dateNow = dateNow.Replace("Z", "");

                                    if (res[1] == " \"event\":\"StartJump\"")
                                    {
                                        string[] resTarget = res[3].Split(':');
                                        string target = resTarget[1].Trim(new char[] {'"'});

                                        Console.WriteLine(dateNow + " | " + "Jumping: " + target);
                                        jumpTarget = target;
                                    }
                                    else if (res[1] == " \"event\":\"FSDTarget\"")
                                    {
                                        string[] resTarget = res[2].Split(':');
                                        string target = resTarget[1].Trim(new char[] {'"'});

                                        Console.WriteLine(dateNow + " | " + "Target: " + target);

                                        if (jumpTarget == target)
                                        {
                                            Console.WriteLine(dateNow + " | " + "Hyperdiction detected!");
                                            Console.Beep(800, 500);
                                            Console.Beep(800, 500);
                                            Console.Beep(800, 550);
                                        }
                                    }
                                    else if (res[1] == " \"event\":\"FSDJump\"")
                                    {
                                        string[] resTarget = res[2].Split(':');
                                        string target = resTarget[1].Trim(new char[] {'"'});

                                        Console.WriteLine(dateNow + " | " + "Current: " + target);
                                    }
                                    else if (res[1] == " \"event\":\"Music\"" && res[2] == " \"MusicTrack\":\"Unknown_Encounter\" }")
                                    {
                                        Console.WriteLine(dateNow + " | " + "Hyperdiction confirmed!");
                                    }
                                }
                            }
                            catch
                            {
                            }
                        }
                    }
                }

                if (GetNewestLogInDirectory(_path) != _currentJournalFile)
                {
                    Console.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " | " + "Journal change detected, restarting!");
                    break;
                }

                Thread.Sleep(100);
            }
        }

        // Kudos to this CMDR https://forums.frontier.co.uk/showthread.php/289420-How-to-find-the-Saved-Games-directory-(C-)
        private static string GetSavedGamesDir()
        {
            IntPtr path;
            int result = SHGetKnownFolderPath(new Guid("4C5C32FF-BB9D-43B0-B5B4-2D72E54EAAA4"), 0, new IntPtr(0),
                out path);
            if (result >= 0)
            {
                return Marshal.PtrToStringUni(path) + @"\Frontier Developments\Elite Dangerous";
            }
            else
            {
                throw new ExternalException("Failed to find the saved games directory.", result);
            }
        }

        [DllImport("Shell32.dll")]
        private static extern int SHGetKnownFolderPath([MarshalAs(UnmanagedType.LPStruct)] Guid rfid, uint dwFlags, IntPtr hToken, out IntPtr ppszPath);
    }
}