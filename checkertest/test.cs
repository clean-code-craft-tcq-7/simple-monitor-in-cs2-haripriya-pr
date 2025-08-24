namespace checkertest;

using checkerlib;
using Moq;
using Xunit;

public class CheckerTests
{
    [Fact]
    public void NotOkWhenAnyVitalIsOffRange()
    {
        var mockDisplay = new Mock<ICheckerDisplay>();
        var Checker = new Checker(mockDisplay.Object);
        Assert.True(Checker.VitalsOk(98.1f, 70, 98)); //All okay
        mockDisplay.Verify(d => d.DisplayAlert(It.IsAny<string>()), Times.Never);

        mockDisplay = new Mock<ICheckerDisplay>();
        Checker = new Checker(mockDisplay.Object);
        Assert.False(Checker.VitalsOk(94f, 75, 100)); //Temp low limit
        mockDisplay.Verify(d => d.DisplayAlert("Temperature out of range"), Times.Once);

        mockDisplay = new Mock<ICheckerDisplay>();
        Checker = new Checker(mockDisplay.Object);
        Assert.False(Checker.VitalsOk(104f, 75, 100)); //Temp high limit
        mockDisplay.Verify(d => d.DisplayAlert("Temperature out of range"), Times.Once);

        mockDisplay = new Mock<ICheckerDisplay>();
        Checker = new Checker(mockDisplay.Object);
        Assert.False(Checker.VitalsOk(98f, 59, 100)); //Pulse low limit
        mockDisplay.Verify(d => d.DisplayAlert("Pulse Rate is out of range"), Times.Once);

        mockDisplay = new Mock<ICheckerDisplay>();
        Checker = new Checker(mockDisplay.Object);
        Assert.False(Checker.VitalsOk(98f, 103, 100)); //Pulse high limit
        mockDisplay.Verify(d => d.DisplayAlert("Pulse Rate is out of range"), Times.Once);

        mockDisplay = new Mock<ICheckerDisplay>();
        Checker = new Checker(mockDisplay.Object);
        Assert.False(Checker.VitalsOk(98f, 75, 89)); //SPO2 low limit
        mockDisplay.Verify(d => d.DisplayAlert("Oxygen Saturation out of range"), Times.Once);

        mockDisplay = new Mock<ICheckerDisplay>();
        Checker = new Checker(mockDisplay.Object);
        Assert.False(Checker.VitalsOk(94.5f, 102, 100)); //Pulse & Temp out of range
        mockDisplay.Verify(d => d.DisplayAlert(It.IsAny<string>()), Times.Exactly(2));

        mockDisplay = new Mock<ICheckerDisplay>();
        Checker = new Checker(mockDisplay.Object);
        Assert.False(Checker.VitalsOk(93f, 75, 85)); //Temp & SPO2 out of range
        mockDisplay.Verify(d => d.DisplayAlert(It.IsAny<string>()), Times.Exactly(2));

        mockDisplay = new Mock<ICheckerDisplay>();
        Checker = new Checker(mockDisplay.Object);
        Assert.False(Checker.VitalsOk(98f, 59, 88)); //Pulse & SPO2 out of range
        mockDisplay.Verify(d => d.DisplayAlert(It.IsAny<string>()), Times.Exactly(2));

        mockDisplay = new Mock<ICheckerDisplay>();
        Checker = new Checker(mockDisplay.Object);
        Assert.False(Checker.VitalsOk(103f, 59, 87)); //All out of range
        mockDisplay.Verify(d => d.DisplayAlert(It.IsAny<string>()), Times.Exactly(3));

        mockDisplay = new Mock<ICheckerDisplay>();
        Checker = new Checker(mockDisplay.Object);
        Assert.True(Checker.AlertNotInRange("Temperature out of range", 98, 95, 102)); // Normal temperature
        mockDisplay.Verify(d => d.DisplayAlert(It.IsAny<string>()), Times.Never);

        mockDisplay = new Mock<ICheckerDisplay>();
        Checker = new Checker(mockDisplay.Object);
        Assert.False(Checker.AlertNotInRange("Temperature out of range", 103, 95, 102)); //Temperature out of range
        mockDisplay.Verify(d => d.DisplayAlert("Temperature out of range"), Times.Once);

        mockDisplay = new Mock<ICheckerDisplay>();
        Checker = new Checker(mockDisplay.Object);
        Assert.True(Checker.AlertNotInRange("Temperature out of range", 98, null, 102)); //No temperature lower limit to test IsGreaterThan()
        mockDisplay.Verify(d => d.DisplayAlert(It.IsAny<string>()), Times.Never);
    }
}