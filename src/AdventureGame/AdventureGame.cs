namespace AdventureGame;

public class AdventureGame
{
    // ── Input commands ────────────────────────────────────────────────────────
    public readonly string GO_NORTH  = "W";
    public readonly string GO_SOUTH  = "S";
    public readonly string GO_EAST   = "D";
    public readonly string GO_WEST   = "A";
    public readonly string GET_LAMP  = "L";
    public readonly string GET_KEY   = "K";
    public readonly string OPEN_CHEST = "O";
    public readonly string QUIT      = "Q";

    // ── Core state ────────────────────────────────────────────────────────────
    private Adventurer adventurer = null!;
    private Room[,]    dungeon    = null!;

    // Player position
    private int aRow;
    private int aCol;

    // Exit position (loaded from file)
    private int exitRow;
    private int exitCol;

    // Grue position (loaded from file; moves every turn once chasing)
    private int grueRow;
    private int grueCol;

    // ── Game-state flags ──────────────────────────────────────────────────────
    private bool isChestOpen;     // true once the treasure chest has been opened
    private bool grueChasing;     // true once the chest is open
    private bool gameWon;         // true when player escapes with treasure
    private bool gameLost;        // true when the Grue catches the player
    private bool hasPlayerQuit;
    private bool isAdventureAlive;

    // Tracks the last direction moved (used by the dark-room Grue mechanic)
    private string lastDirection = string.Empty;

    public AdventureGame() { }

    // ── Public entry point ────────────────────────────────────────────────────
    public void Start()
    {
        Init();
        ShowGameStartScreen();

        string input;
        do
        {
            ShowScene();

            do
            {
                ShowInputOptions();
                input = GetInput();
            }
            while (!IsValidInput(input));

            ProcessInput(input);
            UpdateGameState();
        }
        while (!IsGameOver());

        ShowGameOverScreen();
    }

    // ── Initialisation ────────────────────────────────────────────────────────
    private void Init()
    {
        adventurer = new Adventurer();

        // Load the dungeon layout from the resource file.
        // AppContext.BaseDirectory ensures the path resolves correctly whether
        // the game is run directly or from a test runner.
        string filePath = Path.Combine(AppContext.BaseDirectory, "res", "DungeonTemplate.txt");
        DungeonData data = DungeonLoader.Load(filePath);

        dungeon  = data.Dungeon;
        exitRow  = data.ExitRow;
        exitCol  = data.ExitCol;
        grueRow  = data.GrueRow;
        grueCol  = data.GrueCol;
        aRow     = data.PlayerStartRow;
        aCol     = data.PlayerStartCol;

        isChestOpen    = false;
        grueChasing    = false;
        gameWon        = false;
        gameLost       = false;
        hasPlayerQuit  = false;
        isAdventureAlive = true;
        lastDirection  = string.Empty;
    }

    // ── Scene display ─────────────────────────────────────────────────────────
    private void ShowGameStartScreen()
    {
        Console.WriteLine("=== Welcome to Adventure Game! ===");
        Console.WriteLine("Find the KEY, open the CHEST, then escape through the EXIT.");
        Console.WriteLine("Beware the Grue — it lurks in darkness, and once the chest");
        Console.WriteLine("is open it will hunt you down!");
        Console.WriteLine();
    }

    private void ShowScene()
    {
        Room r = dungeon[aRow, aCol];

        bool canSee = adventurer.HasLamp() || r.IsLit();

        if (canSee)
        {
            Console.WriteLine(r.GetDescription());

            // Hint when standing at the exit before the chest is open
            if (aRow == exitRow && aCol == exitCol && !isChestOpen)
                Console.WriteLine("The dungeon exit is right here — but you haven't retrieved the treasure yet!");

            // Hint when standing at the exit after the chest is open
            if (aRow == exitRow && aCol == exitCol && isChestOpen)
                Console.WriteLine("The exit is open! Move north to escape!");
        }
        else
        {
            Console.WriteLine("This room is pitch black!");
        }

        // Atmospheric Grue proximity warnings (only once chasing)
        if (grueChasing)
        {
            int dist = ManhattanDistance(grueRow, grueCol, aRow, aCol);
            if (dist == 1)
                Console.WriteLine("*** You can hear the Grue breathing right next to you! ***");
            else if (dist <= 3)
                Console.WriteLine("You hear a low rumbling growl... The Grue is close!");
        }
    }

