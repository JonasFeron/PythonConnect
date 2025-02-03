# PythonConnect
Seamlessly integrate Python 3 into C# applications.

## Overview
**PythonConnect** provides an efficient way to execute **Python 3 scripts** directly from **C#** without requiring language translation. It enables **asynchronous, parallel execution**, and facilitates the communication between **Python and C# threads**.

With **PythonConnect**, you can:
- Run **any Python 3 script** within a C# application.
- Use Python libraries such as **NumPy**, **Pandas**, and more.
- Run Python and C# in **separate parallel processes**, ensuring smooth communication.
- Keep the **Python process active**, avoiding reinitialization overhead.


## Why PythonConnect?
As an alternative to **Python.NET**, **IronPython**, and **subprocess-based solutions**, PythonConnect offers:  
✅ **Full Python 3 Compatibility** – Works with the latest Python versions.  
✅ **Anaconda Support** – Manages environments and installs external Python libraries with ease.  
✅ **Seamless C# Integration** – Directly execute Python scripts inside C# workflows.  
✅ **Multi-Threading** – Runs Python scripts asynchronously alongside C# without blocking the main C# execution.  
✅ **Optimized Performance** – Keeps Python process active, reducing initialization time at each execution.  


## How It Works
PythonConnect establishes a persistent **Python process** that runs in parallel with C#. Instead of launching a new Python instance for every script execution (which is slow), it maintains a background **Python process** that listens for execution requests.

### Key Steps:
1. **Parallel Execution** – PythonConnect starts a background process running Python independently from C#.
2. **Python Environment Setup** – Requires **Anaconda3**, which manages dependencies and facilitates the initialization.
3. **Asynchronous Communication** – Data is exchanged between Python and C# in real-time without blocking execution.

## Installation

### Prerequisites
✅ **Install Anaconda3** – [Download Here](https://www.anaconda.com/download)

## Getting started
1. **Clone PythonConnect Repository** – [GitHub Repo](https://github.com/JonasFeron/PythonConnect)
2. **Set Up Paths in Visual Studio** – Open `PythonConnect.sln` and configure Python paths.
3. **Run the simple example** – Launch the C# console application and see PythonConnect in action.  

## Use Cases
PythonConnect is **ideal for**:
- **Scientific Computing** – Use **NumPy**, **SciPy**, or **Pandas** for complex calculations.
- **Complex Grasshopper Plugins** – Manage multiple components efficiently in **Rhino/Grasshopper**.
- **Performance-Critical Applications** – Avoid reinitializing Python for each script call.

## License
PythonConnect is open-source and available under the **Apache 2.0 License**.

📌 **GitHub Repository**: [PythonConnect on GitHub](https://github.com/JonasFeron/PythonConnect)
📌 **More Details**: see documentation under docs\documentation.pdf

