#p4 command module

import os, sys
from P4 import P4, P4Exception

p4 = P4()

def P4Init():
    global p4                       # Create the P4 instance
    #p4.host = 'SHACNG73700CX'
    #p4.port = "1999"
    p4.user = "t_qix"
    p4.client = "t_qix_ACAD"            # Set some environment variables

    try:                             # Catch exceptions with try/except
        p4.connect()                   # Connect to the Perforce Server
        info = p4.run("info")          # Run "p4 info" (returns a dict)
        for key in info[0]:            # and display all key-value pairs
            print key, "=", info[0][key]
    
        #p4.run_edit(r'D:/depot/Branches/U/subdoc/G050/G055U100qix1.txt')
        #p4.run('move',r'D:/depot/Branches/U/subdoc/G050/G055U100qix1.txt',\
        #       r'D:/depot/Branches/U/subdoc/G050/G055U100qix0.txt')
    
    finally:
        #p4.disconnect()
        pass
    '''
    except P4Exception:
        for e in p4.errors:            # Display errors
            print e
    '''
def P4Del(file_path):
    global p4
    #p4.run_edit(file_path)     #not necessary?
    p4.run('delete',file_path)

def P4Rename(file_path, newname):
    global p4
    p4.run_edit(file_path)
    p4.run('move', file_path, newname)


def P4Close():
    global p4
    if p4.connected() == True:
        p4.disconnect()
        print 'close connection'
    else:
        print 'already closed'
