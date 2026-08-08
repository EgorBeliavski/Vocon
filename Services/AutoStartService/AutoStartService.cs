using IWshRuntimeLibrary;
using System.Diagnostics;
using System.IO;

namespace Vocon.Services.AutoStartService
{
    public class AutoStartService
    {
        public static string GetAutoStartPath(){
            return Environment.GetFolderPath(Environment.SpecialFolder.Startup);
        }


        public static string GetExePath(){
            if (Environment.ProcessPath == null)
                return null;
            return Environment.ProcessPath;
        }
        public static void CreateLabel()
        {
            if (OperatingSystem.IsWindows())
            {
                string exePath = Process.GetCurrentProcess().MainModule!.FileName;
                string workingDirectory = Path.GetDirectoryName(exePath)!;

                string startupFolder = Environment.GetFolderPath(Environment.SpecialFolder.Startup);
                string shortcutPath = Path.Combine(startupFolder, "Vocon.lnk");

                var shell = new WshShell();
                IWshShortcut label = (IWshShortcut)shell.CreateShortcut(shortcutPath);
                label.TargetPath = exePath;
                label.WorkingDirectory = workingDirectory;
                label.Arguments = "--minimized";
                label.Save();
            }
        }

        public static void DeleteLabel(){
            if (OperatingSystem.IsWindows())
            {
                var exelabelpath = Path.Combine(GetAutoStartPath(), "Vocon.lnk");
                if (System.IO.File.Exists(exelabelpath))
                    System.IO.File.Delete(exelabelpath);
            }
        }

        public static bool IsEnabled(){
            if (OperatingSystem.IsWindows())
            {
                if (System.IO.File.Exists(Path.Combine(GetAutoStartPath(), "Vocon.lnk")))
                {
                    return true;
                }
            }
            return false;

        }
    }
}
