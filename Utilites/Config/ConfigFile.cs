﻿using Oculus.Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;
using Utilites.Logger;

namespace Utilites.Config
{
    /// <summary>
    /// This code is based on open-source project Oxide.Core - https://github.com/OxideMod/Oxide.Core
    /// </summary>
    public class ConfigFile : IEnumerable<KeyValuePair<string,object>>
    {
        [JsonIgnore]
        private readonly string _filepath;
        private Dictionary<string, object> _elements;
        /// <summary>
        /// Json setializer setting that would be used to save\load configs. You can add your won if you need to load custom classes
        /// </summary>
        [JsonIgnore]
        public JsonSerializerSettings Settings { get; set; }
        [JsonIgnore]
        private readonly string _modname;
        [JsonIgnore]
        private readonly string _configPath = Environment.CurrentDirectory + @"\QMods\{0}\{1}.json";

        #region IEnumerable

        /// <inheritdoc />
        public IEnumerator<KeyValuePair<string, object>> GetEnumerator() => _elements.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => _elements.GetEnumerator();
        #endregion

        /// <summary>
        /// Check if specified file exists. If filename is null checks the initial file.
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        public bool Exists(string filename = null)=>File.Exists(GetFilePath(filename));
        

        /// <summary>
        /// Generic config file. Extension would be automaticly added to the filename.
        /// </summary>
        /// <param name="filename"></param>
        public ConfigFile(string filename)
        {
            _modname = Assembly.GetCallingAssembly().GetName().Name;
            _filepath = string.Format(_configPath, _modname, filename);
            _elements = new Dictionary<string, object>();
            Settings = new JsonSerializerSettings();
            Settings.Converters.Add(new KeyValuesConverter());
        }

        /// <summary>
        /// Loads the config from the specified file, or the initilized one.
        /// </summary>
        /// <param name="filename"></param>
        public void Load(string filename = null)
        {
            if (!Exists(filename)) return;
            var filepath = GetFilePath(filename);
            string source = File.ReadAllText(filepath);
            _elements = JsonConvert.DeserializeObject<Dictionary<string, object>>(source, Settings);
        }

        /// <summary>
        /// Saves the config from the specified file, or the initilized one.
        /// </summary>
        /// <param name="filename">File name</param>
        public void Save(string filename = null)
        {
            var filepath = GetFilePath(filename);
            var dir = GetDirectory(filepath);
            if (dir != null && !Directory.Exists(dir)) Directory.CreateDirectory(dir);
            File.WriteAllText(filepath, JsonConvert.SerializeObject(_elements,Formatting.Indented,Settings));
        }
        /// <summary>
        /// Removes all entries from the config.
        /// </summary>
        public void Clear() => _elements.Clear();

        /// <summary>
        /// Gets or sets a setting on this config by key
        /// </summary>
        /// <param name="keys"></param>
        /// <returns></returns>
        public object this[params string[] keys]
        {
            get => Get(keys);
            set => Set(new List<object>(keys) { value }.ToArray());
        }

        /// <summary>
        /// Gets a configuration value at the specified path
        /// </summary>
        /// <param name="keys"></param>
        /// <returns></returns>
        public object Get(params string[] keys)
        {
            if (keys.Length < 1)
                throw new ArgumentException("Attempt to get the config value with an empty path");
            object val;
            if (!_elements.TryGetValue(keys[0], out val)) return null;
            for (int i = 1; i < keys.Length; i++)
            {
                var dict = val as Dictionary<string, object>;
                if (dict == null || !dict.TryGetValue(keys[i], out val)) return null;
            }
            return val;
        }

