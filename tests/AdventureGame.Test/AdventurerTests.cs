using Xunit;
using AdventureGame;

namespace AdventureGame.Tests
{
    public class AdventurerTests
    {
        [Fact]
        public void Constructor_ShouldInitializeDefaults()
        {
            var adventurer = new Adventurer();

            Assert.False(adventurer.HasLamp());
            Assert.False(adventurer.HasKey());
        }

        [Fact]
        public void SetLamp_ShouldUpdateValue()
        {
            var adventurer = new Adventurer();

            adventurer.SetLamp(true);

            Assert.True(adventurer.HasLamp());
        }

        [Fact]
        public void SetKey_ShouldUpdateValue()
        {
            var adventurer = new Adventurer();

            adventurer.SetKey(true);

            Assert.True(adventurer.HasKey());
        }

        [Fact]
        public void SetLampAndKey_ShouldBothBeTrue()
        {
            var adventurer = new Adventurer();

            adventurer.SetLamp(true);
            adventurer.SetKey(true);

            Assert.True(adventurer.HasLamp());
            Assert.True(adventurer.HasKey());
        }

        [Fact]
        public void SetValues_ShouldAllowFalseAfterTrue()
        {
            var adventurer = new Adventurer();

            adventurer.SetLamp(true);
            adventurer.SetKey(true);

            adventurer.SetLamp(false);
            adventurer.SetKey(false);

            Assert.False(adventurer.HasLamp());
            Assert.False(adventurer.HasKey());
        }

        [Fact]
        public void ToString_ShouldReturnCorrectFormat_DefaultValues()
        {
            var adventurer = new Adventurer();

            var expected = "Adventurer[hasLamp=False, hasKey=False]";

            Assert.Equal(expected, adventurer.ToString());
        }

        [Fact]
        public void ToString_ShouldReturnCorrectFormat_UpdatedValues()
        {
            var adventurer = new Adventurer();

            adventurer.SetLamp(true);
            adventurer.SetKey(true);

            var expected = "Adventurer[hasLamp=True, hasKey=True]";

            Assert.Equal(expected, adventurer.ToString());
        }
    }
}