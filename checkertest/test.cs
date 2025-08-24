namespace checkertest;

using Xunit;
using checkerlib;
public class CheckerTests
{
    [Fact]
    public void NotOkWhenAnyVitalIsOffRange()
    {
        Assert.True(Checker.VitalsOk(98.1f, 70, 98)); //All okay
        Assert.False(Checker.VitalsOk(94f, 75, 100)); //Temp low limit
        Assert.False(Checker.VitalsOk(104f, 75, 100)); //Temp high limit
        Assert.False(Checker.VitalsOk(98f, 59, 100)); //Pulse low limit
        Assert.False(Checker.VitalsOk(98f, 103, 100)); //Pulse high limit
        Assert.False(Checker.VitalsOk(98f, 75, 89)); //SPO2 low limit
        Assert.False(Checker.VitalsOk(94.5f, 102, 100)); //Pulse & Temp out of range
        Assert.False(Checker.VitalsOk(93f, 75, 85)); //Temp & SPO2 out of range
        Assert.False(Checker.VitalsOk(98f, 59, 88)); //Pulse & SPO2 out of range
        Assert.False(Checker.VitalsOk(103f, 59, 87)); //All out of range
        Assert.True(Checker.AlertNotInRange("Temperature out of range", 98, 95, 102)); // Normal temperature
        Assert.False(Checker.AlertNotInRange("Temperature out of range", 103, 95, 102)); //Temperature out of range
        Assert.True(Checker.AlertNotInRange("Temperature out of range", 98, null, 102)); //No temperature lower limit to test IsGreaterThan()
    }
}