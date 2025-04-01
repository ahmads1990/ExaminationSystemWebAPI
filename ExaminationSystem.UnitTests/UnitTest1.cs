using FluentAssertions;

namespace ExaminationSystem.UnitTests;

public class UnitTest1
{
    [Fact]
    public void Addition_ShouldReturnCorrectSum_Base()
    {
        // Arrange
        int a = 5, b = 3;

        // Act
        int result = a + b;

        // Assert
        Assert.Equal(8, result);
    }

    [Fact]
    public void Addition_ShouldReturnCorrectSum_Fluent()
    {
        // Arrange
        int a = 5, b = 3;

        // Act
        int result = a + b;

        // Assert
        result.Should().Be(8);
    }
}