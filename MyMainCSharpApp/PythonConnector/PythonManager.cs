using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;

namespace Muscle.PythonLink
{
    public class PythonManager : IDisposable
    {
        private static readonly log4net.ILog log = LogHelper.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// Signal to the writer when a new command has been set
        /// </summary>
        private readonly AutoResetEvent _newCommand_Signal = new AutoResetEvent(false);

        /// <summary>
        /// Signal to the writer than the previous command has been executed to avoid sending a command when the previous one is still running
        /// </summary>
        private readonly ManualResetEvent _executedCommand_SignalFromReader = new ManualResetEvent(false);

        private readonly ManualResetEvent _executedCommand_SignalFromWriter = new ManualResetEvent(false);


        /// <summary>
        /// Contains the list of commands to execute
        /// </summary>
        private readonly List<PythonCommand> _commands = new List<PythonCommand>();

        /// <summary>
        /// Contains the list of results
        /// </summary>
        private readonly List<PythonResult> _results = new List<PythonResult>();

        private readonly AutoResetEvent _initializedSignal = new AutoResetEvent(false);
        private readonly AutoResetEvent _stopProcessSignal = new AutoResetEvent(false);
        private bool _stop;
        private static bool successInitialized;
        private static int timeout = 20000; //ms
        private const string AnacondaActivatedFeedback = "AnacondaActivatedFeedback";

        public static string ActivateCondaBat; // =  @"C:\Users\Jonas\anaconda3\Scripts\activate.bat";
        public static string WorkingDirectory; // @"C:\Users\Jonas\Documents\GitHub\Muscle\MusclesPy";


