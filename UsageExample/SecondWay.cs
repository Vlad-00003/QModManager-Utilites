using System;
﻿using Oculus.Newtonsoft.Json;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Text;
using Utilites.Config;
using Utilites.Logger;

namespace ExampleUsageOfUtilites
{
    public static class SecondWay
    {
        private static ConfigFile _config = new ConfigFile("config2");
        public static ModConfig Config;
        [SuppressMessage("ReSharper", "MemberCanBePrivate.Local")]
        public class ModConfig
        {
            [JsonProperty("First value of config")]
            public string FirstValue;
            [JsonProperty("Second value of the config")]
            public int SecondValue;
            [JsonProperty("Some floats to the config. Everybody loves them, right?")]
            public float SomeFloats;
            [JsonProperty("And ofc some doubles")]
            public double SomeDouble;
            [JsonProperty("Example of the list that contains regular values")]
            public List<string> RegularList;
            [JsonProperty("Example of the list that contains custom classes")]
            public List<CustomClass> CustomList;
            [JsonProperty("Example of the dictionary that contains regular classes")]
            public Dictionary<string, int> RegularDict;
            [JsonProperty("Example of the dictionary that contains custom classes")]
            public Dictionary<string, CustomClass> CustomDict;
            //Be aware! Private fields aren't being saved in the config!
            public class CustomClass
            {
                [JsonProperty("Name of this custom class")]
                public string Name;
                [JsonProperty("Count of something")]
                public int Count;
                [JsonProperty("And timer. Why not?")]
                public float Timer;
                public CustomClass(string name, int count, float timer)
                {
                    Name = name;
                    Count = count;
                    Timer = timer;
                }
                public string ToString(string Seperator) => $"Name: {Name}{Seperator}Count = {Count}{Seperator}Timer = {Timer}";
            }

            public static ModConfig DefaultConfig()
            {
                return new ModConfig()
                {
                    FirstValue = "Hey!",
                    SecondValue = 99,
                    SomeFloats = 0.003f,
                    SomeDouble = 1.2d,
                    RegularList = new List<string>() { "How","are","you","?"},
                    CustomList = new List<CustomClass>()
                    {
                        new CustomClass("First",120,0f),
                        new CustomClass("Second",119,0)
                    },
                    RegularDict = new Dictionary<string, int>()
                    {
                        ["Entry one"] = 16,
                        ["Another key"] = 1201
                    },
                    CustomDict = new Dictionary<string, CustomClass>()
                    {
                        ["First custom class"] = new CustomClass("Player 1",65,100f),
                        ["Second custom class"] = new CustomClass("Player 2",1,199f)
                    }
                };
            }
        }

        public static void Load()
        {
            Config = _config.ReadObject(ModConfig.DefaultConfig());
            Logger.Debug($"First value: {Config.FirstValue}");
            Logger.Debug($"Second value: {Config.SecondValue}");
            Logger.Debug($"SomeFloats: {Config.SomeFloats}");
            Logger.Debug($"SomeDouble: {Config.SomeDouble}");
            Logger.Debug($"Regular list values:\n\t{string.Join("\n\t", Config.RegularList.ToArray())}");
            Logger.Debug($"Custom list values:\n\t{string.Join("\n\t", Config.CustomList.Select(x => x.ToString("\n\t")).ToArray())}");
            Logger.Debug($"Regular dictionary values: \n\t{string.Join("\n\t", Config.RegularDict.Select(x => $"{x.Key}: {x.Value}").ToArray())}");
            Logger.Debug($"Custom dictionary values: \n\t{string.Join("\n\t", Config.CustomDict.Select(x => $"{x.Key}:\n\t\t{x.Value.ToString("\n\t\t")}").ToArray())}");
            Logger.Debug("Changing first value to \"Weeeee\"");
            Config.FirstValue = "Weeeee";
            _config.WriteObject(Config);
        }
    }
}
