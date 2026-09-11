namespace KitchenChaos.Server.Models;

public class Player
{
    public required string ConnectionId { get; set; }
    public required string Name { get; set; }
}

public class Room
{
    public required string Code { get; set; }
    public List<Player> Players { get; } = new();
}
