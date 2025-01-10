using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;

namespace MyMainCSharpApp
{
    public class PythonManager : IDisposable
    {

        #region Private Properties
        // three properties to be set by the user in the main program
        private static int _timeout = 20000; //set the timeout to 20 seconds, after that the program will stop waiting for a signal and close. 
        private static string _pathToActivateConda; // set the path to the activate.bat file in your Anaconda Environnement for instance: ActivateCondaBat = @"C:\Users\Me\anaconda3\Scripts\activate.bat";
        private static string _pythonProjectDirectory; // Path to the python project @"C:\Users\Me\Documents\CSharpPython3Connector\MyPythonProject";



        private static readonly log4net.ILog log = LogHelper.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType); // Log object to log messages

        private static PythonManager _instance; // Singleton instance of the PythonManager
        private static readonly object Locker = new object(); // Lock object to ensure thread safety

        private static bool successInitialized; // Boolean to check if the Python process has been initialized successfully
        private const string AnacondaActivatedFeedback = "AnacondaActivatedFeedback"; 

        private readonly List<PythonCommand> _commands = new List<PythonCommand>(); // List of commands to execute

        private readonly List<PythonResult> _results = new List<PythonResult>(); // List of results

        private bool _stop;

        #endregion


        #region Signals

        // Signal when a new command has been set 
        private readonly AutoResetEvent _signal_newCommand = new AutoResetEvent(false);


        private readonly ManualResetEvent _signal_executedCommand_fromPythonThread = new ManualResetEvent(false);
        private readonly ManualResetEvent _signal_returnTheResults = new ManualResetEvent(false);

        // Signal when the process "Command Prompt" has been initialized as an Anaconda environnement able to execute python scripts
        private readonly AutoResetEvent _signal_initializedPythonThread = new AutoResetEvent(false);

        // Signal to stop the python thread
        private readonly AutoResetEvent _signal_stopPythonThread = new AutoResetEvent(false);

        #endregion


        #region Constructor
        /// This constructor starts a new thread to initialize the Python process and waits for the initialization signal.
        /// </summary>
        private PythonManager()
        {
            log.Debug("Initializing PythonManager...");
            ThreadPool.QueueUserWorkItem(ManagePythonThread); // Start a new thread to initialize the Python process
            successInitialized = _signal_initializedPythonThread.WaitOne(_timeout); // Wait for the initialization signal
            log.Debug($"PythonManager initialized: {successInitialized}");
        }

        #endregion


        #region Get Method