        /// <summary>
        /// Sets a configuration value at the specified path
        /// </summary>
        /// <param name="pathAndTrailingValue"></param>
        public void Set(params object[] pathAndTrailingValue)
        {
            if (pathAndTrailingValue.Length < 2)
                throw new ArgumentException("Attempt to set the config value without path.");
            var path = new string[pathAndTrailingValue.Length - 1];
            for (var i = 0; i < pathAndTrailingValue.Length - 1; i++)
                path[i] = (string)pathAndTrailingValue[i];
            var value = pathAndTrailingValue[pathAndTrailingValue.Length - 1];
            if (path.Length == 1)
            {
                _elements[path[0]] = value;
                return;
            }
            object val;
            if (!_elements.TryGetValue(path[0], out val))
                _elements[path[0]] = val = new Dictionary<string, object>();
            for (var i = 1; i < path.Length - 1; i++)
            {
                if (!(val is Dictionary<string, object>))
                    throw new ArgumentException("Attempt to set the config value with path not being dictionary.");
                var oldVal = (Dictionary<string, object>)val;
                if (!oldVal.TryGetValue(path[i], out val))
                    oldVal[path[i]] = val = new Dictionary<string, object>();
            }
            ((Dictionary<string, object>)val)[path[path.Length - 1]] = value;
        }
        /// <summary>
        /// Converts a configuration value to another type
        /// </summary>
        /// <param name="value"></param>
        /// <param name="destinationType"></param>
        /// <returns></returns>
        public object ConvertValue(object value, Type destinationType)
        {
            if (value == null) return null;
            if (!destinationType.IsGenericType) return Convert.ChangeType(value, destinationType);
            if (destinationType.GetGenericTypeDefinition() == typeof(List<>))
            {
                var valueType = destinationType.GetGenericArguments()[0];
                var list = (IList)Activator.CreateInstance(destinationType);
                foreach (var val in (IList)value)
                {
                    //Custom classes. They are being stored as Dictionary<string,object>.
                    IDictionary customClass = val as IDictionary;
                    list.Add(customClass != null ? ConvertCustomType(customClass, valueType) : ConvertValue(val, valueType));
                }
                return list;
            }
            if (destinationType.GetGenericTypeDefinition() == typeof(Dictionary<,>))
            {
                var keyType = destinationType.GetGenericArguments()[0];
                var valueType = destinationType.GetGenericArguments()[1];
                var dict = (IDictionary)Activator.CreateInstance(destinationType);
                foreach (var key in ((IDictionary)value).Keys)
                {
                    var val = ((IDictionary)value)[key];
                    IDictionary customClass = val as IDictionary;
                    dict.Add(Convert.ChangeType(key, keyType), 
                        customClass != null ? ConvertCustomType(customClass,valueType) : ConvertValue(val, valueType));
                }
                return dict;
            }
            throw new InvalidCastException("Generic types other than List<> and Dictionary<,> are not supported");
        }

        private object ConvertCustomType(IDictionary dict, Type destinationType)
        {
            var fields = destinationType.GetAllFields().ToList();
            object inst;
            try
            {
                inst = Activator.CreateInstance(destinationType);
            }
            catch (Exception e)
            {
                e.Log(LogType.Console);
                return null;
            }
            foreach (var key in dict.Keys)
            {
                var field = fields.FirstOrDefault(x => x.Name == key.ToString());
                if (field == null)
                    throw new ArgumentNullException($"Field \"{key}\" not found!");
                field.SetValue(inst, ConvertValue(dict[key], field.FieldType));
            }
            return inst;
        }

        /// <summary>
        /// Tries to get a value out of config. If it exists it sets the value to the config one.
        /// If it doesn't - Adds the provided value to the config.
        /// If the config was changed returns true, overwise it return false.
        /// </summary>
        /// <typeparam name="T">T</typeparam>
        /// <param name="variable">Variable</param>
        /// <param name="path">Config path</param>
        /// <returns></returns>
        public bool TryGet<T>(ref T variable, params string[] path)
        {
            var tmp = Get(path);
            if (tmp != null)
            {
                var res = ConverValues<T>(tmp);
                variable = res;
                return false;
            }
            this[path] = variable;
            return true;
        }
        /// <summary>
        /// Tries to convert object to type of T
        /// </summary>
        /// <param name="value"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T ConverValues<T>(object value) => (T) ConvertValue(value, typeof(T));
        /// <summary>
        /// Gets the value from the config stored at path, then tries to convert it to requested type of T
        /// </summary>
        /// <param name="path"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T Get<T>(params string[] path) => ConverValues<T>(Get(path));

        /// <summary>
        /// Tries to read object of type T from the config file. If it doesnt' exists creates the new one with the default object of type T.
        /// </summary>
        /// <param name="defaultValue"></param>
        /// <param name="filename"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T ReadObject<T>(T defaultValue, string filename = null)
        {
            var filePath = GetFilePath(filename);
            T customObject;
            if (Exists(filename))
            {
                var source = File.ReadAllText(filePath);
                customObject = JsonConvert.DeserializeObject<T>(source, Settings);
            }
            else
            {
                // ReSharper disable once CompareNonConstrainedGenericWithNull
                // ReSharper disable once ConvertIfStatementToNullCoalescingExpression
                if (defaultValue != null)
                {
                    customObject = defaultValue;
                }
                else
                {
                    customObject = Activator.CreateInstance<T>();
                }
                WriteObject(customObject,filename);
            }
            return customObject;
        }
        /// <summary>
        /// Saves the config to the specified file, or the initilized one.
        /// Sync determines if the config should get the data as well.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="config"></param>
        /// <param name="filename"></param>
        /// <param name="sync"></param>
        public void WriteObject<T>(T config, string filename = null, bool sync = false)
        {
            var filepath = GetFilePath(filename);
            var dir = GetDirectory(filepath);
            if (dir != null && !Directory.Exists(dir)) Directory.CreateDirectory(dir);
            var json = JsonConvert.SerializeObject(config, Formatting.Indented, Settings);
            File.WriteAllText(filepath, json);
            if (sync) _elements = JsonConvert.DeserializeObject<Dictionary<string, object>>(json, Settings);
        }

        #region Helpers

        private string GetFilePath(string filename = null) =>
            filename == null ? _filepath : string.Format(_configPath, _modname, filename);

