using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.IO;

namespace MyMainCSharpApp
{
    internal class Program
    {
        
        private static void Main()
        {
            // set up the paths required for the execution

            string pathToActivateConda = @"C:\Users\Jonas\anaconda3\Scripts\activate.bat"; // OverWrite your path to the activate.bat file of your anaconda environment
            
            string CSharpAppDirectory = new DirectoryInfo(Directory.GetCurrentDirectory()).Parent.Parent.FullName;// new DirectoryInfo(Directory.GetCurrentDirectory()) return @"...\CSharpPython3Connector\MyMainCSharpApp\bin\Debug" where MyMainCSharpApp.exe is located. 
            string solutionDirectory  = Directory.GetParent(CSharpAppDirectory).FullName;
            string pythonProjectDirectory = Path.Combine(solutionDirectory, "MyPythonProject");


            // Set up the logger with a desired level
            LogHelper.Setup("Debug",solutionDirectory);

            // Get a logger instance to write log messages
            var log = LogHelper.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

            // Use the logger
            log.Debug("This is a debug message.");
            log.Info("This is an info message.");
            log.Warn("This is a warning message.");
            log.Error("This is an error message.");
            log.Fatal("This is a fatal message.");

            log.Info("Check the log files stored here: " + solutionDirectory + @"\.logs");

            //Set up the PythonManager
            log.Debug("Check that the following Paths are correct");
            log.Debug(pathToActivateConda);
            log.Debug(pythonProjectDirectory);

            int timeout = 10000;//10 seconds. This is the time the python script will be allowed to run before it is killed.
            // do not set the timeout lower than 5 seconds because it takes time to initialize the python thread. 

            PythonManager.Setup(timeout,pathToActivateConda, pythonProjectDirectory);

            //Set up the paths to the Data and Result files.txt that will be used to transfer data between C# and Python. 
            string pathToDataFile = Path.Combine(solutionDirectory, ".io", "DataTestFile.txt");
            string pathToResultFile = Path.Combine(solutionDirectory, ".io", "ResultTestFile.txt");


            // set up the stopwatch to measure the execution time
            var stopWatch = new Stopwatch();
            
            
            stopWatch.Start();
            using (var pythonManager = PythonManager.Instance)
            {
                stopWatch.Stop();
                Console.WriteLine($"A new python thread has been initialized in: {stopWatch.Elapsed}");
                Console.WriteLine($"\nOne advantage of using CSharpPython3Connector is: the python Thread is initialized only once!");
                Console.WriteLine($"Initializing a python Thread is very time consuming! it took: {stopWatch.Elapsed}");

                Console.WriteLine($"\nMultiple python commands can then be executed without reinitializing python everytime, which saves a lot of time:");


                stopWatch.Restart();
                var result = pythonManager.ExecuteCommand("TestScript.py", pathToDataFile, pathToResultFile, "HELLO", "WORLD");
                stopWatch.Stop();
                Console.WriteLine(result + "\nExecution time: " + stopWatch.Elapsed);


                stopWatch.Restart();
                result = pythonManager.ExecuteCommand("TestScript.py", pathToDataFile, pathToResultFile, "MY NAME IS", "BOND");
                stopWatch.Stop();
                Console.WriteLine(result + "\nExecution time: " + stopWatch.Elapsed);

                stopWatch.Restart();
                result = pythonManager.ExecuteCommand("TestScript.py", pathToDataFile, pathToResultFile, "JAMES", "BOND");
                stopWatch.Stop();
                Console.WriteLine(result + "\nExecution time: " + stopWatch.Elapsed);


                Console.WriteLine($"\nIn the above example, TestScript.py was run with two strings as input. It returned string1.lower() and string2.upper().");


                Console.WriteLine($"\nNote that very large data can be transfered between C# and python through read/write Datafile.txt and Resultfile.txt");
                Console.WriteLine($"See here:");
                Console.WriteLine(pathToDataFile);
                Console.WriteLine(pathToResultFile);

            }
            Console.ReadKey();
        }
    }
}
