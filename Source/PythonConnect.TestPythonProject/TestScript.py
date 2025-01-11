import sys
import Util.InputOutput as IO


def main():
    """
    Main function to process data from a file.txt and write the result to another file.txt.
    The script must be called from the command prompt with:
    python TestScript.py arg1 arg2 arg3
    where arg1 is a GuiD key, 
    arg2 is the path to the data file.txt to be processed by python, 
    and arg3 is the path to the result file.txt to be written by python.
    """
    # sys.argv[0] returns the name of the script "TestScript.py" 
    key = sys.argv[1]  #returns the first argument "arg1". It must be a GuID key to ensure that the result returned by python is correctly associated with the right data in C#
    PathTo_DataFile = sys.argv[2]  # returns the second argument "arg2". It must be the path to the data file to be processed by python.    
    PathTo_ResultFile = sys.argv[3] # returns the third argument "arg3". It must be the path to the result file to be (over-)written by python.
    
    # the Data file must contain:
    # In first line: a key (that shall be identical to the one received from the command prompt. This ensures that the TestScript.py is applied to the correct data from the Data File.txt)
    # In the following lines: the data to be processed by python
    (key_FromDataFile, data_FromDataFile) = IO.ReadDataFile(PathTo_DataFile)  # read the key and data from the DataFile.txt

    output_string = " "
    if key == key_FromDataFile: # the keys - read from the DataFile.txt and received from the command prompt - shall be identical to ensure that the TestScript.py is applied to the correct data.
        try:
            # Here do whatever you want with the data.
            output_string = MyFunction(data_FromDataFile)

        except Exception as e:
            # if an error occurs, the error message is written to the result file
            output_string = str(e)
    else:
        output_string = "Keys from command prompt and Data text file did not match"

    # write the result as a string to the ResultFile.txt, starting with the key as first line. 
    IO.WriteResultFile(PathTo_ResultFile, key, output_string)

    # send the signal to C# that the result is ready to be read from the result file.txt
    print(key + ":" + PathTo_ResultFile)    # it is important that the PathTo_ResultFile is contained in the message.



def MyFunction(data_FromDataFile):
    """
    Function to process data from the data file.

    Args:
        data_FromDataFile (list): List of strings, each string is a line from the DataFile.txt.

    Returns:
        str: Processed result as a single string.
    """
    # 1) retrieve data
    # data_FromDataFile is a list of strings. Each string is a line from the DataFile.txt
    data0, data1 = data_FromDataFile[:2]  # retrieve first two lines of data

    # 2) process data
    result0 = data0.strip().lower()  # to lower case and remove leading/trailing whitespace
    result1 = data1.strip().upper()  # to upper case and remove leading/trailing whitespace

    # 3) write result
    output_string = f"{result0} {result1}"
    return output_string


if __name__ == '__main__':  
    main()