        private static string GetDirectory(string path)
        {
            try
            {
                path = path.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
                return path.Substring(0, path.LastIndexOf(Path.DirectorySeparatorChar));
            }
            catch (Exception e)
            {
                Logger.Logger.Error("Unable to read config file!",LogType.Console);
                e.Log(LogType.Console);
                return null;
            }
        }

        #endregion

        #region Custom json converter
        /// <summary>
        /// Public code from the Oxide.Core - https://github.com/OxideMod/Oxide.Core
        /// </summary>
        [SuppressMessage("ReSharper", "AssignNullToNotNullAttribute")]
        public class KeyValuesConverter : JsonConverter
        {
            /// <summary>
            /// Returns if this converter can convert the specified type or not
            /// </summary>
            /// <param name="objectType"></param>
            /// <returns></returns>
            public override bool CanConvert(Type objectType)
            {
                return objectType == typeof(Dictionary<string, object>) || objectType == typeof(List<object>);
            }

            private void Throw(string message)
            {
                throw new Exception(message);
            }

            /// <summary>
            /// Reads an instance of the specified type from json
            /// </summary>
            /// <param name="reader"></param>
            /// <param name="objectType"></param>
            /// <param name="existingValue"></param>
            /// <param name="serializer"></param>
            /// <returns></returns>
            public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
            {
                if (objectType == typeof(Dictionary<string, object>))
                {
                    // Get the dictionary to populate
                    Dictionary<string, object> dict = existingValue as Dictionary<string, object> ?? new Dictionary<string, object>();
                    if (reader.TokenType == JsonToken.StartArray)
                    {
                        return dict;
                    }
                    // Read until end of object
                    while (reader.Read() && reader.TokenType != JsonToken.EndObject)
                    {
                        // Read property name
                        if (reader.TokenType != JsonToken.PropertyName) Throw("Unexpected token: " + reader.TokenType);
                        string propname = reader.Value as string;
                        if (!reader.Read()) Throw("Unexpected end of json");

                        // What type of object are we reading?
                        switch (reader.TokenType)
                        {
                            case JsonToken.String:
                            case JsonToken.Float:
                            case JsonToken.Boolean:
                            case JsonToken.Bytes:
                            case JsonToken.Date:
                            case JsonToken.Null:
                                dict[propname] = reader.Value;
                                break;

                            case JsonToken.Integer:
                                var value = reader.Value.ToString();
                                if (int.TryParse(value, out var result))
                                    dict[propname] = result;
                                else
                                    dict[propname] = value;
                                break;

                            case JsonToken.StartObject:
                                dict[propname] = serializer.Deserialize<Dictionary<string, object>>(reader);
                                break;

                            case JsonToken.StartArray:
                                dict[propname] = serializer.Deserialize<List<object>>(reader);
                                break;

                            default:
                                Throw("Unexpected token: " + reader.TokenType);
                                break;
                        }
                    }

                    // Return it
                    return dict;
                }
                if (objectType == typeof(List<object>))
                {
                    // Get the list to populate
                    List<object> list = existingValue as List<object> ?? new List<object>();

                    // Read until end of array
                    while (reader.Read() && reader.TokenType != JsonToken.EndArray)
                    {
                        // What type of object are we reading?
                        switch (reader.TokenType)
                        {
                            case JsonToken.String:
                            case JsonToken.Float:
                            case JsonToken.Boolean:
                            case JsonToken.Bytes:
                            case JsonToken.Date:
                            case JsonToken.Null:
                                list.Add(reader.Value);
                                break;

                            case JsonToken.Integer:
                                var value = reader.Value.ToString();
                                int result;
                                if (int.TryParse(value, out result))
                                    list.Add(result);
                                else
                                    list.Add(value);
                                break;

                            case JsonToken.StartObject:
                                list.Add(serializer.Deserialize<Dictionary<string, object>>(reader));
                                break;

                            case JsonToken.StartArray:
                                list.Add(serializer.Deserialize<List<object>>(reader));
                                break;

                            default:
                                Throw("Unexpected token: " + reader.TokenType);
                                break;
                        }
                    }

                    // Return it
                    return list;
                }
                return existingValue;
            }

            /// <summary>
            /// Writes an instance of the specified type to json
            /// </summary>
            /// <param name="writer"></param>
            /// <param name="value"></param>
            /// <param name="serializer"></param>
            public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
            {
                // Get the dictionary to write if any
                if (value is Dictionary<string, object> dict)
                {
                    // Start object
                    writer.WriteStartObject();

                    // Simply loop through and serialise
                    foreach (var pair in dict.OrderBy(i => i.Key))
                    {
                        writer.WritePropertyName(pair.Key, true);
                        serializer.Serialize(writer, pair.Value);
                    }

                    // End object
                    writer.WriteEndObject();
                }
                else if (value is List<object>)
                {
                    // Get the list to write
                    var list = (List<object>)value;

                    // Start array
                    writer.WriteStartArray();

                    // Simply loop through and serialise
                    foreach (var t in list)
                        serializer.Serialize(writer, t);

                    // End array
                    writer.WriteEndArray();
                }
            }
        }
        #endregion
    }
}
