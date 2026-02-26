using System;
using System.Reflection;
using NuclearOption.SavedMission;
using UnityEngine;
using Random = System.Random;


namespace GW_server_plugin.Features
{
    /// <summary>
    ///    A service to make the weather be random at mission start
    /// </summary>
    public static class WeatherRandomizerService
    {
        /// <summary>
        ///     Randomizes the weather in a json mission string.
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        public static string RandomizeWeather(string json)
        {
            var mission = JsonUtility.FromJson<Mission>(json);
            if (mission == null) return json;

            var rnd = new Random();

            var env = GetFieldOrProperty(mission, "environment");
            if (env == null)
            {
                var envMemberType = GetFieldOrPropertyType(mission.GetType(), "environment");
                if (envMemberType == null) return json;

                env = Activator.CreateInstance(envMemberType);
                SetFieldOrProperty(mission, "environment", env);
            }

            SetFieldOrProperty(env, "timeOfDay", rnd.Next(5, 18));
            SetFieldOrProperty(env, "timeFactor", 8.0f);
            SetFieldOrProperty(env, "weatherIntensity", (float)(rnd.NextDouble() * 0.8));
            SetFieldOrProperty(env, "cloudAltitude", (float)(500 + rnd.NextDouble() * 1000));
            SetFieldOrProperty(env, "windSpeed", (float)(rnd.NextDouble() * 4));
            SetFieldOrProperty(env, "windTurbulence", (float)(rnd.NextDouble() * 1));
            SetFieldOrProperty(env, "windHeading", rnd.Next(0, 360));

            return JsonUtility.ToJson(mission);
        }

        private static object? GetFieldOrProperty(object obj, string name)
        {
            var t = obj.GetType();
            var f = t.GetField(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (f != null) return f.GetValue(obj);

            var p = t.GetProperty(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (p != null) return p.GetValue(obj, null);

            return null;
        }

        private static Type? GetFieldOrPropertyType(Type t, string name)
        {
            var f = t.GetField(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (f != null) return f.FieldType;

            var p = t.GetProperty(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (p != null) return p.PropertyType;

            return null;
        }

        private static void SetFieldOrProperty(object obj, string name, object value)
        {
            var t = obj.GetType();

            var f = t.GetField(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (f != null)
            {
                f.SetValue(obj, ConvertTo(value, f.FieldType));
                return;
            }

            var p = t.GetProperty(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (p != null && p.CanWrite)
            {
                p.SetValue(obj, ConvertTo(value, p.PropertyType), null);
            }
        }

        private static object ConvertTo(object value, Type targetType)
        {
            if (targetType.IsInstanceOfType(value)) return value;

            if (targetType == typeof(float)) return Convert.ToSingle(value);
            if (targetType == typeof(double)) return Convert.ToDouble(value);
            if (targetType == typeof(int)) return Convert.ToInt32(value);

            return value;
        }
    }
}
