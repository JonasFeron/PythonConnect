using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.IO;
using Muscle.PythonLink;

namespace MyMainCSharpApp
{
    internal class Program
    {

        public const string ActivateCondaBat = @"C:\Users\Jonas\anaconda3\Scripts\activate.bat"; // OverWrite your path to the activate.bat file of your anaconda environment
        public static string PythonProjectDirectory { get; } = Path.Combine(new DirectoryInfo(Directory.GetCurrentDirectory()).FullName, "MyPythonProject");


        private static void Main()
        {
            log.Info("Hello logging world!");

            var stopWatch = new Stopwatch();

            // Use the new PythonManager inside a using, so at the end of the using, the communication with the cmd will close automatically

            using (var pythonManager = PythonManager.Instance)
            {

                stopWatch.Start();
                var result = pythonManager.ExecuteCommand("DoStuffInPython.py", "HELLO", "WORLD");
                Console.WriteLine(result);

                result = pythonManager.ExecuteCommand("DoStuffInPython.py", "MY NAME", "IS BOND");
                Console.WriteLine(result);

                result = pythonManager.ExecuteCommand("DoStuffInPython.py", "JAMES", "BOND");
                Console.WriteLine(result);
                stopWatch.Stop();

            }

            Console.WriteLine($"Execution for new version: {stopWatch.Elapsed}");

            Console.ReadKey();
        }
    }
}
