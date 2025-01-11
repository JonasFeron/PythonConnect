def ReadDataFile(path):
    """
    Reads a data file and returns the key and data.

    Parameters:
    path (str): The path to the data file.

    Returns:
    tuple: A tuple containing the key (str) and the data (list of str).
    """
    with open(path, "r") as f:
        key = f.readline().strip()  # read the key and remove any trailing whitespace
        data_string = f.readlines()  # list of strings (each string is a line from the file)

    return (key, data_string)

def WriteResultFile(PathTo_ResultFile, key, result):
    """
    Writes the key and result to a result file.

    Parameters:
    PathTo_ResultFile (str): The path to the result file.
    key (str): The key to be written to the file.
    result (str): The result to be written to the file.
    """
    with open(PathTo_ResultFile, "w") as f:
        f.writelines([key, "\n", result])


