using System.Collections.Concurrent;
using KitchenChaos.Server.Models;

namespace KitchenChaos.Server.Services;

public class JoinRoomResult
{
    public bool Success { get; set; }
    public string? Error { get; set; }
    public Room? Room { get; set; }
}

public class RoomService
{
    private const int MaxPlayers = 4;

    // ConcurrentDictionary porque varias salas pueden crearse/consultarse al mismo tiempo
    // desde jugadores distintos, sin que un hilo pise el diccionario de otro.
    private readonly ConcurrentDictionary<string, Room> _rooms = new();

    public Room CreateRoom(string connectionId, string playerName)
    {
        var code = GenerateUniqueCode();
        var room = new Room { Code = code };
        room.Players.Add(new Player { ConnectionId = connectionId, Name = playerName });

        _rooms[code] = room;
        return room;
    }

    public JoinRoomResult JoinRoom(string code, string connectionId, string playerName)
    {
        if (!_rooms.TryGetValue(code, out var room))
        {
            return new JoinRoomResult { Success = false, Error = "El código de sala no existe." };
        }

        // Dos jugadores pueden intentar unirse a la misma sala en el mismo instante.
        // Este lock evita que ambos pasen la validación de cupo y la sala quede con 5 jugadores.
        lock (room)
        {
            if (room.Players.Count >= MaxPlayers)
            {
                return new JoinRoomResult { Success = false, Error = "La sala ya está llena (4/4)." };
            }

            room.Players.Add(new Player { ConnectionId = connectionId, Name = playerName });
        }

        return new JoinRoomResult { Success = true, Room = room };
    }

    private string GenerateUniqueCode()
    {
        string code;
        do
        {
            code = Random.Shared.Next(1000, 9999).ToString();
        } while (_rooms.ContainsKey(code));

        return code;
    }
}
