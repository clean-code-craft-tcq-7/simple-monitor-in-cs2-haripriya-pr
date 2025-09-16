namespace checkertest;

using checkerlib;
using Moq;
using Xunit;

public class CheckerTests
{
    private Mock<ICheckerDisplay> _mockDisplay = new();
    private Checker? _checker;
    private string _language = "English";
    private Checker InitMockDisplay()
    {
        _mockDisplay = new Mock<ICheckerDisplay>();
        _checker = new Checker(_mockDisplay.Object);
        _checker.language = _language;
        return _checker;
    }

    private void VerifyMockDisplayWithMsg(Times times, string alertMsg)
    {
        _mockDisplay.Verify(d => d.DisplayAlert(alertMsg), times);
    }

    private void VerifyMockDisplayWithAny(Times times)
    {
        _mockDisplay.Verify(d => d.DisplayAlert(It.IsAny<string>()), times);
    }

    [Fact]
    public void NotOkWhenAnyVitalIsOffRange()
    {
        Assert.True(InitMockDisplay().VitalsOk(98.1f, 70, 98)); //All okay
        VerifyMockDisplayWithAny(Times.Never());

        Assert.False(InitMockDisplay().VitalsOk(94f, 75, 100)); //Temp low limit
        VerifyMockDisplayWithMsg(Times.Once(), "Temperature is out of range");

        Assert.False(InitMockDisplay().VitalsOk(104f, 75, 100)); //Temp high limit
        VerifyMockDisplayWithMsg(Times.Once(), "Temperature is out of range");

        Assert.False(InitMockDisplay().VitalsOk(98f, 59, 100)); //Pulse low limit
        VerifyMockDisplayWithMsg(Times.Once(), "PulseRate is out of range");

        Assert.False(InitMockDisplay().VitalsOk(98f, 103, 100)); //Pulse high limit
        VerifyMockDisplayWithMsg(Times.Once(), "PulseRate is out of range");

        Assert.False(InitMockDisplay().VitalsOk(98f, 75, 89)); //SPO2 low limit
        VerifyMockDisplayWithMsg(Times.Once(), "OxygenSaturation is out of range");

        Assert.False(InitMockDisplay().VitalsOk(94.5f, 102, 100)); //Pulse & Temp out of range
        VerifyMockDisplayWithAny(Times.Once());

        Assert.False(InitMockDisplay().VitalsOk(93f, 75, 85)); //Temp & SPO2 out of range
        VerifyMockDisplayWithAny(Times.Once());

        Assert.False(InitMockDisplay().VitalsOk(98f, 59, 88)); //Pulse & SPO2 out of range
        VerifyMockDisplayWithAny(Times.Once());

        Assert.False(InitMockDisplay().VitalsOk(103f, 59, 87)); //All out of range
        VerifyMockDisplayWithAny(Times.Once());

        Assert.True(InitMockDisplay().AlertNotInRange("Temperature", 98, 95, 102)); // Normal temperature
        VerifyMockDisplayWithAny(Times.Never());

        Assert.False(InitMockDisplay().AlertNotInRange("Temperature", 103, 95, 102)); //Temperature out of range
        VerifyMockDisplayWithMsg(Times.Once(), "Temperature is out of range");

        Assert.True(InitMockDisplay().AlertNotInRange("Temperature", 98, null, 102)); //No temperature lower limit to test IsGreaterThan()
        VerifyMockDisplayWithAny(Times.Never());
        
        Vitals vitals = new()
        {
            Temperature = 98.1f,
            PulseRate = 70,
            OxygenSaturation = 98,
            BloodSugar = 100,
            BloodPressure = 100,
            RespiratoryRate = 15
        };
        Assert.True(InitMockDisplay().CheckAllVitals(vitals)); //All okay
        VerifyMockDisplayWithAny(Times.Never());

        vitals.BloodPressure = 200;
        Assert.False(InitMockDisplay().CheckAllVitals(vitals)); //Blood pressure out of range
        VerifyMockDisplayWithMsg(Times.Once(), "BloodPressure is out of range");

        _language = "German";
        Assert.False(InitMockDisplay().CheckAllVitals(vitals));
        VerifyMockDisplayWithMsg(Times.Once(), "Blutdruck ist auﬂerhalb des Bereichs");

        vitals.RespiratoryRate = 50;
        Assert.False(InitMockDisplay().CheckAllVitals(vitals)); //Blood pressure and Respiratory rate out of range
        VerifyMockDisplayWithAny(Times.Once());

        //To build again
    }
}