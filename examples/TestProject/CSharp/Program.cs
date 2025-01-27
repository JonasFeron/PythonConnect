//Copyright < 2021 - 2025 > < ITAO, Université catholique de Louvain (UCLouvain)>

//Licensed under the Apache License, Version 2.0 (the "License");
//you may not use this file except in compliance with the License.
//You may obtain a copy of the License at

//    http://www.apache.org/licenses/LICENSE-2.0

//Unless required by applicable law or agreed to in writing, software
//distributed under the License is distributed on an "AS IS" BASIS,
//WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//See the License for the specific language governing permissions and
//limitations under the License.

//List of the contributors to the development of PythonConnect: see NOTICE file.
//Description and complete License: see NOTICE file.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.IO;
using PythonConnect;
using log4net.Core;
using System.Threading;

namespace PythonConnect.TestProject
{
    internal class Program
    {


        private static void Main()
        {
            #region Setup
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
            string loglevel = Level.Info.DisplayName; // Set the desired log level. Options are: Debug, Info, Warn, Error, Fatal, Off
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

            string condaEnvironmentName = "base"; // The name of the conda environment to activate. Default is "base". Or specify other environment name you might have created (refer to your anaconda navigator application)
            int timeout = 10000;//10 seconds. This is the time the python script will be allowed to run before it is killed.
            // do not set the timeout lower than 5 seconds because it takes time to initialize the python thread. 

            PythonManager.Setup(pythonProjectDirectory, condaPath, condaEnvironmentName, timeout);
            #endregion
            #endregion

            //Main Program
            //Example of how to use the PythonManager in multi-threaded program
            #region Main
            #region Python Initialization
            var stopWatch = new Stopwatch();
            stopWatch.Start();
            var pythonManager = PythonManager.Instance;

            //Alternative way to use PythonManager
            //using (var pythonManager = PythonManager.Instance)
            //{
            //    var result = pythonManager.ExecuteCommand(pythonScript, dataPath, resultPath, input1, input2);
            //} //Dispose() is called automatically
            //this method is useful if you want to use the PythonManager in a limited scope, such as in a single threaded program.

            if (pythonManager == null)
            {
                Console.WriteLine("[Main][ERROR]: PythonManager failed to initialize due to following errors: ");
                foreach (string errorMessage in PythonManager.GetErrorMessages())
                {
                    Console.WriteLine(errorMessage);
                }
                return;
            }
            stopWatch.Stop();
            double InitializationTime = stopWatch.Elapsed.TotalSeconds;
            
            Console.WriteLine($"A python thread has been initialized in: {Math.Round(InitializationTime, 2)}s");
            Console.WriteLine($"Initializing a python Thread is very time consuming!");

            Console.WriteLine($"\n\nPress any key to continue and execute a simple python command...");
            Console.ReadKey();
            #endregion

            #region Single threaded C# app
            Console.WriteLine($"\nPython {pythonScript} takes two strings as input and returns string1.lower() + string2.upper():");

            string input1 = "HELLO";
            string input2 = "world";

            stopWatch.Restart();
            var result = pythonManager.ExecuteCommand(pythonScript, dataPath, resultPath, input1, input2);
            stopWatch.Stop();
            Print(input1, input2, result, Math.Round(stopWatch.Elapsed.TotalSeconds, 2));
            Console.WriteLine($"This was fast ! ...because python was already initialized and was waiting for the order from C#.");


            Console.WriteLine($"\n\nPress any key to continue and run 3 C# threads in parallel...\n");
            Console.ReadKey();
            #endregion

            #region Multi-threaded C# app

            Thread t1 = new Thread(() =>
            {
                Console.WriteLine($"C# thread 1 says: python, execute {pythonScript}");
                string input1_t1 = "MY NAME IS";
                string input2_t1 = "bond";
                var stopWatch_t1 = new Stopwatch();
                stopWatch_t1.Start();
                result = pythonManager.ExecuteCommand(pythonScript, dataPath, resultPath, input1_t1, input2_t1);
                stopWatch_t1.Stop();
                Console.WriteLine($"\npython replies to C# thread 1: ");
                Print(input1_t1, input2_t1, result, Math.Round(stopWatch_t1.Elapsed.TotalSeconds, 2));
            });


            Thread t2 = new Thread(() =>
            {
                Console.WriteLine($"C# thread 2 says: python, execute {pythonScript}");
                string input1_t2 = "JAMES";
                string input2_t2 = "bond";
                var stopWatch_t2 = new Stopwatch();
                stopWatch_t2.Start();
                result = pythonManager.ExecuteCommand(pythonScript, dataPath, resultPath, input1_t2, input2_t2);
                stopWatch_t2.Stop();
                Console.WriteLine($"\npython replies to C# thread 2: ");
                Print(input1_t2, input2_t2, result, Math.Round(stopWatch_t2.Elapsed.TotalSeconds, 2));
            });

            Thread t3 = new Thread(() =>
            {
                Console.WriteLine($"C# thread 3 says: I am working on something else.");
                Thread.Sleep(100); //wait 0.1s
                Console.WriteLine($"\nC# thread 3 says: I am done, because I kept on working in parallel.");                
            });

            stopWatch.Restart();
            t1.Start();
            t2.Start();
            t3.Start();
            //wait for threads 1, 2 and 3 to finish
            t1.Join();
            t2.Join();
            t3.Join();
            stopWatch.Stop();
            Console.WriteLine("\nTotal execution time for threads 1, 2 and 3 = "+ Math.Round(stopWatch.Elapsed.TotalSeconds, 2));

            Console.WriteLine($"This was very very fast ! ...because only one python thread was initialized and managed multiple C# orders !");
            Console.WriteLine($"Initializing two python threads would have taken {Math.Round(2*InitializationTime,2)}s");
            #endregion

            pythonManager.Dispose();
            
            Console.WriteLine($"\n\nEnd of Demo: Press any key to exit...");
            Console.ReadKey();
            #endregion

        }

        private static void Print(string input1, string input2, string result, double time)
        {
            if (PythonManager.GetErrorMessages().Count > 0)
            {
                Console.WriteLine("[Main][ERROR]: PythonManager failed to execute command due to following errors: ");
                foreach (string errorMessage in PythonManager.GetErrorMessages())
                {
                    Console.WriteLine(errorMessage);
                }
                return;
            }
            else
            {
                Console.WriteLine("inputs: " + input1 + " " + input2);
                Console.WriteLine("return: " + result + " in: " + time + "s");
            }
        }
    }
}
