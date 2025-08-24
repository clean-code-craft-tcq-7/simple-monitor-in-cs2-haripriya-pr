namespace checkertest;

using checkerlib;
using Moq;
using Xunit;

public class CheckerTests
{
    public Mock<ICheckerDisplay> mockDisplay = new();
    public Checker Checker;
    public void InitMockDisplay()
    {
        mockDisplay = new Mock<ICheckerDisplay>();
        Checker = new Checker(mockDisplay.Object);
    }

    public void VerifyMockDisplayWithMsg(Times times, string alertMsg)
    {
        mockDisplay.Verify(d => d.DisplayAlert(alertMsg), times);
    }

    public void VerifyMockDisplayWithAny(Times times)
    {
        mockDisplay.Verify(d => d.DisplayAlert(It.IsAny<string>()), times);
    }

    [Fact]
    public void NotOkWhenAnyVitalIsOffRange()
    {
        Assert.True(Checker.VitalsOk(98.1f, 70, 98)); //All okay
        VerifyMockDisplayWithAny(Times.Never());

        Assert.False(Checker.VitalsOk(94f, 75, 100)); //Temp low limit
        VerifyMockDisplayWithMsg(Times.Once(), "Temperature out of range");

        Assert.False(Checker.VitalsOk(104f, 75, 100)); //Temp high limit
        VerifyMockDisplayWithMsg(Times.Once(), "Temperature out of range");

        Assert.False(Checker.VitalsOk(98f, 59, 100)); //Pulse low limit
        VerifyMockDisplayWithMsg(Times.Once(), "Pulse Rate is out of range");

        Assert.False(Checker.VitalsOk(98f, 103, 100)); //Pulse high limit
        VerifyMockDisplayWithMsg(Times.Once(), "Pulse Rate is out of range");

        Assert.False(Checker.VitalsOk(98f, 75, 89)); //SPO2 low limit
        VerifyMockDisplayWithMsg(Times.Once(), "Oxygen Saturation out of range");

        Assert.False(Checker.VitalsOk(94.5f, 102, 100)); //Pulse & Temp out of range
        VerifyMockDisplayWithAny(Times.Exactly(2));

        Assert.False(Checker.VitalsOk(93f, 75, 85)); //Temp & SPO2 out of range
        VerifyMockDisplayWithAny(Times.Exactly(2));

        Assert.False(Checker.VitalsOk(98f, 59, 88)); //Pulse & SPO2 out of range
        VerifyMockDisplayWithAny(Times.Exactly(2));

        Assert.False(Checker.VitalsOk(103f, 59, 87)); //All out of range
        VerifyMockDisplayWithAny(Times.Exactly(3));

        Assert.True(Checker.AlertNotInRange("Temperature out of range", 98, 95, 102)); // Normal temperature
        VerifyMockDisplayWithAny(Times.Never());

        Assert.False(Checker.AlertNotInRange("Temperature out of range", 103, 95, 102)); //Temperature out of range
        VerifyMockDisplayWithMsg(Times.Once(), "Temperature out of range");

        Assert.True(Checker.AlertNotInRange("Temperature out of range", 98, null, 102)); //No temperature lower limit to test IsGreaterThan()
        VerifyMockDisplayWithAny(Times.Never());
    }
}