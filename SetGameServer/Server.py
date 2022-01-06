import socket
import select
from random import randint
from Game import is_set, return_sets, cards
from time import sleep

board = [cards.pop(randint(0, len(cards)-1)) for _ in range(4*3)]
while not list(return_sets(board)):
    board.append(cards.pop(randint(0, len(cards) - 1)))

server = socket.socket()
server.bind(("0.0.0.0", 5552))
server.listen(5)

def handle_response(sock: socket.socket, data):
    global board
    global votes_addcard

    data = data.decode()
    if data == 'state':
        print("State requested")
        sendBoard(sock)

    elif data.startswith('takeset'):
        addr, port = sock.getpeername()

        idx1, idx2, idx3 = data[7:].split()
        name = ip2name[f'{addr}:{port}']
        print("User", f'''"{name}"''', f"took {idx1} {idx2} {idx3}")
        idx1, idx2, idx3 = int(idx1), int(idx2), int(idx3)

        if idx1 < len(board) and idx2 < len(board) and idx3 < len(board) and is_set([board[idx1], board[idx2], board[idx3]]):
            # update score
            ip2score[f"{addr}:{port}"] += 1

            # send all scores
            sendScores()

            # send message
            sleep(0.2)  # Does it work without this?
            sendMessage(f"{name} Got A Set!")

            sleep(0.2)
            data = f"{idx1} {idx2} {idx3}"
            msg = f"{len(data):0>4d}1{data}".encode()
            for s in clients:
                # if s is not sock: # do i want this?
                s.send(msg)

            to_change = sorted([idx1, idx2, idx3], reverse=True)

            for idx in to_change:
                if idx > 11 or len(cards) == 0:
                    del board[idx]
                elif len(cards) == 1:
                    board[idx] = cards.pop()
                else:
                    board[idx] = cards.pop(randint(0, len(cards)-1))

            while not list(return_sets(board)) and len(cards) > 0:
                if len(cards) == 1:
                    board.append(cards.pop())
                else:
                    board.append(cards.pop(randint(0, len(cards)-1)))


            # send new board
            sleep(0.2)
            sendBoard()

    elif data.startswith('changename'):
        name = data[11:]
        print("changename name:", name)
        addr, port = sock.getpeername()
        ip2name[f"{addr}:{port}"] = name
        ip2score[f"{addr}:{port}"] = 0
        # reply with all the players in game
        sleep(0.2)
        sendPlayers()
        sleep(0.2)
        sendScores()
        sleep(0.2)
        sendMessage(f"{name} Joined The Game")

    elif data.startswith("chatmsg"):
        msg = data[8:]
        addr, port = sock.getpeername()
        name = ip2name.get(f"{addr}:{port}", "")

        print(f"CHAT: {name} >>> {msg}")
        sendMessage(f"{name} >>> {msg}")

def sendScores():
    data = '\n'.join(str(z) for z in ip2score.values())
    msg = f"{len(data):0>4d}S{data}".encode()
    for s in clients:
        s.send(msg)

def sendBoard(sock=None):
    data = ' '.join(board)
    msg = f"{len(data):0>4d}B{data}"
    msg = msg.encode()
    if sock:
        sock.send(msg)
    else:
        for s in clients:
            s.send(msg)

def sendPlayers():
    data = '\n'.join(ip2name.values())
    msg = f"{len(data):0>4d}P{data}"
    msg = msg.encode()
    for s in clients:
        s.send(msg)

def sendMessage(data):
    msg = f'{len(data):0>4d}M{data}'

    msg = msg.encode()
    for s in clients:
        s.send(msg)

def disconnectPlayer(sock):
    clients.remove(sock)
    addr = sock.getpeername()
    del ip2name[f"{addr[0]}:{addr[1]}"]
    del ip2score[f"{addr[0]}:{addr[1]}"]
    del sock
    print(f"[-] Connection disconnected from ({addr[0]}:{addr[1]})")

clients = []
ip2name = {}
ip2score = {}

while True:
    rlist, wlist, _ = select.select(clients+[server], clients + [server], [])

    for s in rlist:
        if s is server:
            conn, addr = server.accept()
            print(f"[+] New connection from ({addr[0]}:{addr[1]})")
            clients.append(conn)
        else:
            try:
                headerlen = int(s.recv(4))
            except (ValueError, ConnectionResetError) as e:
                # print("e is bad:", e)
                disconnectPlayer(s)

                # update players
                sendPlayers()
                sleep(0.2)
                sendScores()
            else:
                data = b""
                while len(data) < headerlen:
                    data += s.recv(100)
                handle_response(s, data)
