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

namespace PythonConnect
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
