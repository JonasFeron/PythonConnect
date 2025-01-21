import sys
# from python_connect import mainhelper  #if "import" does not work, then use the following lines of code, or install python_connect in the anaconda environment using "pip install python_connect".

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

# if python_connect is installed in the anaconda environment, then comment out the three above statements.
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
