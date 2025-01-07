import sys

#0) retrieve data from C# through the console

#example
#when we run in the terminal:
#python ASimpleScript.py arg1 arg2 arg3
#this lauch ASimpleScript.py
#with the sys.argv = list [ASimpleScript.py, arg1, arg2, arg3]

# sys.argv[0] returns the name of the script
key = sys.argv[1] #retrieve arg1 sent by C# 
string1 = sys.argv[2] #retrieve arg2 sent by C# 
string2 = sys.argv[3] #retrieve arg3 sent by C# 

#1) A very Simple Python Script
result1 = string1.lower() 
result2 = string2.upper() 

#2) Send the result back to C# through the console
print(key + ":" + result1 + " " + result2) 
# the key is used to ensure that the result is correctly associated with the right data in C#

#See AnotherSimpleScript.py for a more complex example