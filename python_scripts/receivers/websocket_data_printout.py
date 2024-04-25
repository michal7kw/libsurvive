import asyncio
import websockets
import json

async def process_data(websocket):
    while True:
        message = await websocket.recv()
        print(message)

async def main():
    async with websockets.connect('ws://localhost:8080/ws') as websocket:
        await process_data(websocket)

asyncio.run(main())