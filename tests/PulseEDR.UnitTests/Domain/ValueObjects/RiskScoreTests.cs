using PulseEDR.Domain.Enums;
using PulseEDR.Domain.ValueObjects;

namespace PulseEDR.UnitTests.Domain.ValueObjects;

/// <summary>
/// Unit tests for the RiskScore value object.
/// Demonstrates the AAA pattern (Arrange-Act-Assert) and FluentAssertions usage.
///
/// Additional tests for RiskScore + other entities/VOs should follow this template.
/// </summary>
public class RiskScoreTests
{
    [Theory]
    [InlineData(0, Severity.None)]
    [InlineData(19, Severity.None)]
    [InlineData(20, Severity.Low)]
    [InlineData(39, Severity.Low)]
    [InlineData(40, Severity.Medium)]
    [InlineData(59, Severity.Medium)]
    [InlineData(60, Severity.High)]
    [InlineData(79, Severity.High)]
    [InlineData(80, Severity.Critical)]
    [InlineData(100, Severity.Critical)]
    public void Constructor_WithValueInRange_DerivesExpectedSeverity(
        int value, Severity expected)
    {
        // Act
        var score = new RiskScore(value);

        // Assert
        score.Value.Should().Be(value);
        score.Severity.Should().Be(expected);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(101)]
    [InlineData(int.MaxValue)]
    [InlineData(int.MinValue)]
    public void Constructor_WithOutOfRangeValue_ThrowsArgumentOutOfRange(int value)
    {
        // Act
        Action act = () => new RiskScore(value);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void Zero_ReturnsScoreWithValueZeroAndSeverityNone()
    {
        // Act
        var score = RiskScore.Zero;

        // Assert
        score.Value.Should().Be(0);
        score.Severity.Should().Be(Severity.None);
    }

    [Fact]
    public void Max_ReturnsScoreWithValueOneHundredAndSeverityCritical()
    {
        // Act
        var score = RiskScore.Max;

        // Assert
        score.Value.Should().Be(100);
        score.Severity.Should().Be(Severity.Critical);
    }

    [Fact]
    public void ToString_ReturnsHumanReadableRepresentation()
    {
        // Arrange
        var score = new RiskScore(75);

        // Act
        var result = score.ToString();

        // Assert
        result.Should().Be("75 (High)");
    }

    [Fact]
    public void TwoScoresWithSameValue_AreEqual()
    {
        // Arrange (records have value equality)
        var a = new RiskScore(50);
        var b = new RiskScore(50);

        // Assert
        a.Should().Be(b);
    }
}