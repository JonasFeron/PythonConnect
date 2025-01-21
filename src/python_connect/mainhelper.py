def execute(a_core_function,sys_argv):
    """
    From a main script, execute a (custom) core function given the system arguments passed to the script.

    For instance, the following command (executed from a command prompt):
        python main_script.py arg1 arg2 arg3
    gives the following system arguments: sys.argv = ['main_script.py', 'arg1', 'arg2', 'arg3']

    Parameters:
        a_core_function (function): a custom function expecting a list of string as input.

        sys_argv (list):        sys.argv is expected as input with the following elements:
            sys_argv[0] (str):  The name of the script.
            sys_argv[1] (str):  The key to be compared with the key in the data file.
            sys_argv[2] (str):  The path to the data file.
            sys_argv[3] (str):  The path to the result file.
    """
    arg_key, data_path, result_path = sys_argv[1:4]

    file_key, data_list = read_data(data_path)

    if arg_key == file_key:
        try:
            result_string = a_core_function(data_list)
        except Exception as e:
            result_string = str(e)
    else:
        result_string = 'The core function has tried to process the wrong data. ' \
            + 'The key provided as a script argument and the key read in the Data file did not match.'

    write_result(result_path, arg_key, result_string)

    # send the signal to C# that the result is ready to be read from the result file
    print(f"{arg_key}:{result_path}")


def read_data(path):
    """
    Reads a data file and returns the key and data.

    Parameters:
    path (str): The path to the data file. 
                The first line of the file must be the key.
                The following lines are the data.

    Returns:
    tuple: A tuple containing the file_key (string) and the data_lines (list of string).
    """
    with open(path, "r") as f:
        file_key = f.readline().strip()
        data_lines = f.read().splitlines()

    return file_key, data_lines


def write_result(result_path, key, result_string):
    """
    Writes the key and result to a result file.

    Parameters:
    result_path (str): The path to the result file.
    key (str): The key to be written to the file.
    result (str): The result to be written to the file.
    """
    with open(result_path, "w") as f:
        f.write(f"{key}\n{result_string}")
