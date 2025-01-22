using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace PythonConnect
{
    /// <summary>
    /// Represents a command to execute a Python script with specified data and result file paths.
    /// </summary>
    public class PythonCommand
    {
        public Guid Id { get; }
        public string PythonScriptName { get; } 
        public string DataPath { get; } 
        public string ResultPath { get; } 
        public List<string> Datas { get; } 

        /// <summary>
        /// Initializes a new instance of the <see cref="PythonCommand"/> class.
        /// </summary>
        /// <param name="pythonScriptName">The name of the Python script to be executed. The python script must be located in "MyPythonProject" directory.</param>
        /// <param name="pathTo_DataFile">The path to the file where the data will be written.</param>
        /// <param name="pathTo_ResultFile">The path to the file where the result will be written.</param>
        /// <param name="datas">The data to be written to the data file.</param>
        public PythonCommand(string pythonScriptName, string pathTo_DataFile, string pathTo_ResultFile, params string[] datas)
        {
            Id = Guid.NewGuid();
            PythonScriptName = pythonScriptName;
            DataPath = pathTo_DataFile;
            ResultPath = pathTo_ResultFile;
            Datas = datas.Select(o => $"{o}").ToList();
        }

        /// <summary>
        /// Writes the data for a Python Command to a specified file.
        /// </summary>
        public void WriteDataFile()
        {
            // Ensure the directory exists
            string directory = Path.GetDirectoryName(DataPath);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            // Ensure the file exists
            if (!File.Exists(DataPath))
            {
                File.Create(DataPath).Dispose();
            }

            using (StreamWriter outputFile = new StreamWriter(DataPath, false))
            {
                outputFile.WriteLine(Id);
                foreach (string data in Datas)
                {
                    outputFile.WriteLine(data);
                }
            }
        }

        /// <summary>
        /// Returns the representation of a string considering special characters.
        /// </summary>
        /// <param name="data">The input string.</param>
        /// <returns>The string representation with special characters escaped.</returns>
        public static string ToStringRepr(string data)
        {
            StringBuilder result = new StringBuilder();

            result.Append("\"");
            foreach (char c in data)
            {
                if (c.ToString() == "\"") result.Append("\\\"");
                else result.Append(c);
            }
            result.Append("\"");
            return result.ToString();
        }
    }

}

