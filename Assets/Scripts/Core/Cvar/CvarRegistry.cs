using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEngine;

namespace CodeBlue
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class ConFuncAttribute : Attribute
    {
        public string Name { get; }
        public string Description { get; }

        public ConFuncAttribute(string name, string desc = "")
        {
            Name = name;
            Description = desc;
        }
    }

    public static class CvarRegistry
    {
        private static readonly Dictionary<string, (MethodInfo method, object instance)> Commands = new();

        public static void RegisterCommands(object obj)
        {
            var type = obj.GetType();

            foreach (var method in type.GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic))
            {
                var attr = method.GetCustomAttribute<ConFuncAttribute>();
                if (attr != null)
                {
                    var instance = method.IsStatic ? null : obj;
                    Commands[attr.Name.ToLower()] = (method, instance);
                }
            }
        }

        // I LOVE BUILT IN RUNTIME REFLECTION !! c++ could never
        public static void ExecuteCommand(string input)
        {
            var tokens = ParseCommandInput(input);
            if (tokens.Length == 0) return;

            var commandName = tokens[0].ToLower();
            var args = tokens.Skip(1).ToArray();

            if (!Commands.TryGetValue(commandName.ToLower(), out var command))
            {
                Debug.LogError($"[CVAR] Command {commandName} not found");
                return;
            }

            var parameters = command.method.GetParameters();
            if (args.Length != parameters.Length)
            {
                Debug.LogError($"[CVAR] {commandName}: Expected {parameters.Length} params, got {args.Length}");
                return;
            }

            object[] convertedArgs = parameters.Select((p, i) => TryParseArgument(args[i], p.ParameterType)).ToArray();

            if (convertedArgs.Contains(null))
            {
                Debug.LogError($"[CVAR] Failed to parse args for {commandName}");
                return;
            }

            try
            {
                command.method.Invoke(command.instance, convertedArgs);
            }
            catch (Exception e)
            {
                Debug.LogError($"[CVAR] Error executing {commandName}: {e.InnerException.Message}");
            }
        }

        private static object TryParseArgument(string arg, Type targType)
        {
            if (targType == typeof(string)) return arg;
            if (targType.IsEnum && Enum.TryParse(targType, arg, true, out var enumVal)) return enumVal;

            try
            {
                return Convert.ChangeType(arg, targType);
            }
            catch
            {
                Debug.LogError($"[CVAR] Failed to convert `{arg}`to {targType.Name}");
                return null;
            }
        }

        private static string[] ParseCommandInput(string input)
        {
            // thank u gpt for this magic regex i hate regex so much
            var matches = Regex.Matches(input, @"[\""].+?[\""]|\S+");
            return matches.Cast<Match>()
                .Select(m => m.Value.Trim('"'))
                .ToArray();
        }
    }
}
