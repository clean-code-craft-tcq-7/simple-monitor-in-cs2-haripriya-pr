namespace checkerlib;
using System;
using System.Diagnostics;

public class Checker
{
    public static void DisplayAlert(string message)
    {
        Console.WriteLine(message);
        for (int i = 0; i < 6; i++)
        {
            Console.Write("\r* ");
            System.Threading.Thread.Sleep(1000);
            Console.Write("\r *");
            System.Threading.Thread.Sleep(1000);
        }
    }

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

    public static bool AlertNotInRange(string alertMsg, float reading, float? lowerLimit, float? upperLimit)
    {
        if (IsGreaterThan(reading, upperLimit) || IsLesserThan(reading, lowerLimit))
        {
            DisplayAlert(alertMsg);
            return false;
        }
        return true;
    }


    public static bool VitalsOk(float temperature, int pulseRate, int spo2)
    {
        return AlertNotInRange("Temperature out of range", temperature, 95, 102) 
               && AlertNotInRange("Pulse Rate is out of range", pulseRate, 60, 100)
               && AlertNotInRange("Oxygen Saturation out of range", spo2, 90, null);
    }
}