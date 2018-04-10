using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Utilites.Logger
{
    /// <summary>
    /// This class provides additional help for logging methods\constructors and fields
    /// </summary>
    public static class ReflectionUtilites
    {

        private const BindingFlags Flags =
            BindingFlags.Public |
            BindingFlags.NonPublic |
            BindingFlags.Static |
            BindingFlags.Instance |
            BindingFlags.DeclaredOnly;

        #region Fields
        /// <summary>
        /// Returns a list of all Fields existing in a class
        /// </summary>
        /// <param name="type">Type</param>
        /// <returns></returns>
        public static IEnumerable<FieldInfo> GetAllFields(this Type type) =>
            type?.GetFields(Flags).Union(GetAllFields(type.BaseType)) ?? Enumerable.Empty<FieldInfo>();

        /// <summary>
        /// Logs all the fileds existing in a class as debug messages in the desired location
        /// </summary>
        /// <param name="type">Type</param>
        /// <param name="logType">Where it should be logged</param>
        public static void LogAllFields(this Type type, LogType logType = LogType.Custom)
        {
            var caller = Assembly.GetCallingAssembly().GetName().Name;
            Logger.Log("Logging all Fields",LogLevel.Debug,logType, caller);
            foreach (var info in type.GetAllFields())
                Logger.Log($"Field: \"{info}\"\n{info?.Attributes}", LogLevel.Debug, logType, caller);
            Logger.Log("End of Fields", LogLevel.Debug, logType, caller);
        }

        /// <summary>
        /// Logs FieldInfo as debug messages in the desired location
        /// </summary>
        /// <param name="info">FieldInfo</param>
        /// <param name="logType">Where it should be logged</param>
        public static void Log(this FieldInfo info, LogType logType = LogType.Custom) =>
            Logger.Log($"Field: \"{info}\"\n{info?.Attributes}", LogLevel.Debug, logType, Assembly.GetCallingAssembly().GetName().Name);

        /// <summary>
        /// Logs FieldInfo and the value it has on instance as debug messages in the desired location
        /// </summary>
        /// <param name="info">FieldInfo</param>
        /// <param name="instance">Instance</param>
        /// <param name="logType">Where it should be logged</param>
        public static void Log(this FieldInfo info, object instance, LogType logType = LogType.Custom)
        {
            Logger.Log($"Field: \"{info}\". Value: \"{info?.GetValue(instance)}\"\n{info?.Attributes}", LogLevel.Debug, logType, Assembly.GetCallingAssembly().GetName().Name);
        }
        #endregion

        #region Methods

        /// <summary>
        /// Outputs a list of all methods existing in a method
        /// </summary>
        /// <param name="type">Method</param>
        /// <returns></returns>
        public static IEnumerable<MethodInfo> GetAllMethods(this Type type) =>
            type?.GetMethods(Flags).Union(GetAllMethods(type.BaseType)) ?? Enumerable.Empty<MethodInfo>();

        /// <summary>
        /// Logs all methods as debug messages in the desired location
        /// </summary>
        /// <param name="type">Methods</param>
        /// <param name="logType">Where it should be logged</param>
        public static void LogAllMethods(this Type type, LogType logType = LogType.Custom)
        {
            var caller = Assembly.GetCallingAssembly().GetName().Name;
            Logger.Log("Logging all Methods", LogLevel.Debug, logType, caller);
            foreach (var info in type.GetAllMethods())
                Logger.Log($"Methods: \"{info}\"\n{info?.Attributes}", LogLevel.Debug, logType, caller);
            Logger.Log("End of Methods", LogLevel.Debug, logType, caller);
        }

        /// <summary>
        /// Logs MethodInfo as debug messages in the desired location
        /// </summary>
        /// <param name="info">MethodInfo</param>
        /// <param name="logType">Where it should be logged</param>
        public static void Log(this MethodInfo info, LogType logType = LogType.Custom) =>
            Logger.Log($"Method: \"{info}\"\n{info?.Attributes}", LogLevel.Debug, logType, Assembly.GetCallingAssembly().GetName().Name);

        #endregion

        #region Constructors

        /// <summary>
        /// Returns a list of all Constructors existing in a class
        /// </summary>
        /// <param name="type">Class</param>
        /// <returns></returns>
        public static IEnumerable<ConstructorInfo> GetAllConstructors(this Type type) =>
            type?.GetConstructors(Flags) ?? Enumerable.Empty<ConstructorInfo>();

        /// <summary>
        /// Losg all constructos of a class as debug messages in the desired location
        /// </summary>
        /// <param name="type">Class</param>
        /// <param name="logType">Where it should be logged</param>
        public static void LogAllConstructors(this Type type, LogType logType = LogType.Custom)
        {
            var caller = Assembly.GetCallingAssembly().GetName().Name;
            Logger.Log("Logging all Constructors", LogLevel.Debug, logType, caller);
            foreach (var info in type.GetAllConstructors())
                Logger.Log($"Constructor: \"{info}\"\n{info?.Attributes}", LogLevel.Debug, logType, caller);
            Logger.Log("End of Constructors", LogLevel.Debug, logType, caller);
        }

        /// <summary>
        /// Logs ConstructorInfo as a debug message in the desired location
        /// </summary>
        /// <param name="info">ConstructorInfo</param>
        /// <param name="logType">Where it should be logged</param>
        public static void Log(this ConstructorInfo info, LogType logType = LogType.Custom) =>
            Logger.Log($"Constructors \"{info}\"\n{info?.Attributes}", LogLevel.Debug, logType, Assembly.GetCallingAssembly().GetName().Name);
        #endregion
    }
}
