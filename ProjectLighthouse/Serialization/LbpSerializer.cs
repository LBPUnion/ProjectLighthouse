using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace ProjectLighthouse.Serialization {
    public static class LbpSerializer {
        public static string GetBlankElement(string key) => $"<{key}></{key}>";

        public static string GetStringElement(KeyValuePair<string, object> pair) => $"<{pair.Key}>{pair.Value}</{pair.Key}>";

        public static string GetStringElement(string key, object value) => $"<{key}>{value}</{key}>";

        public static string GetElements(params KeyValuePair<string, object>[] pairs) => 
            pairs.Aggregate(string.Empty, (current, pair) => current + GetStringElement(pair));
    }
}