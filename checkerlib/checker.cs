namespace checkerlib;

using Castle.Core.Internal;
using System;
using System.Diagnostics;

public interface ICheckerDisplay
{
    void DisplayAlert(string message);
}

public class ConsoleDisplay : ICheckerDisplay
{
    public void DisplayAlert(string message)
    {
        Console.WriteLine(message);
        for (int i = 0; i < 6; i++)
        {
            Console.Write("\r* ");
            Thread.Sleep(1000);
            Console.Write("\r *");
            Thread.Sleep(1000);
        }
    }
}

public class Vitals
{
    public float Temperature { get; set; }
    public int PulseRate {  get; set; }
    public int? OxygenSaturation { get; set; }
    public int BloodSugar { get; set; }
    public int BloodPressure { get; set; }
    public int RespiratoryRate { get; set; }
}


public class Checker (ICheckerDisplay display)
{
    private readonly ICheckerDisplay _display = display;
    Vitals lowerLimit = new()
    {
        Temperature = 95,
        PulseRate = 60,
        OxygenSaturation = 90,
        BloodSugar = 70,
        BloodPressure = 90,
        RespiratoryRate = 12
    };
    Vitals upperLimit = new()
    {
        Temperature = 102,
        PulseRate = 100,
        OxygenSaturation = null,
        BloodSugar = 110,
        BloodPressure = 150,
        RespiratoryRate = 20
    };

    private static bool IsGreaterThan(float a, float? b, float toleranceValue = 0.00001f)
    {
        if (b.HasValue)
            return (a - b.Value) > toleranceValue;
        return false;

    }

    private static bool IsLesserThan(float a, float? b, float toleranceValue = 0.00001f)
    {
        if (b.HasValue)
            return (b.Value - a) > toleranceValue;
        return false;
    }

    public bool AlertNotInRange(string alertMsg, float reading, float? lowerLimit, float? upperLimit)
    {
        if (IsGreaterThan(reading, upperLimit) || IsLesserThan(reading, lowerLimit))
        {
            _display.DisplayAlert(alertMsg);
            return false;
        }
        return true;
    }

    public bool VitalsOk(float temperature, int pulseRate, int spo2)
    {
        return AlertNotInRange("Temperature out of range", temperature, 95, 102)
               && AlertNotInRange("Pulse Rate is out of range", pulseRate, 60, 100)
               && AlertNotInRange("Oxygen Saturation out of range", spo2, 90, null);
    }

    public bool CheckAllVitals(Vitals vitals)
    {
        bool result = true;
        vitals.GetType().GetProperties().ToList().ForEach(vital =>
        {
            float vitalValue = (float)vitals.GetType().GetProperty(vital.Name)!.GetValue(vitals)!;
            float? lowerLimitValue = (float?)lowerLimit!.GetType().GetProperty(vital.Name)!.GetValue(lowerLimit);
            float? upperLimitValue = (float?)upperLimit!.GetType().GetProperty(vital.Name)!.GetValue(upperLimit);
            if (!AlertNotInRange($"{vital.Name} is out of range", vitalValue, lowerLimitValue, upperLimitValue))
            {
                result = false;
                return;
            }
        });
        return result;
    }
}