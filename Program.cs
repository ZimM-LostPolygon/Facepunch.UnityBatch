using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;

namespace Facepunch.UnityBatch {

    internal class Program {
        private static int Main(string[] args) {
            string unityExePath = args.First();
            string unityExeArguments = String.Join(" ", args.Skip(1).ToArray());
            string projectPath = args.SkipWhile(arg => arg.ToLowerInvariant() != "-projectpath").ElementAt(1);

            return RunUnity(unityExePath, unityExeArguments, projectPath);
        }

        private static int RunUnity(string unityExePath, string unityExeArguments, string projectPath) {
            string logPath = Path.GetTempFileName();
            unityExeArguments += $" -logFile \"{logPath}\"";

            if (!File.Exists(unityExePath)) {
                Console.WriteLine($"File doesn't exist: {unityExePath}");
                return 1;
            }

            Process process = new Process {
                StartInfo = {
                    FileName = unityExePath,
                    //WindowStyle = ProcessWindowStyle.Hidden,
                    WorkingDirectory = projectPath,
                    ErrorDialog = false,
                    Arguments = unityExeArguments
                }
            };

            Console.WriteLine("Unity Executable: " + process.StartInfo.FileName);
            Console.WriteLine("Unity Argument: " + process.StartInfo.Arguments);
            Console.WriteLine("Log File : " + logPath);

            Console.WriteLine();

            process.Start();

            using (FileStream stream = File.Open(logPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)) {
                using (StreamReader reader = new StreamReader(stream)) {
                    while (!process.HasExited) {
                        PrintFromLog(reader);
                        Thread.Sleep(250);
                    }

                    Thread.Sleep(250);
                    PrintFromLog(reader);
                }
            }

            //
            // Try to delete the log file
            //
            Thread.Sleep(1000);
            for (int i = 0; i < 5; i++) {
                try {
                    File.Delete(logPath);
                    break;
                } catch (IOException) {
                    Console.WriteLine($"Couldn't delete {logPath}.. trying again..");
                    Thread.Sleep(1000);
                }
            }

            return process.ExitCode;
        }

        private static void PrintFromLog(StreamReader logStream) {
            string txt = logStream.ReadToEnd();
            if (string.IsNullOrEmpty(txt))
                return;

            Console.Write(txt);
        }
    }
}
