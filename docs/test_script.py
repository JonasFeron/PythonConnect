# test_script.py
import sys

def main (arg1, arg2):
	print( f"{arg1.lower()} {arg2.upper()}")

if __name__ == '__main__':  
    main(sys.argv[1], sys.argv[2])