        private static readonly object Locker = new object();
        private static PythonManager _instance;
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
                            log.Warn($"Main PythonManager: did not receive signal - Initialized - after waiting {timeout} ms");
                            _instance = null;
                        }
                        else log.Debug("Main PythonManager: received signal - python is ready.");
                    }
                }
                return _instance;
            }

        }

        private PythonManager()
        {
            ThreadPool.QueueUserWorkItem(StartProcess);
            successInitialized = _initializedSignal.WaitOne(timeout);
        }

        #region Public Methods

        /// <summary>
        /// For multiple arguments, pass them in the method like this: var result = ExecuteCommand("myPythonFileName", "arg1", "arg2", "arg3", ... );
        /// </summary>
        public string ExecuteCommand(string pythonFileName, params string[] args)
        {
            var command = new PythonCommand(pythonFileName, args);

            lock (_commands)
            {
                log.Debug("Main PythonManager: LOCKED - Add a new command.");
                // Add the new command to the list of commands to execute
                _commands.Add(command);
            }
            log.Debug("Main PythonManager: REALEASED");
            log.Debug("Main PythonManager: send signal - New Command");
            _newCommand_Signal.Set();

            while (true)
            {
                log.Debug("Main PythonManager: wait for signal - Executed Command");

                bool success = _executedCommand_SignalFromWriter.WaitOne(timeout);
                if (!success)
                {
                    log.Warn($"Main PythonManager: did not receive signal - Executed Command - after waiting {timeout} ms");
                    return "Python failed to answer";
                }
                log.Debug("Main PythonManager: received signal from Writer - Executed Command");
                string result = String.Empty;
                lock (_results)
                {
                    log.Debug("Main PythonManager: LOCKED - look for the result.");
                    result = _results.FirstOrDefault(o => o.Id == command.Id)?.Result;

                    if (!string.IsNullOrEmpty(result))
                    {
                        log.Debug("Main PythonManager: well retrieved the result.");
                        _results.RemoveAll(o => o.Id == command.Id);
                    }
                    else log.Warn("Main PythonManager: DID NOT FIND THE RESULT");
                }
                log.Debug("Main PythonManager: RELEASED");
                _executedCommand_SignalFromWriter.Reset();
                log.Debug("Main PythonManager: Resetted signal - Executed Command");
                log.Debug("Main PythonManager: return the result");
                return result;
            }
        }

        /// <summary>
        /// Stop the communication
        /// </summary>
        public void Dispose()
        {
            _stop = true;
            _stopProcessSignal.Set();
            _initializedSignal?.Dispose();
            _newCommand_Signal?.Dispose();
            _executedCommand_SignalFromReader?.Dispose();
            _executedCommand_SignalFromWriter?.Dispose();
            _instance = null;
            ///log.Fatal("PythonManager is dead");
        }

        #endregion

        #region Private Methods

        private void StartProcess(Object stateInfo)
        {
            #region Initialize Process

            var startInfo = new ProcessStartInfo
            {
                FileName = "cmd.exe",
                UseShellExecute = false,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
                WorkingDirectory = WorkingDirectory
            };

            var process = new Process /// using put in comment
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
                Console.WriteLine("Process not started !");
                return;
            }

            // Reads the output stream first and then waits because deadlocks are possible

            var sw = process.StandardInput; ///using put in comment
            if (!sw.BaseStream.CanWrite)
            {
                Console.WriteLine("Cannot write in StandardInput !");
                return;
            }
            // Vital to activate Anaconda
            ///log.Info("Writer: activate python");
            var activateCondaCmd = ($"activateConda.bat \"{ActivateCondaBat}\" {AnacondaActivatedFeedback}");
            sw.WriteLine(activateCondaCmd);

            #endregion

            #region Write New Commands

            while (!_stop)
            {
                ///log.Debug("Writer: wait for signal - New Command ");
                var index = WaitHandle.WaitAny(new WaitHandle[] { _newCommand_Signal, _stopProcessSignal });
                ///log.Debug("Writer: received signal");


                if (index == 1)
                {
                    ///log.Warn("Signal wants to kill pythonManager");
                    _stopProcessSignal?.Dispose();
                    break;
                }
                ///log.Debug("Writer: receveid signal is - New Command");
                lock (_commands)
                {
                    ///log.Debug("Writer: LOCKED");
                    ///log.Debug("Writer: There is " + _commands.Count + " commands to execute.");
                    if (!_commands.Any())
                    {
                        ///log.Warn("0 newCommand received");
                        continue;
                    }

                    // Send the commands to python
                    foreach (var command in _commands)
                    {
                        string path = command.WriteDataInTxtFile();
                        var commandString = command.Parameters.Any()
                            ? $"python {command.PythonFileName} \"{command.Id}\" {path}"
                            : $"python {command.PythonFileName} \"{command.Id}\"";
                        //var commandString = command.Parameters.Any()
                        //    ? $"python {command.PythonFileName} \"{command.Id}\" {PythonCommand.ToStringRepr(command.Parameters.Aggregate((o, p) => $"{o} {p}"))}"
                        //    : $"python {command.PythonFileName} \"{command.Id}\"";
                        ///log.Info("Writer: ask to execute command. ");
                        sw.WriteLine(commandString);
                        ///log.Debug("Writer: wait for signal - Executed Command");
                        _executedCommand_SignalFromReader.WaitOne();
                        ///log.Info("Writer: received signal from reader -  Executed Command");
                        _executedCommand_SignalFromReader.Reset();
                        ///log.Debug("Writer: resetted signal from reader - Executed Command");
                    }
                    ///log.Debug("Writer: Delete " + _commands.Count + " commands");
                    _commands.Clear();
                    ///log.Debug("Writer: There is " + _commands.Count + " commands to execute.");
                    _newCommand_Signal.Reset();
                    ///log.Debug("Writer: resetted signal - New Command");
                    ///log.Info("Writer: send signal -  Executed Command");
                    _executedCommand_SignalFromWriter.Set();
                }
                ///log.Debug("Writer: RELEASED");
            }

            #endregion
            process.Close();
            ///log.Fatal("Python has closed");
        }

        private void DataReceived(object s, DataReceivedEventArgs e)
        {
            ///log.Info("Reader read: " + e.Data);
            if (e.Data == null)
            {
                ///log.Debug("Python, STOP !");
                _stop = true;
            }
            else
            {
                if (e.Data == AnacondaActivatedFeedback)
                {
                    ///log.Debug("Reader: send signal - Python is ready !");
                    _initializedSignal.Set();
                }

                var i = e.Data.IndexOf(':');

                if (i == -1)
                {
                    ///log.Debug("This message is useless0");
                    return;
                }

                if (!Guid.TryParse(e.Data.Substring(0, i), out var id))
                {
                    ///log.Debug("This message is useless");
                    return;
                }
                string resultPath = e.Data.Substring(i + 1, e.Data.Length - i - 1);

                var result = ReadResultInTxtFile(resultPath, id);

                lock (_results)
                {
                    ///log.Debug("Reader: Locked");
                    _results.Add(result);
                    ///log.Debug("There is " + _results.Count + "result(s) pending");
                }
                ///log.Debug("Reader : RELEASED");
                ///log.Debug("Reader : send signal - Executed Command");
                _executedCommand_SignalFromReader.Set();
            }
        }

        private PythonResult ReadResultInTxtFile(string path, Guid key)
        {
            string key_txtFile;
            string result;
            using (StreamReader inputFile = new StreamReader(path))
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



        #endregion
    }
}
