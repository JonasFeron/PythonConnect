using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyMainCSharpApp
{
    /// <summary>
    /// Represents the result of a Python script execution.
    /// </summary>
    public class PythonResult
    {
        public Guid Id { get; }
        public string Result { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="PythonResult"/> class.
        /// </summary>
        /// <param name="id">The unique identifier of the Python command.</param>
        /// <param name="result">The result of the Python script execution.</param>
        public PythonResult(Guid id, string result)
        {
            Id = id;
            Result = result;
        }
    }

}
