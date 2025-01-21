using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.IO;
using PythonConnect;
using log4net.Core;

namespace PythonConnect.TestProject
{
    internal class Program
    {
        
        private static void Main()
        {
            #region Paths Setup
            //1) ProjectDirectory: The directory of the project. This is the root directory that contains the Python and C# directories.
            //Console.WriteLine("the executable of this program is located here: " + new DirectoryInfo(Directory.GetCurrentDirectory()).FullName);
            string ProjectDirectory = new DirectoryInfo(Directory.GetCurrentDirectory()).Parent.Parent.Parent.FullName; //returns @"C:\Path\To\...\PythonConnect\examples\TestProject"


            //2) Python Setup
            //2.0) PythonProjectDirectory: The directory of the Python project containing the file activateCondaEnv.bat. 
            string pythonProjectDirectory = Path.Combine(ProjectDirectory, "Python");

            //2.1) CondaPath: overwrite your path to the activate.bat file to setup the conda environment.
            string condaPath = @"C:\Users\Jonas\anaconda3\Scripts\activate.bat";

            //2.2) PythonScript: name of the python script inside the PythonProjectDirectory.
            string pythonScript = @"test_script.py";


            //3) Output Directories
            //3.0) TempDirectory: The temporary directory where the log files will be stored.
            string tempDirectory = Path.Combine(ProjectDirectory, ".temp");

            //3.1) Data and Result Paths: The paths to the Data and Result files that will be used to communicate between C# and Python.
            string dataPath = Path.Combine(tempDirectory, "Data.txt");
            string resultPath = Path.Combine(tempDirectory, "Result.txt");
            #endregion

            #region Log Setup
            // Set up the logger with a desired level
            string loglevel = Level.Debug.DisplayName; // Set the desired log level. Options are: Debug, Info, Warn, Error, Fatal, Off
            LogHelper.Setup(loglevel, tempDirectory);

            // Get a logger instance to write log messages
            var log = LogHelper.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

            // Use the logger
            log.Debug("This is a debug message.");
            log.Info("This is an info message.");
            log.Warn("This is a warning message.");
            log.Error("This is an error message.");
            log.Fatal("This is a fatal message.");

            log.Info("Check the log files stored here: " + tempDirectory);
            #endregion

            #region PythonManager Setup
            log.Debug("Check that the following Paths are correct");
            log.Debug(condaPath);
            log.Debug(pythonProjectDirectory);

            int timeout = 10000;//10 seconds. This is the time the python script will be allowed to run before it is killed.
            // do not set the timeout lower than 5 seconds because it takes time to initialize the python thread. 

            PythonManager.Setup(timeout,condaPath,pythonProjectDirectory);
            #endregion


            //Main Program
            //Example of how to use the PythonManager
            var stopWatch = new Stopwatch();
            stopWatch.Start();
            using (var pythonManager = PythonManager.Instance)
            {
                stopWatch.Stop();
                Console.WriteLine($"A new python thread has been initialized in: {stopWatch.Elapsed}");
                Console.WriteLine($"\nOne advantage of using PythonConnect is: the python Thread is initialized only once!");
                Console.WriteLine($"Initializing a python Thread is very time consuming! it took: {stopWatch.Elapsed}");

                Console.WriteLine($"\nMultiple python commands can then be executed without reinitializing python everytime, which saves a lot of time!");
                Console.WriteLine($"In the following example, " + pythonScript +" takes two strings as input and returns string1.lower() and string2.upper().\n");
                Console.WriteLine("Execute python test_script with: ");

                string input1 = "HELLO";
                string input2 = "WORLD";

                stopWatch.Restart();
                var result = pythonManager.ExecuteCommand(pythonScript, dataPath, resultPath, input1, input2);
                stopWatch.Stop();
                Console.WriteLine("inputs: "+ input1 + " " + input2);
                Console.WriteLine("return: "+ result + " in: " + stopWatch.Elapsed + "\n");


                input1 = "MY NAME IS";
                input2 = "BOND";
                stopWatch.Restart();
                result = pythonManager.ExecuteCommand(pythonScript, dataPath, resultPath, input1, input2);
                stopWatch.Stop();
                Console.WriteLine("inputs: " + input1 + " " + input2);
                Console.WriteLine("return: " + result + " in: " + stopWatch.Elapsed + "\n");


                input1 = "JAMES";
                input2 = "BOND";
                stopWatch.Restart();
                result = pythonManager.ExecuteCommand(pythonScript, dataPath, resultPath, input1, input2);
                stopWatch.Stop();
                Console.WriteLine("inputs: " + input1 + " " + input2);
                Console.WriteLine("return: " + result + " in: " + stopWatch.Elapsed + "\n");


                Console.WriteLine($"\nGo to:");
                Console.WriteLine(dataPath);
                Console.WriteLine(resultPath);
                Console.WriteLine($"to see how C# and python communicated through read/write text files");
            }
            Console.ReadKey();
        }
    }
}
