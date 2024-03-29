﻿using System;
using System.Collections.Generic;

namespace FishsGrandAdventure.Utils.Converters
{
#pragma warning disable CA2235 // Mark all non-serializable fields
    [Serializable]
    [System.Diagnostics.DebuggerDisplay("{converterName}, enabled={enabled}")]
    public struct ConverterConfig : IEquatable<ConverterConfig>
    {
        public bool enabled;

        public string converterName;

        public List<KeyedConfig> settings;

        public override string ToString()
        {
            return $"{{enabled={enabled}, converterName={converterName}, settings=[{settings?.Count ?? 0}]}}";
        }

        public override bool Equals(object obj)
        {
            return obj is ConverterConfig config && Equals(config);
        }

        public bool Equals(ConverterConfig other)
        {
            return enabled == other.enabled &&
                   converterName == other.converterName &&
                   EqualityComparer<List<KeyedConfig>>.Default.Equals(settings, other.settings);
        }

#pragma warning disable S2328 // "GetHashCode" should not reference mutable fields
        public override int GetHashCode()
#pragma warning restore S2328 // "GetHashCode" should not reference mutable fields
        {
            int hashCode = 1016066258;
            hashCode = hashCode * -1521134295 + enabled.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(converterName);
            hashCode = hashCode * -1521134295 + EqualityComparer<List<KeyedConfig>>.Default.GetHashCode(settings);
            return hashCode;
        }

        public static bool operator ==(ConverterConfig left, ConverterConfig right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(ConverterConfig left, ConverterConfig right)
        {
            return !(left == right);
        }
    }
#pragma warning restore CA2235 // Mark all non-serializable fields
}
