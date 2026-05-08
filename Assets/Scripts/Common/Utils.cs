using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace CodeBlue
{
    public static class Utils
    {
        public static string GetDescription(this Enum value)
        {
            Type type = value.GetType();
            string name = Enum.GetName(type, value);
            if (name != null)
            {
                FieldInfo field = type.GetField(name);
                if (field != null)
                {
                    // cool c# tricks for pattern matching
                    if (Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute)) is DescriptionAttribute attr)
                        return attr.Description;
                }
            }
            return null;
        }

        public static void Show(this CanvasGroup group, bool isShowing)
        {
            group.alpha = isShowing ? 1 : 0;
            group.interactable = isShowing;
        }

        public static T SelectRandom<T>(this IEnumerable<T> list)
        {
            var rnd = new System.Random();
            return list.ElementAt(rnd.Next(list.Count()));
        }

        public static IList<T> Shuffle<T>(this IList<T> list) => list.OrderBy(x => Guid.NewGuid()).ToList();

        /// <summary>
        /// Copies all public fields and properties from the source object to the target object.
        /// </summary>
        /// <typeparam name="T">The type of the objects.</typeparam>
        /// <param name="source">The source object to copy from.</param>
        /// <param name="target">The target object to copy to.</param>
        public static void Copy<T>(T source, T target)
        {
            if (source == null || target == null)
            {
                Debug.LogError("Source or target object is null.");
                return;
            }

            Type type = typeof(T);

            // Copy fields
            FieldInfo[] fields = type.GetFields(BindingFlags.Public | BindingFlags.Instance);
            foreach (FieldInfo field in fields)
            {
                field.SetValue(target, field.GetValue(source));
            }

            // Copy properties
            PropertyInfo[] properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (PropertyInfo property in properties)
            {
                if (property.CanRead && property.CanWrite)
                {
                    property.SetValue(target, property.GetValue(source));
                }
            }
        }
    }
}