    private void ShowInputOptions()
    {
        string options =
            $"GO NORTH [{GO_NORTH}] | GO EAST  [{GO_EAST}]  | GET LAMP  [{GET_LAMP}] | OPEN CHEST [{OPEN_CHEST}]\n"
          + $"GO SOUTH [{GO_SOUTH}] | GO WEST  [{GO_WEST}]  | GET KEY   [{GET_KEY}]  | QUIT       [{QUIT}]\n"
          + "> ";

        Console.Write(options);
    }

    private string GetInput()
        => Console.ReadLine()!.ToUpper();

    private bool IsValidInput(string input)
    {
        string[] valid = { GO_NORTH, GO_SOUTH, GO_EAST, GO_WEST, GET_LAMP, GET_KEY, OPEN_CHEST, QUIT };

        if (!valid.Contains(input))
        {
            Console.WriteLine("ERROR: Invalid input. Please try again.");
            return false;
        }

        return true;
    }

    // ── Input processing ──────────────────────────────────────────────────────
    private void ProcessInput(string input)
    {
        Room r = dungeon[aRow, aCol];

        // ── Original dark-room Grue mechanic (unchanged) ──────────────────────
        // Moving in an unlit room without a lamp (and not retracing the last step)
        // means the Grue gets you instantly.
        if (!adventurer.HasLamp() && !r.IsLit() && input != lastDirection)
        {
            Console.WriteLine("You stumble in the pitch black... and the Grue eats you alive!");
            isAdventureAlive = false;
            gameLost = true;
            return;
        }

        if      (input == GO_NORTH)    GoNorth(r);
        else if (input == GO_SOUTH)    GoSouth(r);
        else if (input == GO_EAST)     GoEast(r);
        else if (input == GO_WEST)     GoWest(r);
        else if (input == GET_LAMP)    GetLamp(r);
        else if (input == GET_KEY)     GetKey(r);
        else if (input == OPEN_CHEST)  OpenChest(r);
        else                           Quit();
    }

    // ── Game-state update (runs every turn after ProcessInput) ────────────────
    private void UpdateGameState()
    {
        // Nothing to do if the player is already out of the game
        if (!isAdventureAlive || gameLost || gameWon || hasPlayerQuit)
            return;

        // Activate the Grue chase the moment the chest is opened
        if (isChestOpen)
            grueChasing = true;

        // Move the Grue one step toward the player each turn (chase mode only)
        if (grueChasing)
            MoveGrue();

        // Check whether the Grue has caught the player (either direction)
        if (grueRow == aRow && grueCol == aCol)
        {
            Console.WriteLine("The Grue lunges from the shadows and catches you! You have been devoured!");
            isAdventureAlive = false;
            gameLost = true;
            return;
        }

        // Win condition: chest opened AND player has reached the exit
        if (isChestOpen && aRow == exitRow && aCol == exitCol)
        {
            gameWon = true;
        }
    }

    // ── Grue movement (greedy: reduces Manhattan distance each turn) ──────────
    private void MoveGrue()
    {
        Room grueRoom = dungeon[grueRow, grueCol];

        int bestDist  = ManhattanDistance(grueRow, grueCol, aRow, aCol);
        int newGrueRow = grueRow;
        int newGrueCol = grueCol;

        // Evaluate every legal direction and pick the one closest to the player
        if (grueRoom.HasNorth())
        {
            int d = ManhattanDistance(grueRow - 1, grueCol, aRow, aCol);
            if (d < bestDist) { bestDist = d; newGrueRow = grueRow - 1; newGrueCol = grueCol; }
        }
        if (grueRoom.HasSouth())
        {
            int d = ManhattanDistance(grueRow + 1, grueCol, aRow, aCol);
            if (d < bestDist) { bestDist = d; newGrueRow = grueRow + 1; newGrueCol = grueCol; }
        }
        if (grueRoom.HasEast())
        {
            int d = ManhattanDistance(grueRow, grueCol + 1, aRow, aCol);
            if (d < bestDist) { bestDist = d; newGrueRow = grueRow; newGrueCol = grueCol + 1; }
        }
        if (grueRoom.HasWest())
        {
            int d = ManhattanDistance(grueRow, grueCol - 1, aRow, aCol);
            if (d < bestDist) { bestDist = d; newGrueRow = grueRow; newGrueCol = grueCol - 1; }
        }

        grueRow = newGrueRow;
        grueCol = newGrueCol;
    }

