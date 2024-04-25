import asyncio
import websockets
import json

async def process_data(websocket):
    async for message in websocket:
        data = message.strip().split()
        print (data)
async def main():
    async with websockets.connect('ws://localhost:8080/ws') as websocket:
        await process_data(websocket)

asyncio.run(main())