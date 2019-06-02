using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;

namespace ZookeeperTest
{
    public class JsonConfigurationParser
    {
        private readonly IDictionary<string, string> Data = new SortedDictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        private readonly Stack<string> Context = new Stack<string>();
        private string CurrentPath;

        public static IDictionary<string, string> Parse(string json) => new JsonConfigurationParser().ParseJson(json);

        private IDictionary<string, string> ParseJson(string json)
        {
            Data.Clear();

            var jsonConfig = JObject.Parse(json);

            VisitJObject(jsonConfig);

            return Data;
        }

        private void VisitJObject(JObject jObject)
        {
            foreach (var property in jObject.Properties())
            {
                EnterContext(property.Name);
                VisitProperty(property);
                ExitContext();
            }
        }

        private void VisitProperty(JProperty property)
        {
            VisitToken(property.Value);
        }

        private void VisitToken(JToken token)
        {
            switch (token.Type)
            {
                case JTokenType.Object:
                    VisitJObject(token.Value<JObject>());
                    break;

                case JTokenType.Array:
                    VisitArray(token.Value<JArray>());
                    break;

                case JTokenType.Integer:
                case JTokenType.Float:
                case JTokenType.String:
                case JTokenType.Boolean:
                case JTokenType.Bytes:
                case JTokenType.Raw:
                case JTokenType.Null:
                    VisitPrimitive(token.Value<JValue>());
                    break;

                default:
                    throw new FormatException("Unsupported JSON token");
            }
        }

        private void VisitArray(JArray array)
        {
            for (int index = 0; index < array.Count; index++)
            {
                EnterContext(index.ToString());
                VisitToken(array[index]);
                ExitContext();
            }
        }

        private void VisitPrimitive(JValue data)
        {
            var key = CurrentPath;

            if (Data.ContainsKey(key))
            {
                throw new FormatException("Duplicate Key");
            }
            Data[key] = data.ToString(CultureInfo.InvariantCulture);
        }

        private void EnterContext(string context)
        {
            Context.Push(context);
            CurrentPath = ConfigurationPath.Combine(Context.Reverse());
        }

        private void ExitContext()
        {
            Context.Pop();
            CurrentPath = ConfigurationPath.Combine(Context.Reverse());
        }
    }
}