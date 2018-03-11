using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Utilites.Logger
{
    internal static class ReflectionUtilites
    {

        private const BindingFlags Flags =
            BindingFlags.Public |
            BindingFlags.NonPublic |
            BindingFlags.Static |
            BindingFlags.Instance |
            BindingFlags.DeclaredOnly;

        #region Fields

        public static IEnumerable<FieldInfo> GetAllFields(this Type type) =>
            type?.GetFields(Flags).Union(GetAllFields(type.BaseType)) ?? Enumerable.Empty<FieldInfo>();

        public static void LogAllFields(this Type type, LogType logtype = LogType.Custom)
        {
            Logger.Debug("Logging all Fields",logtype);
            foreach (var info in type.GetAllFields())
                Log(info, logtype);
            Logger.Debug("End of Fields", logtype);
        }

        public static void Log(this FieldInfo info, LogType type = LogType.Custom) =>
            Logger.Debug($"Field: \"{info}\"\n{info?.Attributes}", type);

        public static void Log(this FieldInfo info, object obj, LogType type = LogType.Custom)
        {
            Logger.Debug($"Field: \"{info}\". Value: \"{info?.GetValue(obj)}\"\n{info?.Attributes}", type);
        }
        #endregion

        #region Methods

        public static IEnumerable<MethodInfo> GetAllMethods(this Type type) =>
            type?.GetMethods(Flags).Union(GetAllMethods(type.BaseType)) ?? Enumerable.Empty<MethodInfo>();

        public static void LogAllMethods(this Type type, LogType logtype = LogType.Custom)
        {
            Logger.Debug("Logging all Methods", logtype);
            foreach (var info in type.GetAllMethods())
                Log(info, logtype);
            Logger.Debug("End of Methods", logtype);
        }

        public static void Log(this MethodInfo info, LogType logtype = LogType.Custom) =>
            Logger.Debug($"Method: \"{info}\"\n\"{info?.Attributes}\"", logtype);

        #endregion

        #region Constructors

        public static IEnumerable<ConstructorInfo> GetAllConstructors(this Type type) =>
            type?.GetConstructors(Flags) ?? Enumerable.Empty<ConstructorInfo>();

        public static void LogAllConstructors(this Type type, LogType logtype = LogType.Custom)
        {
            Logger.Debug("Logging all Constructors", logtype);
            foreach (var info in type.GetAllConstructors())
                Log(info, logtype);
            Logger.Debug("End of Constructors", logtype);
        }

        public static void Log(this ConstructorInfo info, LogType type = LogType.Custom) =>
            Logger.Debug($"Constructor: {info}\n{info?.Attributes}",type);
        #endregion
    }
}
