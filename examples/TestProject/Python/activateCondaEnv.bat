@echo off
::argument %~1 = path\to\activate.bat = script to activate Conda base environment
::argument %~2 = environment name = name of Conda environment to activate
::argument %~3 = success message = Message to display if activation is successful
::argument %~4 = error message = Message to display if the Base Conda environment fails to activate
::argument %~5 = error message = Message to display if the Other Conda environment fails to activate

:: Activate Conda base
call %~1
if errorlevel 1 (
    :: send error message to stderr
    echo %4 >&2 
    exit /b 1
)

:: Check if the environment Name is "base"
if /i "%~2"=="base" (
    :: send success message to stdout    
    echo %3 
    exit /b 0
)

:: Attempt to activate Other environment name
call conda activate %~2
if errorlevel 1 (
    :: send error message to stderr
    echo %5 >&2 
    exit /b 2
)

:: Output success feedback
echo %3
exit /b 0