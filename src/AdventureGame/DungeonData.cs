namespace AdventureGame;

/// <summary>
/// Holds all data parsed from the dungeon file by DungeonLoader.
/// </summary>
public class DungeonData
{
    public Room[,] Dungeon { get; set; } = null!;
    public int ExitRow { get; set; }
    public int ExitCol { get; set; }
    public int GrueRow { get; set; }
    public int GrueCol { get; set; }
    public int PlayerStartRow { get; set; }
    public int PlayerStartCol { get; set; }
}
