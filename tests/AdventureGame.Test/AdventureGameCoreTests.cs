using Xunit;
using AdventureGame;
using System.Reflection;

namespace AdventureGame.Tests
{
    public class AdventureGameTests
    {
        private AdventureGame CreateInitializedGame()
        {
            var game = new AdventureGame();

            // Call private Init() using reflection
            var initMethod = typeof(AdventureGame)
                .GetMethod("Init", BindingFlags.NonPublic | BindingFlags.Instance);

            initMethod!.Invoke(game, null);

            return game;
        }

        private object? InvokePrivate(object obj, string methodName, params object[] args)
        {
            var method = obj.GetType()
                .GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Instance);

            return method!.Invoke(obj, args);
        }

        private T GetPrivateField<T>(object obj, string fieldName)
        {
            var field = obj.GetType()
                .GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);

            return (T)field!.GetValue(obj)!;
        }

        // ----------------------------
        // Init Tests
        // ----------------------------

        [Fact]
        public void Init_ShouldSetInitialStateCorrectly()
        {
            var game = CreateInitializedGame();

            Assert.False(GetPrivateField<bool>(game, "isChestOpen"));
            Assert.False(GetPrivateField<bool>(game, "hasPlayerQuit"));
            Assert.True(GetPrivateField<bool>(game, "isAdventureAlive"));

            Assert.Equal(1, GetPrivateField<int>(game, "aRow"));
            Assert.Equal(0, GetPrivateField<int>(game, "aCol"));
        }

        // ----------------------------
        // Movement Tests
        // ----------------------------

        [Fact]
        public void GoNorth_ShouldMovePlayer_WhenPathExists()
        {
            var game = CreateInitializedGame();

            var dungeon = GetPrivateField<Room[,]>(game, "dungeon");
            var room = dungeon[1, 0]; // starting room (r3 has north)

            InvokePrivate(game, "GoNorth", room);

            int newRow = GetPrivateField<int>(game, "aRow");

            Assert.Equal(0, newRow);
        }

        [Fact]
        public void GoWest_ShouldNotMove_WhenNoPath()
        {
            var game = CreateInitializedGame();

            var dungeon = GetPrivateField<Room[,]>(game, "dungeon");
            var room = dungeon[1, 0]; // no west

            int originalCol = GetPrivateField<int>(game, "aCol");

            InvokePrivate(game, "GoWest", room);

            int newCol = GetPrivateField<int>(game, "aCol");

            Assert.Equal(originalCol, newCol);
        }

        // ----------------------------
        // Item Tests
        // ----------------------------

        [Fact]
        public void GetLamp_ShouldGiveLamp_WhenAvailable()
        {
            var game = CreateInitializedGame();

            var dungeon = GetPrivateField<Room[,]>(game, "dungeon");
            var room = dungeon[0, 0]; // r1 has lamp

            InvokePrivate(game, "GetLamp", room);

            var adventurer = GetPrivateField<Adventurer>(game, "adventurer");

            Assert.True(adventurer.HasLamp());
            Assert.False(room.HasLamp());
        }

        [Fact]
        public void GetKey_ShouldGiveKey_WhenAvailable()
        {
            var game = CreateInitializedGame();

            var dungeon = GetPrivateField<Room[,]>(game, "dungeon");
            var room = dungeon[0, 0]; // r1 has key

            InvokePrivate(game, "GetKey", room);

            var adventurer = GetPrivateField<Adventurer>(game, "adventurer");

            Assert.True(adventurer.HasKey());
            Assert.False(room.HasKey());
        }

        // ----------------------------
        // Chest Tests
        // ----------------------------

        [Fact]
        public void OpenChest_ShouldFail_WithoutKey()
        {
            var game = CreateInitializedGame();

            var dungeon = GetPrivateField<Room[,]>(game, "dungeon");
            var room = dungeon[1, 0]; // r3 has chest

            InvokePrivate(game, "OpenChest", room);

            Assert.False(GetPrivateField<bool>(game, "isChestOpen"));
        }

        [Fact]
        public void OpenChest_ShouldSucceed_WithKey()
        {
            var game = CreateInitializedGame();

            var dungeon = GetPrivateField<Room[,]>(game, "dungeon");
            var room = dungeon[1, 0]; // r3 has chest

            var adventurer = GetPrivateField<Adventurer>(game, "adventurer");
            adventurer.SetKey(true);

            InvokePrivate(game, "OpenChest", room);

            Assert.True(GetPrivateField<bool>(game, "isChestOpen"));
        }

        // ----------------------------
        // Quit Test
        // ----------------------------

        [Fact]
        public void Quit_ShouldSetHasPlayerQuit()
        {
            var game = CreateInitializedGame();

            InvokePrivate(game, "Quit");

            Assert.True(GetPrivateField<bool>(game, "hasPlayerQuit"));
        }

        // ----------------------------
        // Game Over Logic
        // ----------------------------

        [Fact]
        public void IsGameOver_ShouldReturnTrue_WhenPlayerQuits()
        {
            var game = CreateInitializedGame();

            InvokePrivate(game, "Quit");

            bool result = (bool)InvokePrivate(game, "IsGameOver")!;

            Assert.True(result);
        }

        [Fact]
        public void IsGameOver_ShouldReturnTrue_WhenChestOpened()
        {
            var game = CreateInitializedGame();

            var dungeon = GetPrivateField<Room[,]>(game, "dungeon");
            var room = dungeon[1, 0];

            var adventurer = GetPrivateField<Adventurer>(game, "adventurer");
            adventurer.SetKey(true);

            InvokePrivate(game, "OpenChest", room);

            bool result = (bool)InvokePrivate(game, "IsGameOver")!;

            Assert.True(result);
        }

        [Fact]
        public void IsGameOver_ShouldReturnFalse_WhenGameContinues()
        {
            var game = CreateInitializedGame();

            bool result = (bool)InvokePrivate(game, "IsGameOver")!;

            Assert.False(result);
        }
    }
}