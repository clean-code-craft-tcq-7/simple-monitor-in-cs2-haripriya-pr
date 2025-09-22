namespace checkerlib;

using Castle.Core.Internal;
using System;
using System.Diagnostics;
using System.Reflection;
using System.Text.RegularExpressions;


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
    private string _temperatureString { get; set; } = string.Empty;
    private float _temperatureValue { get; set; }
    Regex tempRegex = new Regex(@"^\d+(\.\d+)?(C|F)$", RegexOptions.IgnoreCase);
    //Temperature in F by default
    public string Temperature { get { return _temperatureValue.ToString(); } 
        set {
            if (tempRegex.IsMatch(value) && !_temperatureString.Equals(value,StringComparison.OrdinalIgnoreCase))
            {
                string numericPart = value.Substring(0, value.Length - 2);
                if(float.TryParse(numericPart,out float parsedValue))
                {
                    if(value.LastOrDefault() == 'C')
                    {
                        parsedValue = (parsedValue * 9 / 5) + 32;
                    }
                    _temperatureValue = parsedValue;
                    _temperatureString = value;
                }
            }
        }
    }

    public int PulseRate {  get; set; }
    public int? OxygenSaturation { get; set; }
    public int BloodSugar { get; set; }
    public int BloodPressure { get; set; }
    public int RespiratoryRate { get; set; }
}

public class VitalLimits
{
    public float VitalValue { get; set; }
    public float? VitalMaximum { get; set; }
    public float? VitalMinimum { get; set; }
}

public class VitalLevels
{
    public string? Low { get; set; }
    public string? High { get; set; }
    public string? Normal { get; set; }
}

public class VitalLanguage
{
    public VitalLevels Temperature { get; set; } = new() { Low = "Hypothermia",High = "Hyperthermia", Normal = "Temperature" };
    public VitalLevels PulseRate { get; set; } = new() { Low = "Bradycardia", High = "Tachycardia", Normal = "Pulse rate" };
    public VitalLevels OxygenSaturation { get; set; } = new() { Low = "Hypoxemia", High = "Hyperoxia", Normal = "Oxygen saturation" };
    public VitalLevels BloodSugar { get; set; } = new() { Low = "Hypoglycemia", High = "Hyperglycemia", Normal = "Blood sugar" };
    public VitalLevels BloodPressure { get; set; } = new() { Low = "Hypotension", High = "Hypertension", Normal = "Blood pressure" };
    public VitalLevels RespiratoryRate { get; set; } = new() { Low = "Bradypnea", High = "Tachypnea", Normal = "Respiratory rate" };
}

public class Checker (ICheckerDisplay display)
{
    public string language = "English";
    private readonly ICheckerDisplay _display = display;
    private readonly Vitals lowerLimit = new()
    {
        Temperature = "95",
        PulseRate = 60,
        OxygenSaturation = 90,
        BloodSugar = 70,
        BloodPressure = 90,
        RespiratoryRate = 12
    };
    private readonly Vitals upperLimit = new()
    {
        Temperature = "102",
        PulseRate = 100,
        OxygenSaturation = null,
        BloodSugar = 110,
        BloodPressure = 150,
        RespiratoryRate = 20
    };
    private readonly VitalLanguage german = new()
    {
        Temperature = new() { Normal = "Temperatur", Low = "Hypothermie",  High = "Hyperthermie" },
        PulseRate = new() { Normal = "Pulsfrequenz", Low = "Bradykardie", High = "Tachykardie" },
        OxygenSaturation = new() { Normal = "Sauerstoffsättigung", Low = "Hypoxämie", High = "Hyperoxie" },
        BloodSugar = new() { Normal = "Blutzucker", Low = "Hypoglykämie", High = "Hyperglykämie" },
        BloodPressure = new() { Normal = "Blutdruck", Low = "Hypotonie", High = "Hypertonie" },
        RespiratoryRate = new() { Normal = "Atemfrequenz", Low = "Bradypnoe", High = "Tachypnoe" }
    };
    private readonly float warningToleranceValue = 0.015f; //1.5%

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

    private static bool IsWithinLowerTolerance(float value, float? lowerTolerance, float currentToleranceValue, float toleranceValue = 0.00001f)
    {
        if(lowerTolerance.HasValue)
            return ((lowerTolerance+currentToleranceValue) - value) > toleranceValue;
        return false;
    }

