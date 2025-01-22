using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;

namespace PythonConnect
{
    public class PythonManager : IDisposable
    {

        #region Private Properties
        // three properties to be set by the user in the main program
        private static int _timeout = 20000; //set the timeout to 20 seconds, after that the program will stop waiting for a signal and close. 
        private static string _condaPath; // set the path to the activate.bat file in your Anaconda Environnement for instance: ActivateCondaBat = @"C:\Users\Me\anaconda3\Scripts\activate.bat";
        private static string _pythonProjectDirectory; // Path to the python project that contains the python scripts to be executed.
        private static string _condaEnvironmentName = "base"; // Name of the Anaconda environment to activate


        private static readonly log4net.ILog log = LogHelper.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType); // Log object to log messages

        private static PythonManager _instance; // Singleton instance of the PythonManager
        private static readonly object Locker = new object(); // Lock object to ensure thread safety

        private readonly List<PythonCommand> _commands = new List<PythonCommand>(); // List of commands to execute

        private readonly List<PythonResult> _results = new List<PythonResult>(); // List of results

        private static readonly List<string> _errorMessages = new List<string>(); // error messages received from the python process


        private bool _stop;

        #endregion


        #region Signals

        // Signal when a new command has been set 
        private readonly AutoResetEvent _signal_newCommand = new AutoResetEvent(false);


        private readonly ManualResetEvent _signal_executedCommand_fromPythonThread = new ManualResetEvent(false);
        private readonly ManualResetEvent _signal_resultsReady = new ManualResetEvent(false);


        // Signal when the activation of the Anaconda environment has finished. Activation can be successful or not.
        private readonly AutoResetEvent _signal_pythonThreadActivation = new AutoResetEvent(false);
        private static bool successfulPythonActivation; // Boolean to check if the Python process has been activated successfully
        private const string feedback_success_pythonActivation = "feedback_success_pythonActivation";
        private const string feedback_fail_condaBaseEnv = "feedback_fail_condaBaseEnv";
        private const string feedback_fail_condaOtherEnv = "feedback_fail_condaOtherEnv";


        // Signal to stop the python thread
        private readonly AutoResetEvent _signal_stopPythonThread = new AutoResetEvent(false);

        #endregion