        /// <summary>
        /// Gets the singleton instance of the PythonManager. 
        /// </summary>
        /// <remarks>
        /// A PythonManager manages the communication with a Python process in a separate thread.
        /// This property ensures that only one Python thread is created (Singleton pattern).
        /// It initializes the Python thread if it is not already initialized and waits for the initialization signal.
        /// </remarks>
        public static PythonManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (Locker)
                    {
                        if (_instance != null)
                        {
                            // If _instance has been implemented in the meantime
                            return _instance;
                        }

                        _instance = new PythonManager();

                        if (!successInitialized)
                        {
                            log.Warn($"PythonManager.Instance: did not receive signal - Initialized - after waiting {_timeout} ms");
                            _instance = null;
                        }
                        else log.Debug("PythonManager.Instance: received signal - python is ready.");
                    }
                }
                return _instance;
            }
        }

        #endregion


        #region Private Methods

        private void ManagePythonThread(Object stateInfo)
        {
            #region 1) Initialize Python Thread

            var startInfo = new ProcessStartInfo
            {
                FileName = "cmd.exe",
                UseShellExecute = false,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
                WorkingDirectory = _pythonProjectDirectory
            };

            var process = new Process 
            {
                StartInfo = startInfo
            };

            process.OutputDataReceived += DataReceived;
            process.ErrorDataReceived += DataReceived;

            bool isStarted;
            try
            {
                isStarted = process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();
            }
            catch (Exception e)
            {
                // Usually it occurs when an executable file is not found or is not executable

                Console.WriteLine($"Exception during the start of the process: {e}");
                isStarted = false;
            }

            if (!isStarted)
            {
                Console.WriteLine("the new process \"Command Prompt\" has failed to start !");
                return;
            }
            log.Info("ManagePythonThread: a new thread \"Command Prompt\" has been launched");



            var console = process.StandardInput; 
            if (!console.BaseStream.CanWrite)
            {
                Console.WriteLine("Cannot write in the CSharp console !");
                return;
            }
             
            log.Info("ManagePythonThread: turn the \"Command Prompt\" into a python environment using Anaconda");
            var activateCondaCmd = ($"activateConda.bat \"{_pathToActivateConda}\" {AnacondaActivatedFeedback}");
            console.WriteLine(activateCondaCmd);

            #endregion

            #region 2) Write Commands to Python

            while (!_stop)
            {
                log.Debug("ManagePythonThread: wait for signal \"New Command\"");
                var index = WaitHandle.WaitAny(new WaitHandle[] { _signal_newCommand, _signal_stopPythonThread });
                log.Debug("ManagePythonThread: received a signal");


                if (index == 1)
                {
                    log.Warn("Signal wants to kill the python Thread");
                    _signal_stopPythonThread?.Dispose();
                    break;
                }
                log.Debug("Signal is a New Command");

                lock (_commands)
                {
                    log.Debug("ManagePythonThread: is LOCKED");
                    log.Debug("ManagePythonThread: There is " + _commands.Count + " commands to execute.");
                    if (!_commands.Any())
                    {
                        continue;
                    }

                    // Send the commands to python
                    foreach (var command in _commands)
                    {
                        command.WriteDataFile(); // Write the data to a file.txt to be read by the python script
                        var commandString = command.Datas.Any()
                            ? $"python {command.PythonScriptName} \"{command.Id}\" \"{command.PathTo_DataFile}\" \"{command.PathTo_ResultFile}\""
                            : $"python {command.PythonScriptName} \"{command.Id}\"";
                        
                        log.Debug("ManagePythonThread: execute a python command. write in the console: "+commandString);
                        console.WriteLine(commandString);

                        log.Debug("ManagePythonThread: wait for signal \"Executed Command\"");
                        _signal_executedCommand_fromPythonThread.WaitOne();
                        log.Info("ManagePythonThread: received signal \"Executed Command\": a command was executed by python, and a result was retrieved.");
                        
                        _signal_executedCommand_fromPythonThread.Reset();
                    }
                    _commands.Clear();
                    _signal_newCommand.Reset();

                    log.Info("ManagePythonThread: send signal \"Return the Results\"");
                    _signal_returnTheResults.Set();
                }
            }

            #endregion
            process.Close();
        }

        /// <summary>
        /// Handles the data received from the Python process.
        /// </summary>
        /// <param name="s">The source of the event.</param>
        /// <param name="e">The data received event arguments.</param>
        private void DataReceived(object s, DataReceivedEventArgs e)
        {
            log.Debug("DataReceived: the following data was read in the console:" + e.Data);
            if (e.Data == null)
            {
                _stop = true;
            }
            else
            {
                if (e.Data == AnacondaActivatedFeedback)
                {
                    log.Debug("DataReceived: send signal \"the Python Thread has been correctly initialized !\"");
                    _signal_initializedPythonThread.Set();
                }

                // If the message received in the console is a result from a python script
                // the message is in the form of "id:pathTo_ResultFile"
                var i = e.Data.IndexOf(':');

                if (i == -1)
                {
                    log.Debug("DataReceived: This data is useless");
                    return;
                }

                if (!Guid.TryParse(e.Data.Substring(0, i), out var id))
                {
                    log.Debug("DataReceived: This data is also useless");
                    return;
                }

                log.Debug("DataReceived: a GuID was retrieved");

                string pathTo_ResultFile = e.Data.Substring(i + 1, e.Data.Length - i - 1);
                log.Debug("DataReceived: the following path to a result file was retrieved: " + pathTo_ResultFile);

                var pythonResult = ReadResultFile(pathTo_ResultFile, id);
                log.Info("DataReceived: the ResultFile was read and stored in a pythonResult Object.");

                lock (_results)
                {
                    _results.Add(pythonResult);
                    log.Debug("DataReceived: a pythonResult was added to the list of results");
                }
                log.Info("DataReceived: send signal \"Executed Command\": a command was executed by python, and a result was retrieved.");
                _signal_executedCommand_fromPythonThread.Set();
            }
        }

        /// <summary>
        /// Reads the result file and returns a PythonResult object.
        /// </summary>
        /// <param name="pathTo_ResultFile">The path to the result file.</param>
        /// <param name="key">The unique identifier for the command.</param>
        /// <returns>A PythonResult object containing the result of the command, or null if the key does not match.</returns>
        private PythonResult ReadResultFile(string pathTo_ResultFile, Guid key)
        {
            string key_txtFile;
            string result;
            using (StreamReader inputFile = new StreamReader(pathTo_ResultFile))
            {
                key_txtFile = inputFile.ReadLine();
                result = inputFile.ReadToEnd();
            }
            if (key_txtFile == key.ToString())
            {
                return new PythonResult(key, result);
            }
            else return null;
        }


        /// <summary>
        /// Sets up the PythonManager with the specified timeout, path to activate Conda, and Python project directory.
        /// </summary>
        /// <param name="timeout">The timeout duration in milliseconds.</param>
        /// <param name="pathToActivateConda">The path to the activate.bat file in the Anaconda environment.</param>
        /// <param name="pythonProjectDirectory">The path to the Python project directory.</param>
        public static void Setup(int timeout, string pathToActivateConda, string pythonProjectDirectory)
        {
            _timeout = timeout;
            _pathToActivateConda = pathToActivateConda;
            _pythonProjectDirectory = pythonProjectDirectory;
        }

        /// <summary>
        /// Executes a Python script with the specified data and result file paths.
        /// </summary>
        /// <param name="pythonFileName">The name of the Python script to be executed.</param>
        /// <param name="dataFilePath">The path to the file where the data will be written.</param>
        /// <param name="resultFilePath">The path to the file where the result will be written.</param>
        /// <param name="datas">The data to be written to the data file.</param>
        /// <returns>A string containing the result of the command execution.</returns>
        public string ExecuteCommand(string pythonFileName, string dataFilePath, string resultFilePath, params string[] datas)
        {
            var command = new PythonCommand(pythonFileName, dataFilePath, resultFilePath, datas); // Create a new command and write the data to a file

            lock (_commands)
            {
                log.Debug("ExecuteCommand: LOCKED - Add a new command.");
                _commands.Add(command); // Add the new command to the list of commands to execute
            }
            log.Debug("ExecuteCommand: RELEASED");
            log.Debug("ExecuteCommand: send signal \"New Command\"");
            _signal_newCommand.Set();

            //continue reading the code in method "NewPythonThread", region 2) Write New Commands, from the begining of the while(!stop) loop. 
            //...
            //come back here once _signal_returnTheResults has been Set at the end of the while(!stop) loop 

            log.Debug("ExecuteCommand: wait for signal - Return the results");
            bool success = _signal_returnTheResults.WaitOne(_timeout);
            if (!success)
            {
                log.Warn($"Main PythonManager: did not receive signal - Executed Command - after waiting {_timeout} ms");
                return "Python failed to answer";
            }

            log.Debug("ExecuteCommand: received signal \"Return the results\" because that the python command has been executed");

            string result = null;
            lock (_results)
            {
                log.Debug("ExecuteCommand: LOCKED");
                log.Info("ExecuteCommand: retrieve the result with the correct Id in the list of results, and return it.");

                var pythonResult = _results.FirstOrDefault(o => o.Id == command.Id); // In the list of results received from python, find the result that has the same Id than the command 
                if (pythonResult != null)
                {
                    result = pythonResult.Result;
                    log.Info("ExecuteCommand: the result has well been retrieved.");
                    _results.RemoveAll(o => o.Id == command.Id); //delete the processed result from the list of results
                    log.Debug("ExecuteCommand: a pythonResult was deleted from the list of results.");
                }
                else
                {
                    log.Warn("ExecuteCommand: DID NOT FIND THE RESULT");
                }
            }
            log.Debug("ExecuteCommand: RELEASED");
            _signal_returnTheResults.Reset();
            log.Debug("ExecuteCommand: Resetted signal \"Return the results\"");
            log.Info("ExecuteCommand: the result from python has been returned.");

            return result; // a string that contains the result of the command
        }


        /// <summary>
        /// Disposes the resources used by the PythonManager.
        /// </summary>
        public void Dispose()
        {
            _stop = true;
            _signal_stopPythonThread.Set();

            _signal_initializedPythonThread?.Dispose();
            _signal_newCommand?.Dispose();
            _signal_executedCommand_fromPythonThread?.Dispose();
            _signal_returnTheResults?.Dispose();

            _instance = null;
            log.Fatal("The Python Thread has been stopped");
        }

        #endregion


    }
}