    private static bool IsWithinUpperTolerance(float value, float? upperTolerance, float currentToleranceValue, float toleranceValue = 0.00001f)
    {
        if(upperTolerance.HasValue)
            return (value - (upperTolerance-currentToleranceValue)) > toleranceValue;
        return false;
    }

    public string TranslateAlertMsg(PropertyInfo languagePropertyInfo, PropertyInfo levelPropertyInfo, string appendResult = "")
    {
        string alertMsg = string.Empty;
        switch (language)
        {
            case "English":
                appendResult += !appendResult.Equals(string.Empty) ? " " : "";
                alertMsg = $"{appendResult}{levelPropertyInfo.GetValue(languagePropertyInfo.GetValue(new VitalLanguage()))}";
                break;
            case "German":
                appendResult = appendResult.Equals("Near") ? "Fast " : "";
                appendResult = appendResult.Equals("Normal") ? "Normal " : "";
                alertMsg = $"{appendResult}{levelPropertyInfo.GetValue(languagePropertyInfo.GetValue(german))}";
                break;
        }
        return alertMsg;
    }

    public bool AlertNotInRange(string propertyName, float reading, float? lowerLimit, float? upperLimit)
    {
        float currentWarningTolerance = 0;
        if (upperLimit != null)
            currentWarningTolerance = (float)upperLimit * warningToleranceValue;
        PropertyInfo languagePropertyInfo = new VitalLanguage().GetType().GetProperty(propertyName)!;
        PropertyInfo lowLevelPropertyInfo = new VitalLevels().GetType().GetProperty("Low")!;
        PropertyInfo highLevelPropertyInfo = new VitalLevels().GetType().GetProperty("High")!;
        PropertyInfo normalLevelPropertyInfo = new VitalLevels().GetType().GetProperty("Normal")!;
        if (IsGreaterThan(reading, upperLimit))
        {
            _display.DisplayAlert(TranslateAlertMsg(languagePropertyInfo,highLevelPropertyInfo));
            return false;
        }
        else if (IsLesserThan(reading, lowerLimit))
        {
            _display.DisplayAlert(TranslateAlertMsg(languagePropertyInfo, lowLevelPropertyInfo));
            return false;
        }
        else if (IsWithinLowerTolerance(reading, lowerLimit, currentWarningTolerance)){
            _display.DisplayAlert(TranslateAlertMsg(languagePropertyInfo, lowLevelPropertyInfo,"Near"));
        }
        else if (IsWithinUpperTolerance(reading, upperLimit, currentWarningTolerance))
        {
            _display.DisplayAlert(TranslateAlertMsg(languagePropertyInfo, highLevelPropertyInfo, "Near"));
        }
        else
        {
            _display.DisplayAlert(TranslateAlertMsg(languagePropertyInfo, normalLevelPropertyInfo, "Normal"));
        }
        return true;
    }

    public bool VitalsOk(float temperature, int pulseRate, int spo2)
    {
        return AlertNotInRange("Temperature", temperature, 95, 102)
               && AlertNotInRange("PulseRate", pulseRate, 60, 100)
               && AlertNotInRange("OxygenSaturation", spo2, 90, null);
    }

    private static List<PropertyInfo> GetAllProperties()
    {
        return [..new Vitals().GetType().GetProperties()];
    }

    private VitalLimits GetCurrentVital(PropertyInfo vital, Vitals vitals)
    {
        return new VitalLimits(){ 
            VitalValue = Convert.ToSingle(vital.GetValue(vitals)!),
            VitalMinimum = vital.GetValue(lowerLimit) != null? Convert.ToSingle(vital.GetValue(lowerLimit)) : null,
            VitalMaximum = vital.GetValue(upperLimit) != null ? Convert.ToSingle(vital.GetValue(upperLimit)) : null
        };
    }

    public bool CheckAllVitals(Vitals vitals)
    {
        foreach ( var vital in GetAllProperties())
        {
            VitalLimits currentVitalValue = GetCurrentVital(vital, vitals);
            if (!AlertNotInRange($"{vital.Name}", currentVitalValue.VitalValue, currentVitalValue.VitalMinimum, currentVitalValue.VitalMaximum))
            {
                return false;
            }
        }
        return true;
    }
}