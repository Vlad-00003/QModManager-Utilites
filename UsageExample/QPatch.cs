using System;
using Utilites.Logger;

namespace ExampleUsageOfUtilites
{
    public class QPatch
    {
        public static void Patch()
        {
            Logger.ClearCustomLog();
            try
            {
                Logger.Warning("Beggining first test. First way of handling config.");
                FirstWay.Load();
                Logger.Warning("End of the first test.");
                Logger.Warning("Beggining second test. Second way of handling config.");
                SecondWay.Load();
                Logger.Warning("End of the second test.");
                //typeof(SubControl).LogAllConstructors();
                //typeof(SubControl).LogAllMethods();
                //typeof(SubControl).LogAllFields();
                //HarmonyInstance harmony = HarmonyInstance.Create("com.waterfilteroverflow.mod");
                //harmony.PatchAll(Assembly.GetExecutingAssembly());
            }
            catch (Exception e) { e.Log();}
        }
    }
}
