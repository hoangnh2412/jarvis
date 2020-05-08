using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace Jarvis.Core.Extensions
{
    public static class JsonExtension
    {
        public static JObject MergeObjectsUseReflection(List<JObject> objects)
        {
            JObject json = new JObject();
            foreach (JObject JSONObject in objects)
            {
                foreach (var property in JSONObject)
                {
                    string name = property.Key;
                    JToken value = property.Value;

                    json.Add(property.Key, property.Value);
                }
            }
            return json;
        }

        public static JObject MergeObjectsUseNewtonsoft(List<JObject> objects, JsonMergeSettings settings = null)
        {
            if (objects.Count == 1)
                return objects[0];

            var json = new JObject();
            var first = objects[0];
            objects.RemoveAt(0);
            foreach (var obj in objects)
            {
                if (settings != null)
                {
                    first.Merge(obj, settings);
                }
                else
                {
                    first.Merge(obj, new JsonMergeSettings
                    {
                        MergeNullValueHandling = MergeNullValueHandling.Ignore,
                        MergeArrayHandling = MergeArrayHandling.Replace
                    });
                }
            }
            return json;
        }
    }
}