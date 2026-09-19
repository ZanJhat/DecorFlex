using Engine;
using Engine.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Game
{
    public static class DecorFlexTypesManager
    {
        public const string LogPrefix = "DecorFlexTypesManager";
        public const string LogPrefix2 = "DecorFlexType";

        public const int MaxTypeId = 1023; // 2^10 - 1

        public static readonly Dictionary<int, DecorFlexType> DecorFlexTypes = new();

        public static void AutoRegister()
        {
            IEnumerable<Type> types = Assembly.GetExecutingAssembly().GetTypes().Where(t => !t.IsAbstract && typeof(DecorFlexType).IsAssignableFrom(t));

            foreach (Type type in types)
            {
                try
                {
                    if (Activator.CreateInstance(type) is DecorFlexType dfType)
                        Register(dfType);
                }
                catch (Exception ex)
                {
                    Log.Warning($"[{LogPrefix}] Failed to register {LogPrefix2} '{type.FullName}': {ex}");
                }
            }
        }

        public static void Register(DecorFlexType dfType)
        {
            if (dfType == null)
            {
                Log.Warning($"[{LogPrefix}] Attempted to register null {LogPrefix2}.");
                return;
            }

            int id = dfType.Id;
            string typeName = dfType.GetType().Name;

            if (id < 0)
            {
                Log.Warning($"[{LogPrefix}] {LogPrefix2} '{typeName}' has invalid Id={id}. Id cannot be negative (< 0).");
                return;
            }

            if (id > MaxTypeId)
            {
                Log.Warning($"[{LogPrefix}] {LogPrefix2} '{typeName}' has invalid Id={id}. Maximum allowed value is {MaxTypeId} (10 bits).");
                return;
            }

            if (DecorFlexTypes.ContainsKey(id))
            {
                Log.Warning($"[{LogPrefix}] {LogPrefix2} '{typeName}' Id={id} already registered by '{DecorFlexTypes[id].GetType().Name}'. Replacing existing value.");
            }

            DecorFlexTypes[id] = dfType;
        }

        public static DecorFlexType GetDecorFlexType(int id) => DecorFlexTypes.TryGetValue(id, out DecorFlexType dfType) ? dfType : null;
    }
}
