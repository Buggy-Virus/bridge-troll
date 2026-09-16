using System;
using System.Collections.Generic;

namespace BridgeTroll
{
    /// <summary>
    /// Stores persistent event flags and numerical progress counters across game days and cycles.
    /// Used by story triggers, dialogue conditions, and technology unlock requirements.
    /// </summary>
    public class EventFlags
    {
        private readonly Dictionary<string, bool> bool_flags = new();
        private readonly Dictionary<string, int> int_counters = new();

        /// <summary> Gets a boolean flag value, returning false if not set. </summary>
        public bool Get(string flagName) => bool_flags.GetValueOrDefault(flagName, false);

        /// <summary> Sets a boolean flag. </summary>
        public void Set(string flagName, bool value = true) => bool_flags[flagName] = value;

        /// <summary> Checks if a flag exists and is true. </summary>
        public bool Has(string flagName) => bool_flags.TryGetValue(flagName, out bool val) && val;

        /// <summary> Gets a numerical counter value, returning 0 if not set. </summary>
        public int GetCounter(string counterName) => int_counters.GetValueOrDefault(counterName, 0);

        /// <summary> Sets a numerical counter. </summary>
        public void SetCounter(string counterName, int value) => int_counters[counterName] = value;

        /// <summary> Increments a numerical counter by a given amount. </summary>
        public void IncrementCounter(string counterName, int amount = 1) =>
            int_counters[counterName] = GetCounter(counterName) + amount;

        /// <summary> Resets all flags and counters. </summary>
        public void Clear()
        {
            bool_flags.Clear();
            int_counters.Clear();
        }

        public IReadOnlyDictionary<string, bool> GetAllFlags() => bool_flags;
        public IReadOnlyDictionary<string, int> GetAllCounters() => int_counters;
    }
}
