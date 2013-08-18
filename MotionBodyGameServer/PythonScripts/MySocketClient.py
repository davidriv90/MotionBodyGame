﻿import socket
import sys

HOST, PORT = "localhost", 4321
# data = " ".join(sys.argv[1:])
data = "Salut!! Le Avatar demande des données pour effectuer des movements!!"

# Create a socket (SOCK_STREAM means a TCP socket)
sock = socket.socket(socket.AF_INET, socket.SOCK_STREAM)

try:
   # Connect to server and send data
   sock.connect((HOST, PORT))
   
   sock.sendall(data + "\n")
   print "Sent:     {}".format(data)
   
   # Receive data from the server and shut down
   received = "";
   while True:
      received = sock.recv(1024)
      print received
finally:
    sock.close()