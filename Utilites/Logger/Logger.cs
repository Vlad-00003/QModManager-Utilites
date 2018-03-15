using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Harmony;

namespace Utilites.Logger
{
    #region Log levels and types.
    /// <summary>
    /// Determinies the type of the log. Currently this is nothig but a prefix before the message.
    /// </summary>
    public enum LogLevel
    {
        /// <summary>
        /// Just an information message.
        /// </summary>
        Info,
        /// <summary>
        /// Something goes wrong, but we can handle it.
        /// </summary>
        Warning,
        /// <summary>
        /// The error has occur.
        /// </summary>
        Error,
        /// <summary>
        /// Debug messages that should be removed in the relized versions.
        /// </summary>
        Debug
    }

    /// <summary>
    /// Determines where the log would be stored
    /// </summary>
    [Flags]
    public enum LogType
    {
        /// <summary>
        /// Placeholder. If set - Custom LogType would be used
        /// </summary>
        None = 0,
        /// <summary>
        /// Print the information to the custom logfile located in the mod folder.
        /// </summary>
        Custom = 1,
        /// <summary>
        /// Print the information to the file "harmony.log.txt" located at the desktop.
        /// </summary>
        Harmony = 2,
        /// <summary>
        /// Print the information to the "output_log.txt".
        /// </summary>
        Console = 4,
        /// <summary>
        /// Print the information on the player screen
        /// </summary>
        PlayerScreen = 8
    }
    #endregion

    /// <summary>
    /// Main class that allows you to log anything way simplier
    /// </summary>
    public static class Logger
    {
        private static readonly string Logpath = Environment.CurrentDirectory + @"\QMods\{0}\log.txt";

        /// <summary>
        /// Created [Debug] prefixed message in the desired location
        /// </summary>
        /// <param name="text"></param>
        /// <param name="type"></param>
        public static void Debug(object text, LogType type = LogType.Custom) =>
            Log(text.ToString(), LogLevel.Debug, type, Assembly.GetCallingAssembly().GetName().Name);

        /// <summary>
        /// Creates [Error] prefixes message in the desired location
        /// </summary>
        /// <param name="text"></param>
        /// <param name="type"></param>
        public static void Error(object text, LogType type = LogType.Custom) =>
            Log(text.ToString(), LogLevel.Error, type, Assembly.GetCallingAssembly().GetName().Name);

        /// <summary>
        /// Creates [Info] prefixed message in the desired loaction
        /// </summary>
        /// <param name="text"></param>
        /// <param name="type"></param>
        public static void Info(object text, LogType type = LogType.Custom) =>
            Log(text.ToString(), LogLevel.Info, type, Assembly.GetCallingAssembly().GetName().Name);

        /// <summary>
        /// Creates [Warning] prefixed message in the desired location
        /// </summary>
        /// <param name="text"></param>
        /// <param name="type"></param>
        public static void Warning(object text, LogType type = LogType.Custom) =>
            Log(text.ToString(), LogLevel.Warning, type, Assembly.GetCallingAssembly().GetName().Name);

        /// <summary>
        /// Creates [Info] prefixed message in the desired loaction
        /// </summary>
        /// <param name="text"></param>
        /// <param name="type"></param>
        public static void Log(object text, LogType type = LogType.Custom) => Info(text, type);

        internal static void Log(string text, LogLevel level, LogType type, string caller)
        {
            if (type.Contains(LogType.None) || type.Contains(LogType.Custom))
                AddToFile(caller, $"{DateTime.Now.ToShortTimeString()} [{caller}] [{level:f}] {text}");

            if(type.Contains(LogType.Harmony))
                FileLog.Log($"{DateTime.Now.ToShortTimeString()} [{caller}] [{level:f}] {text}");

            if(type.Contains(LogType.Console))
                Console.WriteLine($"[{caller}] [{level:f}] {text}]");

            if(type.Contains(LogType.PlayerScreen))
                ErrorMessage.AddDebug($"{DateTime.Now.ToShortTimeString()} [{caller}] [{level:f}] {text}");
        }

        #region Exceptions

