using KitchenChaos.Server.Services;
using Microsoft.AspNetCore.SignalR;

namespace KitchenChaos.Server.Hubs;

/// <summary>
/// Hub principal de SignalR. Canal de comunicación en tiempo real entre servidor y clientes.
/// </summary>
public class GameHub : Hub
{
    private readonly RoomService _roomService;
    private readonly PlayerProfileService _profileService;

    public GameHub(RoomService roomService, PlayerProfileService profileService)
    {
        _roomService = roomService;
        _profileService = profileService;
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

    /// <summary>
    /// Registra al jugador en la sesión y devuelve su perfil.
    /// Si ya jugó antes y envía su ID previo, reutiliza el perfil existente (AB#13).
    /// Si es nuevo, crea un perfil con ID único (AB#12).
    /// </summary>
    /// <param name="displayName">Nombre visible del jugador.</param>
    /// <param name="existingId">ID previo guardado en el cliente. Null si es la primera vez.</param>
    public async Task RegisterPlayer(string displayName, string? existingId = null)
    {
        var profile = await _profileService.RegisterAsync(displayName, existingId);

        // Solo el cliente que llamó recibe su perfil
        await Clients.Caller.SendAsync("ProfileRegistered", new
        {
            profile.Id,
            profile.DisplayName,
            profile.GamesPlayed
        });
    }

    /// <summary>Reenvía un mensaje a todos los clientes conectados (prueba de conectividad).</summary>
    public async Task SendMessage(string user, string message) =>
        await Clients.All.SendAsync("ReceiveMessage", user, message);
}
