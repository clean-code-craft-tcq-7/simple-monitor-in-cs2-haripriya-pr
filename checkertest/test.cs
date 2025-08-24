namespace checkertest;

using checkerlib;
using Moq;
using Xunit;

public class CheckerTests
{
    private Mock<ICheckerDisplay> mockDisplay = new Mock<ICheckerDisplay>();
    private Checker? Checker;
    private Checker InitMockDisplay()
    {
        mockDisplay = new Mock<ICheckerDisplay>();
        Checker = new Checker(mockDisplay.Object);
        return Checker;
    }

    private void VerifyMockDisplayWithMsg(Times times, string alertMsg)
    {
        mockDisplay.Verify(d => d.DisplayAlert(alertMsg), times);
    }

    private void VerifyMockDisplayWithAny(Times times)
    {
        mockDisplay.Verify(d => d.DisplayAlert(It.IsAny<string>()), times);
    }

    [Fact]
    public void NotOkWhenAnyVitalIsOffRange()
    {
        Assert.True(InitMockDisplay().VitalsOk(98.1f, 70, 98)); //All okay
        VerifyMockDisplayWithAny(Times.Never());

        Assert.False(InitMockDisplay().VitalsOk(94f, 75, 100)); //Temp low limit
        VerifyMockDisplayWithMsg(Times.Once(), "Temperature out of range");

        Assert.False(InitMockDisplay().VitalsOk(104f, 75, 100)); //Temp high limit
        VerifyMockDisplayWithMsg(Times.Once(), "Temperature out of range");

        Assert.False(InitMockDisplay().VitalsOk(98f, 59, 100)); //Pulse low limit
        VerifyMockDisplayWithMsg(Times.Once(), "Pulse Rate is out of range");

        Assert.False(InitMockDisplay().VitalsOk(98f, 103, 100)); //Pulse high limit
        VerifyMockDisplayWithMsg(Times.Once(), "Pulse Rate is out of range");

        Assert.False(InitMockDisplay().VitalsOk(98f, 75, 89)); //SPO2 low limit
        VerifyMockDisplayWithMsg(Times.Once(), "Oxygen Saturation out of range");

        Assert.False(InitMockDisplay().VitalsOk(94.5f, 102, 100)); //Pulse & Temp out of range
        VerifyMockDisplayWithAny(Times.Once());

        Assert.False(InitMockDisplay().VitalsOk(93f, 75, 85)); //Temp & SPO2 out of range
        VerifyMockDisplayWithAny(Times.Once());

        Assert.False(InitMockDisplay().VitalsOk(98f, 59, 88)); //Pulse & SPO2 out of range
        VerifyMockDisplayWithAny(Times.Once());

        Assert.False(InitMockDisplay().VitalsOk(103f, 59, 87)); //All out of range
        VerifyMockDisplayWithAny(Times.Once());

        Assert.True(InitMockDisplay().AlertNotInRange("Temperature out of range", 98, 95, 102)); // Normal temperature
        VerifyMockDisplayWithAny(Times.Never());

        Assert.False(InitMockDisplay().AlertNotInRange("Temperature out of range", 103, 95, 102)); //Temperature out of range
        VerifyMockDisplayWithMsg(Times.Once(), "Temperature out of range");

        Assert.True(InitMockDisplay().AlertNotInRange("Temperature out of range", 98, null, 102)); //No temperature lower limit to test IsGreaterThan()
        VerifyMockDisplayWithAny(Times.Never());
    }
}