using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Utilites.Config;
using Utilites.Logger;

namespace ExampleUsageOfUtilites
{
    public static class FirstWay
    {
        public static string FirstOption = "First Value";
        public static int SecondOption = 42;
        public static float ThirdOption = 0.73f;
        public static List<string> RegularList = new List<string>()
        {
            "first list value",
            "Second list value"
        };
        public static List<CustomClass> CustomList = new List<CustomClass>()
        {
            new CustomClass("First custom list value",12,67.5f),
            new CustomClass("Second custom list value",64,12.1f),
        };
        public static Dictionary<string,float> RegularDictionary = new Dictionary<string, float>()
        {
            ["first entry of regular dictionary"] = 1f,
            ["Second key of regular dictionary"] = 2f
        };
        public static Dictionary<string, CustomClass> CustomDictionary = new Dictionary<string, CustomClass>()
        {
            ["first entry of regular dictionary"] = new CustomClass("First entry of custom dictionary",6,45f),
            ["Second key of regular dictionary"] = new CustomClass("Second entry of custom dictionary", 8, 9.4f),
        };
        public static void Load()
        {
            //Creating ConfigFile
            ConfigFile config = new ConfigFile("config");
            //Load its data.
            config.Load();
            //Getting the config values, and also checking if the config was changed due to it.
            var configChanged = config.TryGet(ref FirstOption, "First option") |
                                config.TryGet(ref SecondOption, "Second option") |
                                config.TryGet(ref ThirdOption, "Third option") |
                                config.TryGet(ref RegularList, "Regular list example") |
                                config.TryGet(ref CustomList, "Custom list example") |
                                config.TryGet(ref RegularDictionary, "Regular dictionary example") |
                                config.TryGet(ref CustomDictionary, "Custom dictionary example");
            if(configChanged)
                config.Save();
            LogConfig();
        }

        private static void LogConfig()
        {
            Logger.Info($"First option: {FirstOption}");
            Logger.Info($"Second option: {SecondOption}");
            Logger.Info($"Second option: {ThirdOption}");
            Logger.Info($"Regular list values:\n\t{string.Join("\n\t",RegularList.ToArray())}");
            Logger.Info($"Custom list values:\n\t{string.Join("\n\t", CustomList.Select(x => x.ToString("\n\t")).ToArray())}");
            Logger.Info($"Regular dictionary values: \n\t{string.Join("\n\t",RegularDictionary.Select(x => $"{x.Key}: {x.Value}").ToArray())}");
            Logger.Info($"Custom dictionary values: \n\t{string.Join("\n\t", CustomDictionary.Select(x => $"{x.Key}:\n\t\t{x.Value.ToString("\n\t\t")}").ToArray())}");
        }
        public class CustomClass
        {
            public string Name;
            public int Value;
            public float FloatValue;

            public CustomClass(string name, int value, float floatValue)
            {
                Name = name;
                Value = value;
                FloatValue = floatValue;
            }

            public CustomClass()
            {
                //This method SHOULD exists in oreder to being readable.
            }

            public string ToString(string Seperator) => $"Name: {Name}{Seperator}Value = {Value}{Seperator}FloatValue = {FloatValue}";

        }
    }
}
