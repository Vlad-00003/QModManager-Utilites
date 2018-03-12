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
        /// Returns list of all Fileds existing in class
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static IEnumerable<FieldInfo> GetAllFields(this Type type) =>
            type?.GetFields(Flags).Union(GetAllFields(type.BaseType)) ?? Enumerable.Empty<FieldInfo>();

        /// <summary>
        /// Log all the fileds existing in class as the debug messages in the desired location
        /// </summary>
        /// <param name="type"></param>
        /// <param name="logtype"></param>
        public static void LogAllFields(this Type type, LogType logtype = LogType.Custom)
        {
            Logger.Debug("Logging all Fields",logtype);
            foreach (var info in type.GetAllFields())
                Log(info, logtype);
            Logger.Debug("End of Fields", logtype);
        }

        /// <summary>
        /// Log FieldInfo as the debug message in the desired location
        /// </summary>
        /// <param name="info"></param>
        /// <param name="type"></param>
        public static void Log(this FieldInfo info, LogType type = LogType.Custom) =>
            Logger.Debug($"Field: \"{info}\"\n{info?.Attributes}", type);

        /// <summary>
        /// Log FieldInfo and the value it has on instance as the debug messages in the desired location
        /// </summary>
        /// <param name="info"></param>
        /// <param name="instance"></param>
        /// <param name="type"></param>
        public static void Log(this FieldInfo info, object instance, LogType type = LogType.Custom)
        {
            Logger.Debug($"Field: \"{info}\". Value: \"{info?.GetValue(instance)}\"\n{info?.Attributes}", type);
        }
        #endregion

        #region Methods

        /// <summary>
        /// Outputs list of all methods existing in method
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static IEnumerable<MethodInfo> GetAllMethods(this Type type) =>
            type?.GetMethods(Flags).Union(GetAllMethods(type.BaseType)) ?? Enumerable.Empty<MethodInfo>();

        /// <summary>
        /// Logs all methods as the debug messages in the desired location
        /// </summary>
        /// <param name="type"></param>
        /// <param name="logtype"></param>
        public static void LogAllMethods(this Type type, LogType logtype = LogType.Custom)
        {
            Logger.Debug("Logging all Methods", logtype);
            foreach (var info in type.GetAllMethods())
                Log(info, logtype);
            Logger.Debug("End of Methods", logtype);
        }

        /// <summary>
        /// Log MethodInfo as the debug message in the desired location
        /// </summary>
        /// <param name="info"></param>
        /// <param name="logtype"></param>
        public static void Log(this MethodInfo info, LogType logtype = LogType.Custom) =>
            Logger.Debug($"Method: \"{info}\"\n\"{info?.Attributes}\"", logtype);

        #endregion

        #region Constructors

        /// <summary>
        /// Returns the list if all Constructors existing in the class
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static IEnumerable<ConstructorInfo> GetAllConstructors(this Type type) =>
            type?.GetConstructors(Flags) ?? Enumerable.Empty<ConstructorInfo>();

        /// <summary>
        /// Log all constructos of class as the debug messages in the desired location
        /// </summary>
        /// <param name="type"></param>
        /// <param name="logtype"></param>
        public static void LogAllConstructors(this Type type, LogType logtype = LogType.Custom)
        {
            Logger.Debug("Logging all Constructors", logtype);
            foreach (var info in type.GetAllConstructors())
                Log(info, logtype);
            Logger.Debug("End of Constructors", logtype);
        }

        /// <summary>
        /// Log ConstructorInfo as the debug message in the desired location
        /// </summary>
        /// <param name="info"></param>
        /// <param name="type"></param>
        public static void Log(this ConstructorInfo info, LogType type = LogType.Custom) =>
            Logger.Debug($"Constructor: {info}\n{info?.Attributes}",type);
        #endregion
    }
}
