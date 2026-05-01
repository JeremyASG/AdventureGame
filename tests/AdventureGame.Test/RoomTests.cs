using Xunit;
using AdventureGame;

namespace AdventureGame.Tests
{
    public class RoomTests
    {
        [Fact]
        public void Constructor_ShouldInitializeDefaults()
        {
            var room = new Room();

            Assert.False(room.IsLit());
            Assert.False(room.HasLamp());
            Assert.False(room.HasKey());
            Assert.False(room.HasChest());
            Assert.False(room.HasNorth());
            Assert.False(room.HasSouth());
            Assert.False(room.HasEast());
            Assert.False(room.HasWest());
            Assert.Equal(string.Empty, room.GetDescription());
        }

        [Fact]
        public void SetLit_ShouldUpdateValue()
        {
            var room = new Room();

            room.SetLit(true);

            Assert.True(room.IsLit());
        }

        [Fact]
        public void SetLamp_ShouldUpdateValue()
        {
            var room = new Room();

            room.SetLamp(true);

            Assert.True(room.HasLamp());
        }

        [Fact]
        public void SetKey_ShouldUpdateValue()
        {
            var room = new Room();

            room.SetKey(true);

            Assert.True(room.HasKey());
        }

        [Fact]
        public void SetChest_ShouldUpdateValue()
        {
            var room = new Room();

            room.SetChest(true);

            Assert.True(room.HasChest());
        }

        [Fact]
        public void SetNorth_ShouldUpdateValue()
        {
            var room = new Room();

            room.SetNorth(true);

            Assert.True(room.HasNorth());
        }

        [Fact]
        public void SetSouth_ShouldUpdateValue()
        {
            var room = new Room();

            room.SetSouth(true);

            Assert.True(room.HasSouth());
        }

        [Fact]
        public void SetEast_ShouldUpdateValue()
        {
            var room = new Room();

            room.SetEast(true);

            Assert.True(room.HasEast());
        }

        [Fact]
        public void SetWest_ShouldUpdateValue()
        {
            var room = new Room();

            room.SetWest(true);

            Assert.True(room.HasWest());
        }

        [Fact]
        public void SetDescription_ShouldUpdateValue()
        {
            var room = new Room();
            var description = "A dark mysterious room";

            room.SetDescription(description);

            Assert.Equal(description, room.GetDescription());
        }

        [Fact]
        public void ToString_ShouldReturnCorrectFormat()
        {
            var room = new Room();

            room.SetLit(true);
            room.SetLamp(true);
            room.SetKey(true);
            room.SetChest(true);
            room.SetNorth(true);
            room.SetSouth(false);
            room.SetEast(true);
            room.SetWest(false);
            room.SetDescription("Test Room");

            var expected = "Room[isLit=True, hasLamp=True, hasKey=True, hasChest=True, hasNorth=True, hasSouth=False, hasEast=True, hasWest=False, description=Test Room]";

            Assert.Equal(expected, room.ToString());
        }
    }
}