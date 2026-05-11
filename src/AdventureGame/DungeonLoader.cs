namespace AdventureGame;

public static class DungeonLoader
{
    private const char Wall = '#';

    /// <summary>
    /// Loads the dungeon from the given file and returns a DungeonData object
    /// containing the Room grid and all special-position metadata.
    ///
    /// File format (one integer per line):
    ///   0  rows
    ///   1  cols
    ///   2  exitRow
    ///   3  exitCol
    ///   4  lampRow
    ///   5  lampCol
    ///   6  keyRow
    ///   7  keyCol
    ///   8  chestRow
    ///   9  chestCol
    ///  10  grueRow
    ///  11  grueCol
    ///  12  playerStartRow
    ///  13  playerStartCol
    ///  14 .. 14+rows-1   layout (one row per line, '#' = wall, any other char = room)
    ///  14+rows ..        room descriptions  "litFlag|description text"
    /// </summary>
    public static DungeonData Load(string filePath)
    {
        string[] lines = File.ReadAllLines(filePath);

        int rows         = int.Parse(lines[0]);
        int cols         = int.Parse(lines[1]);
        int exitRow      = int.Parse(lines[2]);
        int exitCol      = int.Parse(lines[3]);
        int lampRow      = int.Parse(lines[4]);
        int lampCol      = int.Parse(lines[5]);
        int keyRow       = int.Parse(lines[6]);
        int keyCol       = int.Parse(lines[7]);
        int chestRow     = int.Parse(lines[8]);
        int chestCol     = int.Parse(lines[9]);
        int grueRow      = int.Parse(lines[10]);
        int grueCol      = int.Parse(lines[11]);
        int playerStartRow = int.Parse(lines[12]);
        int playerStartCol = int.Parse(lines[13]);

        int layoutStart       = 14;
        int descriptionsStart = layoutStart + rows;

        if (lines.Length < descriptionsStart)
            throw new FormatException("File does not contain enough layout rows.");

        Room[,] dungeon = new Room[rows, cols];
        List<(int row, int col)> traversableTiles = new();

        // ── Build the room grid ──────────────────────────────────────────────
        for (int row = 0; row < rows; row++)
        {
            string layoutLine = lines[layoutStart + row];

            if (layoutLine.Length != cols)
                throw new FormatException(
                    $"Layout row {row} must contain exactly {cols} characters.");

            for (int col = 0; col < cols; col++)
            {
                if (layoutLine[col] != Wall)
                {
                    dungeon[row, col] = new Room();
                    traversableTiles.Add((row, col));
                }
            }
        }

        // ── Validate description count ───────────────────────────────────────
        int descriptionCount = lines.Length - descriptionsStart;
        if (descriptionCount != traversableTiles.Count)
            throw new FormatException(
                $"Description count ({descriptionCount}) must match " +
                $"traversable tile count ({traversableTiles.Count}).");

        // ── Assign room properties ────────────────────────────────────────────
        for (int i = 0; i < traversableTiles.Count; i++)
        {
            string[] parts = lines[descriptionsStart + i].Split('|', 2);

            if (parts.Length != 2)
                throw new FormatException(
                    $"Invalid room description line: {lines[descriptionsStart + i]}");

            bool isLit = parts[0] switch
            {
                "1" => true,
                "0" => false,
                _   => throw new FormatException("Room lit value must be 1 or 0.")
            };

            string description = parts[1];

            var (row, col) = traversableTiles[i];
            Room room = dungeon[row, col];

            room.SetLit(isLit);
            room.SetDescription(description);

            room.SetLamp (row == lampRow  && col == lampCol);
            room.SetKey  (row == keyRow   && col == keyCol);
            room.SetChest(row == chestRow && col == chestCol);

            room.SetNorth(IsTraversable(dungeon, row - 1, col));
            room.SetSouth(IsTraversable(dungeon, row + 1, col));
            room.SetEast (IsTraversable(dungeon, row,     col + 1));
            room.SetWest (IsTraversable(dungeon, row,     col - 1));
        }

        // ── Sanity-check key positions ────────────────────────────────────────
        ValidateTraversableTile(dungeon, exitRow,       exitCol,       "exit");
        ValidateTraversableTile(dungeon, playerStartRow, playerStartCol, "player start");
        ValidateTraversableTile(dungeon, grueRow,       grueCol,       "grue");

        return new DungeonData
        {
            Dungeon        = dungeon,
            ExitRow        = exitRow,
            ExitCol        = exitCol,
            GrueRow        = grueRow,
            GrueCol        = grueCol,
            PlayerStartRow = playerStartRow,
            PlayerStartCol = playerStartCol,
        };
    }

    private static bool IsTraversable(Room[,] dungeon, int row, int col)
        => row >= 0
        && row < dungeon.GetLength(0)
        && col >= 0
        && col < dungeon.GetLength(1)
        && dungeon[row, col] != null;

    private static void ValidateTraversableTile(Room[,] dungeon, int row, int col, string name)
    {
        if (!IsTraversable(dungeon, row, col))
            throw new FormatException($"The {name} position ({row},{col}) must be on a traversable tile.");
    }
}
