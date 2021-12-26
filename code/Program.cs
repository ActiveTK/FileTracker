
/*
 * ファイルトラッキングツール FileTracker
 * (c) 2021 ActiveTK.
 * Released under the MIT License
 */

using Shell32;
using System;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Threading;

namespace FileTracker
{
    internal class ConsoleMain
    {
        static int Main(string[] args)
        {
            Console.Title = "FileTracker / build 25 Dec, 2021";
            Console.WriteLine("**********************************************************************");
            Console.WriteLine("** " + Console.Title);
            Console.WriteLine("** (c) 2021 ActiveTK. <+activetk.cf>");
            Console.WriteLine("** Released under the MIT License");
            Console.WriteLine("**********************************************************************");
            try
            {
                Thread.CurrentThread.CurrentUICulture = new CultureInfo("en-US");
                Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
            }
            catch { }
            var tracker = new FileTracker();
            if (args.Length == 0)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write("** arg[0] => (string)FilePath > ");
                Console.ResetColor();
                tracker.FilePath = Console.ReadLine();
            }
            else if (args[0].ToLower() == "/explorer" || args[0].ToLower() == "explorer")
            {
                Console.WriteLine("** レジストリを追加しています。。");
                Console.WriteLine("Microsoft.Win32.Registry.SetValue(");
                Console.WriteLine("  @\"HKEY_CLASSES_ROOT\\*\\shell\\FileTrackerでファイルをトラッキングする\\command\",\\");
                Console.WriteLine("  \"\",");
                Console.WriteLine("  \"\\\"\" + Assembly.GetEntryAssembly().Location + \"\\\" \\\"%1\"\\");
                Console.WriteLine(");");
                Microsoft.Win32.Registry.SetValue(
                    @"HKEY_CLASSES_ROOT\*\shell\FileTrackerでファイルをトラッキングする\command", "", "\"" + Assembly.GetEntryAssembly().Location + "\" \"%1\""
                );
                Console.WriteLine("完了しました。");
                Environment.Exit(0);
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write("** arg[0] => (string)FilePath > ");
                Console.ResetColor();
                Console.WriteLine(string.Join(" ", args));
                tracker.FilePath = string.Join(" ", args);
            }
            tracker.StartTracking();
            return 0;
        }
    }
    internal class FileTracker
    {
        public string FilePath = null;
        public int Interval = 500;
        public int NextLineInterval = 10;
        private decimal _count = 0;
        private bool _started = false;
        private float _lastfs = 0;
        public bool StartTracking()
        {
            if (_started == false)
                _started = true;
            else
                return false;
            new Thread(() =>
                {
                    try
                    {
                        if (Console.BufferWidth < 121)
                            Console.BufferWidth = 120;
                    }
                    catch { }
                    bool IsMov = false;
                    string ex = new FileInfo(FilePath).Extension.ToLower();
                    if (ex == ".mp4" || ex == ".mov" || ex == ".avi" || ex == ".wmv")
                        IsMov = true;
                    Console.WriteLine(new string('*', Console.BufferWidth - 1));
                    Console.Write(
                        "* DateTime".PadRight(DateTimeOffset.Now.ToString().Length + 2) + " | " +
                        "FileSize".PadRight(19) + " | " +
                        "Chenges".PadRight(28) + " | " +
                        "LastWriteTime".PadRight(21)
                    );
                    if (IsMov)
                        Console.Write(" | Movie");
                    Console.WriteLine();
                    Console.WriteLine(new string('*', Console.BufferWidth - 1));
                    try
                    {
                        var f = new FileInfo(FilePath);
                        string data = "* " + DateTimeOffset.Now.ToString() + " | ";
                        data += (f.Length + " (" + GetFormatSizeString(f.Length) + ")").PadRight(19) + " | ";
                        data += "+0 (0B), 0B/s".PadRight(28) + " | " + f.LastWriteTime.ToString().PadRight(21);
                        try
                        {
                            if (IsMov)
                                data += " | " + GetMovieDurationText(FilePath);
                        }
                        catch { }
                        Console.WriteLine(data);
                        _lastfs = f.Length;
                    }
                    catch (Exception e)
                    {
                        int start = Console.CursorTop;
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine(e);
                        Console.ResetColor();
                        Console.CursorTop = start;
                    }
                    while (true)
                    {
                        _count++;
                        try
                        {
                            Thread.Sleep(Interval);
                            try
                            {
                                if (Console.BufferWidth < 121)
                                    Console.BufferWidth = 120;
                            }
                            catch { }
                            Console.CursorLeft = 0;
                            Console.Write(new string(' ', Console.BufferWidth - 1));
                            Console.CursorLeft = 0;
                            var fx = new FileInfo(FilePath);
                            string datax = "* " + DateTimeOffset.Now.ToString() + " | ";
                            datax += (fx.Length + " (" + GetFormatSizeString(fx.Length) + ")").PadRight(19) + " | ";
                            try
                            {
                                var Changes = Math.Abs(fx.Length - _lastfs);
                                float Keika = NextLineInterval - (float.Parse(_count.ToString()) % NextLineInterval);
                                datax += (NumberByString(fx.Length - _lastfs) + ", " + GetFormatSizeString(Changes / Keika) + "/s").PadRight(28);
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine(e);
                                datax += "".PadRight(28);
                            }
                            datax += " | ";
                            datax += fx.LastWriteTime.ToString().PadRight(21);
                            try
                            {
                                if (IsMov)
                                    datax += " | " + GetMovieDurationText(FilePath);
                            }
                            catch { }
                            if (_count % NextLineInterval == 0)
                            {
                                Console.WriteLine(datax);
                                _lastfs = fx.Length;
                            }
                            else
                                Console.Write(datax);
                        }
                        catch (Exception e)
                        {
                            int start = Console.CursorTop;
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine(e);
                            Console.ResetColor();
                            Console.CursorTop = start;
                        }
                    }
                }
            ).Start();
            return true;
        }
        public string NumberByString(float num)
        {
            if (num < 0)
                return num.ToString() + " (" + GetFormatSizeString(num) + ")";
            else
                return "+" + num.ToString() + " (" + GetFormatSizeString(num) + ")";
        }
        public string GetFormatSizeString(float size)
        {
            string[] suffix = { "", "K", "M", "G", "T", "P", "E", "Z", "Y" };
            int index = 0;
            while (size >= 1024)
            {
                size /= 1024;
                index++;
            }
            return string.Format("{0}{1}B", size.ToString("#,##0.##"), index < suffix.Length ? suffix[index] : "-");
        }
        [STAThread]
        public string GetMovieDurationText(string FilePath)
        {
            string strFileName = new FileInfo(FilePath).FullName;
            dynamic shell = Activator.CreateInstance(Type.GetTypeFromProgID("Shell.Application"));
            Folder objFolder = shell.NameSpace(Path.GetDirectoryName(strFileName));
            FolderItem folderItem = objFolder.ParseName(Path.GetFileName(strFileName));
            return objFolder.GetDetailsOf(folderItem, 27);
        }
    }
}
