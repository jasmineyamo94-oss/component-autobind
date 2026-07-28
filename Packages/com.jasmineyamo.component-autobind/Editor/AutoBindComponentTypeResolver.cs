using System;
using System.Reflection;
using UnityEngine;

namespace JasmineYamo.ComponentAutoBind.Editor
{
    internal static class AutoBindComponentTypeResolver
    {
        public static bool TryResolve(string typeName, out Type componentType)
        {
            componentType = null;
            if (string.IsNullOrWhiteSpace(typeName))
            {
                return false;
            }

            componentType = Type.GetType(typeName, false);
            if (IsComponentType(componentType))
            {
                return true;
            }

            foreach (System.Reflection.Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                Type[] types;
                try
                {
                    types = assembly.GetTypes();
                }
                catch (ReflectionTypeLoadException exception)
                {
                    types = exception.Types ?? System.Array.Empty<Type>();
                }
                catch (NotSupportedException)
                {
                    continue;
                }

                for (int i = 0; i < types.Length; i++)
                {
                    Type candidate = types[i];
                    if (candidate == null || !IsComponentType(candidate))
                    {
                        continue;
                    }

                    if (string.Equals(candidate.FullName, typeName, StringComparison.Ordinal)
                        || string.Equals(candidate.Name, typeName, StringComparison.Ordinal))
                    {
                        componentType = candidate;
                        return true;
                    }
                }
            }

            componentType = null;
            return false;
        }

        public static Component GetComponent(GameObject gameObject, string typeName)
        {
            if (gameObject == null || !TryResolve(typeName, out Type componentType))
            {
                return null;
            }

            return gameObject.GetComponent(componentType);
        }

        private static bool IsComponentType(Type type)
        {
            return type != null
                && typeof(Component).IsAssignableFrom(type)
                && !type.IsAbstract;
        }
    }
}
