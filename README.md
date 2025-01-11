# PythonConnect
PythonConnect allows you to access the full power of Python 3 (latest version) directly from any C# application. Sending data from C# to Python 3 and retrieving results back in C# has never been easier. PythonConnect is an alternative to Python.NET and IronPython.

In other words, any of your pre-existing Python 3 scripts can now be executed from C# code without requiring translation between the two languages.

A key feature of PythonConnect is its ability to execute C# and Python 3 code in two distinct parallel threads that work asynchronously and simultaneously. This enables seamless integration of all your custom or favorite Python libraries into your C# workflow.

How It Works:
1. Parallel Thread Creation: Starting from the main C# thread, PythonConnect launches a Command Prompt in a second parallel thread.

2. Python Environment Initialization: The Command Prompt is then turned into an Anaconda environment capable of executing any Python 3 scripts. (This requires Anaconda3 to be preinstalled, allowing easy installation of all your favorite Python libraries, such as NumPy for scientific computations.)

3. Asynchronous Communication: Both the C# and Python threads operate simultaneously in an asynchronous manner. PythonConnect manages the communication between these threads, ensuring smooth data transfer from C# to Python 3 and retrieval of results back into C#.

This approach enables effortless integration of Python's powerful features into your C# applications, extending the capabilities of both languages.
