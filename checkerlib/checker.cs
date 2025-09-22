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
    private readonly Regex tempRegex = new(@"^\d+(\.\d+)?(C|F)$", RegexOptions.IgnoreCase);
    //Temperature in F by default
    public string Temperature { get { return _temperatureValue.ToString(); } 
        set {
            if (tempRegex.IsMatch(value) && !_temperatureString.Equals(value,StringComparison.OrdinalIgnoreCase))
            {
                string numericPart = value[..^1];
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
    public VitalLevels Temperature { get; set; } = new() { Low = "Hypothermia",High = "Hyperthermia", Normal = "Normal Temperature" };
    public VitalLevels PulseRate { get; set; } = new() { Low = "Bradycardia", High = "Tachycardia", Normal = "Normal Pulse rate" };
    public VitalLevels OxygenSaturation { get; set; } = new() { Low = "Hypoxemia", High = "Hyperoxia", Normal = "Normal Oxygen saturation" };
    public VitalLevels BloodSugar { get; set; } = new() { Low = "Hypoglycemia", High = "Hyperglycemia", Normal = "Normal Blood sugar" };
    public VitalLevels BloodPressure { get; set; } = new() { Low = "Hypotension", High = "Hypertension", Normal = "Normal Blood pressure" };
    public VitalLevels RespiratoryRate { get; set; } = new() { Low = "Bradypnea", High = "Tachypnea", Normal = "Normal Respiratory rate" };
    public string Warning = "Near ";
}

public class Checker (ICheckerDisplay display)
{
    public string language = "English";
    private readonly ICheckerDisplay _display = display;
    private readonly Vitals lowerLimit = new()
    {
        Temperature = "95f",
        PulseRate = 60,
        OxygenSaturation = 90,
        BloodSugar = 70,
        BloodPressure = 90,
        RespiratoryRate = 12
    };
    private readonly Vitals upperLimit = new()
    {
        Temperature = "102f",
        PulseRate = 100,
        OxygenSaturation = null,
        BloodSugar = 110,
        BloodPressure = 150,
        RespiratoryRate = 20
    };
    private readonly VitalLanguage german = new()
    {
        Temperature = new() { Normal = "Normal Temperatur", Low = "Hypothermie",  High = "Hyperthermie" },
        PulseRate = new() { Normal = "Normal Pulsfrequenz", Low = "Bradykardie", High = "Tachykardie" },
        OxygenSaturation = new() { Normal = "Normal Sauerstoffsättigung", Low = "Hypoxämie", High = "Hyperoxie" },
        BloodSugar = new() { Normal = "Normal Blutzucker", Low = "Hypoglykämie", High = "Hyperglykämie" },
        BloodPressure = new() { Normal = "Normal Blutdruck", Low = "Hypotonie", High = "Hypertonie" },
        RespiratoryRate = new() { Normal = "Normal Atemfrequenz", Low = "Bradypnoe", High = "Tachypnoe" },
        Warning = "Fast "
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

    public (string,VitalLanguage) TranslateAlertMsg(PropertyInfo languagePropertyInfo, PropertyInfo levelPropertyInfo)
    {
        if (language.Equals("German"))
            return ($"{levelPropertyInfo.GetValue(languagePropertyInfo.GetValue(german))}", german);
        else
            return ($"{levelPropertyInfo.GetValue(languagePropertyInfo.GetValue(new VitalLanguage()))}", new VitalLanguage());
    }

    //level: 0 - low; 1 - normal; 2 - high
    public string MapPropertyInfo(string propertyName, int level, bool warning = false)
    {
        PropertyInfo languagePropertyInfo = new VitalLanguage().GetType().GetProperty(propertyName)!;
        PropertyInfo lowLevelPropertyInfo = new VitalLevels().GetType().GetProperty("Low")!;
        PropertyInfo highLevelPropertyInfo = new VitalLevels().GetType().GetProperty("High")!;
        PropertyInfo normalLevelPropertyInfo = new VitalLevels().GetType().GetProperty("Normal")!;
        List<PropertyInfo> levels = [];
        levels.Add(lowLevelPropertyInfo);
        levels.Add(normalLevelPropertyInfo);
        levels.Add(highLevelPropertyInfo);
        var translateMsg = TranslateAlertMsg(languagePropertyInfo, levels.ElementAt(level));
        string alertMsg = translateMsg.Item1;
        if (warning)
        {
            alertMsg = translateMsg.Item2.Warning + alertMsg;
        }
        return alertMsg;
    }

    private float CalculateWarningTolerance(float? upperLimit)
    {
        return upperLimit != null ? (float)upperLimit * warningToleranceValue : 0;
    }

    public bool AlertNotInRange(string propertyName, float reading, float? lowerLimit, float? upperLimit)
    {
        float currentWarningTolerance = CalculateWarningTolerance(upperLimit);
        if (IsGreaterThan(reading, upperLimit))
        {
            _display.DisplayAlert(MapPropertyInfo(propertyName,2));
            return false;
        }
        else if (IsLesserThan(reading, lowerLimit))
        {
            _display.DisplayAlert(MapPropertyInfo(propertyName,0));
            return false;
        }
        AlertInRangeWarning(propertyName, reading, lowerLimit, upperLimit, currentWarningTolerance);
        return true;
    }


    public void AlertInRangeWarning(string propertyName, float reading, float? lowerLimit, float? upperLimit, float currentWarningTolerance = 0)
    {
        if (IsWithinLowerTolerance(reading, lowerLimit, currentWarningTolerance))
        {
            _display.DisplayAlert(MapPropertyInfo(propertyName,0,true));
        }
        else if (IsWithinUpperTolerance(reading, upperLimit, currentWarningTolerance))
        {
            _display.DisplayAlert(MapPropertyInfo(propertyName, 2, true));
        }
        else
        {
            _display.DisplayAlert(MapPropertyInfo(propertyName,1));
        }
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