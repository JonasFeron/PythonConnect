# Copyright <2021-2025> <ITAO, Université catholique de Louvain (UCLouvain)>

# Licensed under the Apache License, Version 2.0 (the "License");
# you may not use this file except in compliance with the License.
# You may obtain a copy of the License at

#     http://www.apache.org/licenses/LICENSE-2.0

# Unless required by applicable law or agreed to in writing, software
# distributed under the License is distributed on an "AS IS" BASIS,
# WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
# See the License for the specific language governing permissions and
# limitations under the License.

# List of the contributors to the development of PythonConnect: see NOTICE file.
# Description and complete License: see NOTICE file.

import sys
# from python_connect import mainhelper  #if "import" does not work, then use the following lines of code, or copy/paste python_connect folder in the same directory as test_script.py.

# In test_script.py, import mainhelper from the python_connect package given the following directory structure:
#PythonConnect\
#              ├── src\
#              │   └── python_connect\
#              │       ├── __init__.py
#              │       ├── mainhelper.py
#              └── examples\
#                  └── TestProject\
#                      └── Python\
#                          └── test_script.py
import os
base_dir = os.path.abspath(os.path.join(os.path.dirname(__file__), '../../../src'))
sys.path.append(base_dir)

# if python_connect is installed in the same directory as test_script.py, then comment out the three above statements.
from python_connect import mainhelper 


def main():
    mainhelper.execute(test_function, sys.argv)


def test_function(data_lines):
    """
    Here explain what the function does.

    Important note: The function must take a list of strings as input and return a single string as output.

    Args:
        data_lines (list): List of strings where each string is a line from the DataFile.txt. 

    Returns:
        str: a single string containing the result of the function. It will be written to the result file.
    """

    # 1) retrieve data
    # Note: data.lines[0] corresponds to the first line of actual data. 
    #       The very first line of DataFile.txt (containing an automatically generated key) is skipped.

    data0, data1 = data_lines[:2] 
    # data = data_lines[0] #if the data is contained in only one line in DataFile.txt

    # 2) process data. 
    #Example: convert to lower case and upper case. 
    #Note: strip() removes leading/trailing whitespace

    result0 = data0.strip().lower()  
    result1 = data1.strip().upper() 


    # 3) return result. 
    # Note: refer to JSON formatting to deal with more complex data and result.
    return f"{result0} {result1}"


if __name__ == '__main__':  
    main()
