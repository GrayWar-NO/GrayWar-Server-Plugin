using BepInEx.Configuration;
using NuclearOption.SavedMission;
using UnityEngine;
using Random = System.Random;

namespace GW_server_plugin.Helpers;

/// <summary>
/// Apply weather randomness to a mission
/// </summary>
public class WeatherRandomizer(ConfigFile config)
{
    private const string WeatherRandomizerSection = "WeatherRandomizer";

    private readonly ConfigEntry<bool> _enable = config.Bind(WeatherRandomizerSection, "Enable", true);
    private readonly ConfigEntry<int> _minTimeOfDay = config.Bind(WeatherRandomizerSection, "Minimum time of day", 2);
    private readonly ConfigEntry<int> _maxTimeOfDay = config.Bind(WeatherRandomizerSection, "Maximum time of day", 2);

    private readonly ConfigEntry<float> _timeFactor = config.Bind(WeatherRandomizerSection, "time factor", 1f,
        "time factor goes from -2 to 3, values are [0, 0.5, 1, 10, 30, 60]");

    private readonly ConfigEntry<float> _weatherIntensityMult = config.Bind(WeatherRandomizerSection,
        "Weather intensity multiplier", 1f, "Multiplier for weather intensity (between 0 and 1). \n" +
                                            "The value of the weather intensity is (random value between 0 and 1)*multiplier + offset. A final value of 0 means clear weather, a final value of 1 means thunderstorm.");

    private readonly ConfigEntry<float> _weatherIntensityOffset = config.Bind(WeatherRandomizerSection,
        "Weather intensity offset", 0f, "Offset for weather intensity (between 0 and 1). \n" +
                                        "The value of the weather intensity is (random value between 0 and 1)*multiplier + offset. A final value of 0 means clear weather, a final value of 1 means thunderstorm.");

    private readonly ConfigEntry<float> _cloudAltMult = config.Bind(WeatherRandomizerSection,
        "Cloud altitude multiplier", 1000f, "Multiplier for cloud altitude. \n" +
                                            "The final value of the cloud altitude is (random value between 0 and 1)*multiplier + offset. Altitude in meters.");

    private readonly ConfigEntry<float> _cloudAltOffset = config.Bind(WeatherRandomizerSection, "Cloud altitude offset",
        200f, "Offset for cloud altitude. \n" +
              "The final value of the cloud altitude is (random value between 0 and 1)*multiplier + offset. Altitude in meters.");

    private readonly ConfigEntry<float>
        _maxWindSpeed = config.Bind(WeatherRandomizerSection, "Maximum wind speed", 10f,
            "Maximum wind speed in meters per second");

    private readonly ConfigEntry<int>
        _maxWindHeading = config.Bind(WeatherRandomizerSection, "Maximum wind heading (exclusive)", 360);
    private readonly ConfigEntry<int>
        _minWindHeading = config.Bind(WeatherRandomizerSection, "Minimum wind heading", 0);

    private readonly ConfigEntry<int>
        _maxWindRandomChange = config.Bind(WeatherRandomizerSection, "Maximum wind random offset (exclusive)", 0);
    private readonly ConfigEntry<int>
        _minWindRandomChange = config.Bind(WeatherRandomizerSection, "Minimum wind random offset", 0);

    
    internal void Apply(ref Mission mission)
    {
        if (!_enable.Value) return;
        var rnd = new Random();
        mission.environment.timeOfDay = rnd.Next(_minTimeOfDay.Value, _maxTimeOfDay.Value);
        mission.environment.timeFactor =
            _timeFactor.Value; // TIME FACTOR goes from -2 to 3, values are [0, 0.5, 1, 10, 30, 60]
        mission.environment.weatherIntensity =
            Mathf.Clamp01((float)rnd.NextDouble() * _weatherIntensityMult.Value + _weatherIntensityOffset.Value);
        mission.environment.cloudAltitude = (float)(_cloudAltOffset.Value + rnd.NextDouble() * _cloudAltMult.Value);
        mission.environment.windSpeed = (float)(rnd.NextDouble() * _maxWindSpeed.Value); // in m/s (10m/s is 36kph)
        mission.environment.windTurbulence = (float)rnd.NextDouble();
        mission.environment.windHeading = rnd.Next(_minWindHeading.Value, _maxWindHeading.Value);
        mission.environment.windRandomHeading = rnd.Next(_minWindRandomChange.Value, _maxWindRandomChange.Value);
    }
}