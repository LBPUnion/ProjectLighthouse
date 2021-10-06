using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace ProjectLighthouse.Serialization {
    public static class LbpSerializer {
        public static string BlankElement(string key) => $"<{key}></{key}>";

        public static string StringElement(KeyValuePair<string, object> pair) => $"<{pair.Key}>{pair.Value}</{pair.Key}>";

        public static string StringElement(string key, object value) => $"<{key}>{value}</{key}>";

        public static string TaggedStringElement(KeyValuePair<string, object> pair, KeyValuePair<string, object> tagPair) =>
            $"<{pair.Key} {tagPair.Key}=\"{tagPair.Value}\">{pair.Value}</{pair.Key}>";
        
        public static string TaggedStringElement(string key, object value, string tagKey, object tagValue) => 
            $"<{key} {tagKey}=\"{tagValue}\">{value}</{key}>";

        public static string Elements(params KeyValuePair<string, object>[] pairs) => 
            pairs.Aggregate(string.Empty, (current, pair) => current + StringElement(pair));
    }
}