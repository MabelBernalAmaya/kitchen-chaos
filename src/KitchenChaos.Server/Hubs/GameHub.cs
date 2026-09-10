using KitchenChaos.Server.Services;
using Microsoft.AspNetCore.SignalR;

namespace KitchenChaos.Server.Hubs;

public class GameHub : Hub
{
    private readonly RoomService _roomService;

    public GameHub(RoomService roomService)
    {
        _roomService = roomService;
    }

    // AB#4 - Crear sala de juego
    public async Task<string> CreateRoom(string playerName)
    {
        var room = _roomService.CreateRoom(Context.ConnectionId, playerName);
        await Groups.AddToGroupAsync(Context.ConnectionId, room.Code);

        var playerNames = room.Players.Select(p => p.Name).ToList();
        await Clients.Group(room.Code).SendAsync("RoomPlayersUpdated", playerNames);

        return room.Code;
    }

    // AB#5 - Unirse a sala con código
    public async Task JoinRoom(string roomCode, string playerName)
    {
        var result = _roomService.JoinRoom(roomCode, Context.ConnectionId, playerName);

        if (!result.Success)
        {
            await Clients.Caller.SendAsync("JoinRoomError", result.Error);
            return;
        }

        await Groups.AddToGroupAsync(Context.ConnectionId, roomCode);

        var playerNames = result.Room!.Players.Select(p => p.Name).ToList();
        await Clients.Group(roomCode).SendAsync("RoomPlayersUpdated", playerNames);
    }

    public async Task SendMessage(string user, string message)
    {
        await Clients.All.SendAsync("ReceiveMessage", user, message);
    }
}