        /// <summary>
        /// Logs the formatted exception as an error in the desired location
        /// </summary>
        /// <param name="e"></param>
        /// <param name="logType"></param>
        public static void Log(this Exception e, LogType logType = LogType.Custom) =>
            Log(FormatException(e), LogLevel.Debug, logType, Assembly.GetCallingAssembly().GetName().Name);

        private static string FormatException(Exception e)
        {
            if (e == null)
                return string.Empty;
            return $"\"Exception: {e.GetType()}\"\n\tMessage: {e.Message}\n\tStacktrace: {e.StackTrace}\n" +
                   FormatException(e.InnerException);
        }
        #endregion

        #region Harmony
        /// <summary>
        /// Logs list of CodeInstruction as the debug message in the desired location
        /// </summary>
        /// <param name="instructions"></param>
        /// <param name="logType"></param>
        public static void Log(this IEnumerable<CodeInstruction> instructions, LogType logType = LogType.Custom)
        {
            Log($"Logging instuctions", LogLevel.Debug, logType, Assembly.GetCallingAssembly().GetName().Name);
            var codes = new List<CodeInstruction>(instructions);
            for (int i = 0; i < codes.Count; i++)
            {
                Log($"[{i}] {codes[i]}", LogLevel.Debug, logType, Assembly.GetCallingAssembly().GetName().Name);
            }
            Log($"End of instuctions log", LogLevel.Debug, logType, Assembly.GetCallingAssembly().GetName().Name);
        }

        /// <summary>
        /// Logs all patches applied to method
        /// </summary>
        /// <param name="method"></param>
        /// <param name="harmony"></param>
        /// <param name="logType"></param>
        public static void LogPatches(this MethodBase method, HarmonyInstance harmony, LogType logType = LogType.Custom)
        {
            var caller = Assembly.GetCallingAssembly().GetName().Name;
            var patches = harmony.IsPatched(method);
            if (patches == null)
            {
                Log($"Method \"{method}\" is not patched!", LogLevel.Debug, logType, caller);
                return;
            }
            Log("Logging Prefixes...", LogLevel.Debug, logType, caller);
            foreach (var patch in patches.Prefixes)
            {
                Log($"Patch {patch.index}:\n\tOwner: {patch.owner}\n\tPatched method: {patch.patch}\n\tPriority: {patch.priority}\n\tBefore: {patch.before}\n\tAfter:{patch.after}", LogLevel.Debug, logType,caller);
            }
            Log("Logging Postfixes...", LogLevel.Debug, logType, caller);
            foreach (var patch in patches.Postfixes)
            {
                Log($"Patch {patch.index}:\n\tOwner: {patch.owner}\n\tPatched method: {patch.patch}\n\tPriority: {patch.priority}\n\tBefore: {patch.before}\n\tAfter:{patch.after}", LogLevel.Debug, logType, caller);
            }
            Log("Loggind Transpilers...", LogLevel.Debug, logType, caller);
            foreach (var patch in patches.Transpilers)
            {
                Log($"Patch {patch.index}:\n\tOwner: {patch.owner}\n\tPatched method: {patch.patch}\n\tPriority: {patch.priority}\n\tBefore: {patch.before}\n\tAfter:{patch.after}", LogLevel.Debug, logType, caller);
            }
            Log("Done!", LogLevel.Debug, logType, caller);
        }
        #endregion

        #region Helpers
        /// <summary>
        /// Checks if the enum containes value. This is .net 3.5 project, so there is no Enum.HasFlag =(
        /// </summary>
        /// <param name="keys"></param>
        /// <param name="flag"></param>
        /// <returns></returns>
        public static bool Contains(this Enum keys, Enum flag)
        {
            ulong keysVal = Convert.ToUInt64(keys);
            ulong flagVal = Convert.ToUInt64(flag);

            return (keysVal & flagVal) == flagVal;
        }
        private static void AddToFile(string assemblyName, string text)
        {
            var path = string.Format(Logpath, assemblyName);
            using (StreamWriter writer = File.AppendText(path))
            {
                writer.WriteLine(text);
            }
        }

        /// <summary>
        /// Clears custom log file.
        /// </summary>
        public static void ClearCustomLog()
        {
            var path = string.Format(Logpath, Assembly.GetCallingAssembly().GetName().Name);
            File.Delete(path);
        }
        #endregion
    }
}
