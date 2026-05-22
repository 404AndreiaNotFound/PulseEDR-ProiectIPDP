using PulseEDR.Application.Detection;
using PulseEDR.Domain.Entities;
using PulseEDR.Domain.Enums;
using PulseEDR.Domain.ValueObjects;

namespace PulseEDR.UnitTests.Application;

public class RiskCalculatorTests
{
    private readonly RiskCalculator _sut = new();

    [Fact]
    public void Calculate_WithNoAlerts_ReturnsZero()
    {
        // Act
        var result = _sut.Calculate(Array.Empty<Alert>());

        // Assert
        result.Should().Be(RiskScore.Zero);
    }

    [Fact]
    public void Calculate_WithSingleMediumAlert_ReturnsMediumScore()
    {
        // Arrange
        var alert = BuildAlert(Severity.Medium, score: 18);

        // Act
        var result = _sut.Calculate(new[] { alert });

        // Assert
        result.Value.Should().Be(18);
    }

    [Fact]
    public void Calculate_WithMultipleAlerts_SumsAndClampsToHundred()
    {
        // Arrange
        var alerts = new[]
        {
            BuildAlert(Severity.High, score: 30),
            BuildAlert(Severity.High, score: 30),
            BuildAlert(Severity.Medium, score: 25),
            BuildAlert(Severity.Medium, score: 25),
            BuildAlert(Severity.Low, score: 10)
        };

        // Act
        var result = _sut.Calculate(alerts);

        // Assert
        result.Value.Should().Be(100);          // clamped
        result.Severity.Should().Be(Severity.Critical);
    }

    [Fact]
    public void Calculate_WithTwoCriticalAlerts_AppliesBoost()
    {
        // Arrange — 2 criticals, raw sum = 60, +5 boost
        var alerts = new[]
        {
            BuildAlert(Severity.Critical, score: 30),
            BuildAlert(Severity.Critical, score: 30)
        };

        // Act
        var result = _sut.Calculate(alerts);

        // Assert
        result.Value.Should().Be(65);
    }

    private static Alert BuildAlert(Severity severity, int score) =>
        new(
            category: AlertCategory.Other,
            severity: severity,
            score: score,
            title: "test",
            description: "test",
            remediation: "test",
            evidence: Evidence.Empty("TestDetector"));
}