    private static int ManhattanDistance(int r1, int c1, int r2, int c2)
        => Math.Abs(r1 - r2) + Math.Abs(c1 - c2);

    // ── Game-over predicate ───────────────────────────────────────────────────
    private bool IsGameOver()
        => !isAdventureAlive || gameLost || gameWon || hasPlayerQuit;

    // ── End-screen ────────────────────────────────────────────────────────────
    private void ShowGameOverScreen()
    {
        Console.WriteLine();
        if (gameWon)
        {
            Console.WriteLine("╔══════════════════════════════════════╗");
            Console.WriteLine("║  YOU ESCAPED WITH THE TREASURE!      ║");
            Console.WriteLine("║  The Grue howls in fury behind you.  ║");
            Console.WriteLine("║           YOU WIN!                   ║");
            Console.WriteLine("╚══════════════════════════════════════╝");
        }
        else if (hasPlayerQuit)
        {
            Console.WriteLine("You fled the dungeon empty-handed. The treasure remains lost.");
        }
        else
        {
            Console.WriteLine("╔══════════════════════════════════════╗");
            Console.WriteLine("║  The Grue has claimed another soul.  ║");
            Console.WriteLine("║           GAME OVER                  ║");
            Console.WriteLine("╚══════════════════════════════════════╝");
        }
    }

    // ── Movement helpers ──────────────────────────────────────────────────────
    private void GoNorth(Room r)
    {
        if (r.HasNorth())
        {
            aRow -= 1;
            lastDirection = GO_SOUTH;
        }
        else
        {
            Console.WriteLine("You cannot go north!\a");
        }
    }

    private void GoSouth(Room r)
    {
        if (r.HasSouth())
        {
            aRow += 1;
            lastDirection = GO_NORTH;
        }
        else
        {
            Console.WriteLine("You cannot go south!\a");
        }
    }

    private void GoEast(Room r)
    {
        if (r.HasEast())
        {
            aCol += 1;
            lastDirection = GO_WEST;
        }
        else
        {
            Console.WriteLine("You cannot go east!\a");
        }
    }

    private void GoWest(Room r)
    {
        if (r.HasWest())
        {
            aCol -= 1;
            lastDirection = GO_EAST;
        }
        else
        {
            Console.WriteLine("You cannot go west!\a");
        }
    }

    // ── Item helpers ──────────────────────────────────────────────────────────
    private void GetLamp(Room r)
    {
        if (r.HasLamp())
        {
            Console.WriteLine("You got the lamp! The darkness retreats.");
            adventurer.SetLamp(true);
            r.SetLamp(false);
        }
        else
        {
            Console.WriteLine("There is no lamp in this room.");
        }
    }

    private void GetKey(Room r)
    {
        if (r.HasKey())
        {
            Console.WriteLine("You got the key! Now find the chest.");
            adventurer.SetKey(true);
            r.SetKey(false);
        }
        else
        {
            Console.WriteLine("There is no key in this room.");
        }
    }

    private void OpenChest(Room r)
    {
        if (r.HasChest())
        {
            if (isChestOpen)
            {
                Console.WriteLine("The chest is already open. Run!");
            }
            else if (adventurer.HasKey())
            {
                Console.WriteLine("You unlock the chest and seize the treasure!");
                Console.WriteLine("*** THE GRUE HAS BEEN AWAKENED — RUN FOR THE EXIT! ***");
                isChestOpen = true;
            }
            else
            {
                Console.WriteLine("You do not have the key!");
            }
        }
        else
        {
            Console.WriteLine("There is no chest in this room.");
        }
    }

    private void Quit()
    {
        Console.WriteLine("You quit the game!");
        hasPlayerQuit = true;
    }
}