        #region Constructor
        /// This constructor starts a new thread to initialize the Python process and waits for the initialization signal.
        /// </summary>
        private PythonManager()
        {
            log.Info("Try to create a PythonManager");
            log.Debug("Then will try to launch a new Command Prompt Process in a parallel thread");
            log.Debug("Then will try to turn this Command Prompt into a python environment using Anaconda");
            log.Debug("Then will wait for python commands to execute");

            ThreadPool.QueueUserWorkItem(ManagePythonThread); // Start a new thread to initialize the Python process

            log.Debug("Wait signal - End of Python Activation -");
            bool feedbackReceived = _signal_pythonThreadActivation.WaitOne(_timeout);

            if (!feedbackReceived)
            {
                log.Warn($"Did not receive activation feedback within {_timeout} ms.");
                successfulPythonActivation = false;
            }
            else log.Debug("Received signal - End of Python Activation -");
            log.Info("Python activated successfully ?: " + successfulPythonActivation);
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
                log.Debug($"get Instance called: try to return a singleton instance");

                if (_instance == null)
                {
                    lock (Locker)
                    {
                        log.Debug($"LOCK Instance");
                        if (_instance != null)
                        {
                            log.Debug($"_instance is not null: it has been created in the meantime");
                            return _instance;
                        }
                        log.Debug($"_instance is null: call PythonManager constructor");

                        _instance = new PythonManager();
                        log.Info($"PythonManager_instance created");
                        if (!successfulPythonActivation)
                        {
                            log.Error($"But CondaEnv activation failed: kill PythonManager instance");
                            _instance = null;
                            return _instance;
                        }
                        log.Info($"successful CondaEnv activation");
                    }
                    log.Debug($"Released Instance");
                }
                return _instance;
            }
        }

        #endregion


        #region Private Methods

        private void ManagePythonThread(Object stateInfo)
        {
            #region 1) Initialize Python Thread
            log.Debug("Launch Command Prompt here: "+ _pythonProjectDirectory);
            var startInfo = new ProcessStartInfo
            {
                FileName = "cmd.exe",               // Use Command Prompt
                UseShellExecute = false,           // Required for redirection
                RedirectStandardInput = true,      // Allow sending commands
                RedirectStandardOutput = true,     // Capture stdout
                RedirectStandardError = true,      // Capture stderr
                CreateNoWindow = true,             // Do not show a new window
                WorkingDirectory = _pythonProjectDirectory // Set the working directory
            };

            var process = new Process 
            {
                StartInfo = startInfo
            };

            // Attach event handlers for stdout and stderr
            process.OutputDataReceived += OutputDataReceived; 
            process.ErrorDataReceived += ErrorDataReceived;   

            bool isStarted;
            try
            {
                isStarted = process.Start(); // Start the process
                process.BeginOutputReadLine(); // Begin reading stdout
                process.BeginErrorReadLine();  // Begin reading stderr
            }
            catch (Exception e)
            {
                // Handle process start failure
                Console.WriteLine($"Exception during the start of the process: {e}");
                isStarted = false;
            }

            if (!isStarted)
            {
                Console.WriteLine("the new process \"Command Prompt\" has failed to start !");
                successfulPythonActivation = false;
                _signal_pythonThreadActivation.Set();
                return;
            }
            log.Info("Command Prompt process has started successfully.");

            log.Info("Command Prompt tries to activate a Python environment using Anaconda.");
            log.Info("environment Name is: "+ _condaEnvironmentName);

            var activateCondaCmd = $"activateCondaEnv.bat \"{_condaPath}\" \"{_condaEnvironmentName}\" {feedback_success_pythonActivation} {feedback_fail_condaBaseEnv} {feedback_fail_condaOtherEnv}";

            log.Debug("Command Prompt launches batch script: activateCondaEnv.bat");
            process.StandardInput.WriteLine(activateCondaCmd);


            #endregion

            #region 2) Write Commands to Python

            while (!_stop)
            {
                log.Debug("Command Prompt waits for signal - New Command -");
                var index = WaitHandle.WaitAny(new WaitHandle[] { _signal_newCommand, _signal_stopPythonThread });
                log.Debug("Command Prompt received a signal");

                if (index == 1)
                {
                    log.Warn("Signal = Stop Python Thread");
                    _signal_stopPythonThread?.Dispose(); 
                    break;
                }
                log.Debug("Signal = New Command");

                lock (_commands)
                {
                    log.Debug("LOCKED list of commands contains "+ _commands.Count + " commands to execute.");

                    if (!_commands.Any())
                    {
                        log.Debug("No more commands to execute.");
                        continue;
                    }

                    // Send the commands to python
                    foreach (var command in _commands)
                    {
                        log.Info("A data file.txt is written with data for the python script");
                        command.WriteDataFile(); 
                        var commandString = command.Datas.Any()
                            ? $"python {command.PythonScriptName} \"{command.Id}\" \"{command.DataPath}\" \"{command.ResultPath}\""
                            : $"python {command.PythonScriptName} \"{command.Id}\"";

                        log.Info("Launch the python script");
                        log.Debug("Command Prompt writes: "+commandString);
                        process.StandardInput.WriteLine(commandString);

                        log.Debug("Wait for signal - Executed Command -");
                        _signal_executedCommand_fromPythonThread.WaitOne();
                        log.Debug("Received signal - Executed Command -");
                        log.Info("Python script completed");

                        _signal_executedCommand_fromPythonThread.Reset();
                    }
                    _commands.Clear();
                    _signal_newCommand.Reset();

                    log.Debug("Send signal - Results are ready -");
                    _signal_resultsReady.Set();
                }
                log.Debug("RELEASED list of commands");

            }

            #endregion
            process.Close();
        }

        private void ErrorDataReceived(object s, DataReceivedEventArgs e)
        {
            log.Warn($"Command Prompt reads ERROR : {e.Data}");
            if (e.Data == null)
            {
                _stop = true;
                return;
            }
            else
            {
                if (e.Data.Trim().Length == 0) { return; }//empty error message
                else if (e.Data.Trim() == feedback_fail_condaBaseEnv)
                {
                    log.Error("Python activation failed: C:\\Path\\To\\Anaconda3\\Scripts\\activate.bat can not be found");
                    successfulPythonActivation = false;
                    log.Debug("Send signal - End of Python Activation -");
                    _signal_pythonThreadActivation.Set();
                    Dispose();
                }
                else if (e.Data.Trim() == feedback_fail_condaOtherEnv)
                {
                    log.Error("Python activation failed: Name of conda environment do not exist -> check Anaconda Navigator application");
                    successfulPythonActivation = false;
                    log.Debug("Send signal - End of Python Activation -");
                    _signal_pythonThreadActivation.Set();
                    Dispose();
                }
                else
                {
                    addErrorMessage(e.Data);
                }

            }
        }

        /// <summary>
        /// Handles the data received from the Python process.
        /// </summary>
        /// <param name="s">The source of the event.</param>
        /// <param name="e">The data received event arguments.</param>
        private void OutputDataReceived(object s, DataReceivedEventArgs e)
        {
            log.Debug("Command Prompt reads: " + e.Data);
            if (e.Data == null)
            {
                _stop = true;
                return;
            }
            else
            {
                // Trim whitespace from e.Data before comparison
                if (e.Data.Trim() == feedback_success_pythonActivation)
                {
                    log.Debug("Successful Python activation!");
                    successfulPythonActivation = true;
                    log.Debug("Send signal - End of Python Activation -");
                    _signal_pythonThreadActivation.Set();
                }

                // If the message received in the console is a result from a python script
                // the message is in the form of "id:resultPath"
                var i = e.Data.IndexOf(':');

                if (i != -1 && Guid.TryParse(e.Data.Substring(0, i), out var id))
                {
                    log.Debug("A GUID was identified.");
                    string resultPath = e.Data.Substring(i + 1);
                    log.Debug("Retrieved result file path: " + resultPath);

                    var pythonResult = ReadResultFile(resultPath, id);
                    log.Info("Result file read and stored.");

                    lock (_results)
                    {
                        log.Debug("LOCK list of results");
                        _results.Add(pythonResult);
                        log.Info("Result added to the list.");
                    }
                    log.Debug("RELEASED list of results");

                    log.Debug("Send Signal - Executed Command -");
                    _signal_executedCommand_fromPythonThread.Set();
                }
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

        private static void addErrorMessage(string errorMessage)
        {
            lock (_errorMessages)
            {
                _errorMessages.Add(errorMessage);
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Sets up the PythonManager with the specified timeout, path to activate Conda, Python project directory, and environment name.
        /// </summary>
        /// <param name="timeout">The timeout duration in milliseconds.</param>
        /// <param name="condaPath">The path to the activate.bat file in the Anaconda environment.</param>
        /// <param name="pythonProjectDirectory">The path to the Python project directory.</param>
        /// <param name="condaEnvironmentName">The name of the Anaconda environment to activate. Default is "base".</param>
        public static void Setup(string pythonProjectDirectory, string condaPath, string condaEnvironmentName = "base", int timeout=10000)
        {
            _timeout = timeout;
            _condaPath = condaPath;
            _pythonProjectDirectory = pythonProjectDirectory;
            _condaEnvironmentName = condaEnvironmentName;
            log.Info("PythonManager setup completed.");
        }

        /// <summary>
        /// Executes a Python script with the specified data and result file paths.
        /// </summary>
        /// <param name="pythonFileName">The name of the Python script to be executed.</param>
        /// <param name="dataPath">The path to the file where the data will be written.</param>
        /// <param name="resultPath">The path to the file where the result will be written.</param>
        /// <param name="datas">The data to be written to the data file. Multiple datas will be written in multiple lines.</param>
        /// <returns>A string containing the result of the command execution.</returns>
        public string ExecuteCommand(string pythonFileName, string dataPath, string resultPath, params string[] datas)
        {
            var command = new PythonCommand(pythonFileName, dataPath, resultPath, datas); // Create a new command and write the data to a file

            lock (_commands)
            {
                log.Debug("LOCKED list of commands");
                log.Info("Add a new command");
                _commands.Add(command); 
            }
            log.Debug("RELEASED list of commands");
            log.Debug("Send signal - New Command -");
            _signal_newCommand.Set();

            //Ctrl + F and look for _signal_newCommand; to understand the flow of the program
            //...
            //come back here once _signal_resultsReady has been send

            log.Debug("Wait for signal - Results are ready -");
            bool success = _signal_resultsReady.WaitOne(_timeout);

            if (!success)
            {
                log.Warn($"Did not receive signal - Results are ready - after waiting {_timeout} ms");
                return "Python failed to answer";
            }
            log.Debug("Received signal - Results are ready -");


            string result = null;
            lock (_results)
            {
                log.Debug("LOCKED list of results contains " + _results.Count + " results to return.");
                log.Info("Try to retrieve the correct result in the list of results.");

                var pythonResult = _results.FirstOrDefault(o => o.Id == command.Id);
                if (pythonResult != null)
                {
                    log.Info("Retrieved the correct result.");
                    result = pythonResult.Result;
                    _results.RemoveAll(o => o.Id == command.Id); 
                    log.Debug("Delete the correct result from the list");
                }
                else
                {
                    log.Warn("DID NOT FIND THE RESULT");
                }
            }
            log.Debug("RELEASED list of results");
            _signal_resultsReady.Reset();

            log.Info("Return the result from python to main CS Program");
            return result; // as a string 
        }


        /// <summary>
        /// Disposes the resources used by the PythonManager.
        /// </summary>
        public void Dispose()
        {
            _stop = true;
            _signal_stopPythonThread.Set();
            _signal_pythonThreadActivation?.Dispose();
            _signal_newCommand?.Dispose();
            _signal_executedCommand_fromPythonThread?.Dispose();
            _signal_resultsReady?.Dispose();

            _instance = null;
            log.Fatal("The Python Thread has stopped");
        }


        public static List<string> GetErrorMessages()
        {
            List<string> _errors = new List<string>();

            lock (_errorMessages)
            {
                _errors.AddRange(_errorMessages);
                _errorMessages.Clear();
            }
            return _errors;
        }
        #endregion


    }
}
