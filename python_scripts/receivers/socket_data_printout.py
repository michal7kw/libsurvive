import socket

HOST = 'localhost'
PORT = 12345

server_socket = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
server_socket.setsockopt(socket.SOL_SOCKET, socket.SO_REUSEADDR, 1)
server_socket.bind((HOST, PORT))

server_socket.listen(1)
print(f"Server listening on {HOST}:{PORT}")

client_socket, address = server_socket.accept()
print(f"Connected to client: {address}")

while True:
    data = client_socket.recv(1024).decode('utf-8')
    if not data:
        break
    print(f"Received data: {data.strip()}")

client_socket.close()
server_socket.close()