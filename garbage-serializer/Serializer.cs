#nullable enable
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;

namespace garbage_serializer;

public class RenamePropertyAttribute : Attribute
{
    public string Name { get; set; }

    public RenamePropertyAttribute(string name)
    {
        Name = name;
    }
}

public static class Serializer
{
    public static string Serialize<T>(T obj)
    {
        return Serialize(obj as object);
    }

    private static string Serialize(object? obj)
    {
        if (obj is null) return "null";
        var type = obj.GetType();
        if (type == typeof(string))
        {
            return StringSerializer((string)obj);
        }

        if (type == typeof(int))
        {
            return obj.ToString();
        }

        var sb = new StringBuilder("{\n");
        var properties = type.GetProperties();
        foreach (var prop in properties)
        {
            if (prop.Name == "IsEmpty") continue;
            var val = prop.GetValue(obj, null);
            var name = prop.GetCustomAttribute<RenamePropertyAttribute>()?.Name ?? prop.Name;
            sb.Append($"\"{name}\": ");
            var propType = prop.PropertyType;
            if (propType.IsPrimitive || propType.IsEnum)
            {
                sb.Append(val);
            }
            else if (propType == typeof(string))
            {
                sb.Append($"\"{val}\"");
            }
            else if (val is null)
            {
                sb.Append("null");
            }
            else if (propType.IsArray)
            {
                sb.Append(ArraySerializer((Array)val));
            }
            else
            {
                sb.Append(Serialize(val));
            }

            sb.Append(",\n");
        }

        sb.Append('}');
        return sb.ToString();
    }

    private static string[] GarbageSplit(string json)
    {
        var nasting = 0;
        var splitted = new StringBuilder();
        var result = new List<string>();
        for (var i = 1; i < json.Length - 1; i++)
        {
            if (!(nasting == 0 && json[i] == ','))
                splitted.Append(json[i]);
            if (json[i] == '[')
                nasting++;
            if (json[i] == ']')
                nasting--;
            if (json[i] == '{')
                nasting++;
            if (nasting == 0 && json[i] == ',')
            {
                result.Add(splitted.ToString());
                splitted.Clear();
            }
            if (json[i] == '}')
                nasting--;
        }

        return result.ToArray();
    }

    private static JsonArray GetArray(string json)
    {
        var splitted = GarbageSplit(json);
        var result = new List<JsonNode>();
        foreach (var l in GarbageSplit(json))
        {
            var e = l.Trim();
            var value = e.Trim()[0] switch
            {
                '{' => GetKey(e.Trim()),
                '[' => GetArray(e.Trim()),
                '"' => new JsonPrimitiveType { children = e.Trim()[1..^1] },
                _ => new JsonPrimitiveType { children = e.Trim() }
            };

            result.Add(value);
        }

        return new JsonArray { children = result.ToArray() };
    }

    public static JsonNode GetKey(string json)
    {
        json = json.Replace("\n", "").Replace("\t", "");
        var open = 0;
        var close = json[^1];
        var JsonNode = new Dictionary<string, JsonNode>();
        foreach (var l in GarbageSplit(json))
        {
            var e = l.Trim();
            var key = e[..e.IndexOf(':')];
            var value = e[(e.IndexOf(':') + 2)..].Trim()[0] switch
            {
                '{' => GetKey(e[(e.IndexOf(':') + 2)..]),
                '[' => GetArray(e[(e.IndexOf(':') + 2)..]),
                '"' => new JsonPrimitiveType { children = e[(e.IndexOf(':') + 2)..][1..^1] },
                _ => new JsonPrimitiveType { children = e[(e.IndexOf(':') + 2)..] }
            };

            JsonNode[key] = value;
        }

        return new JsonObject { children = JsonNode };
    }

    private static void Deserialize(string json, Type type)
    {
    }

    private static string StringSerializer(string str)
    {
        return $"\"{str}\"";
    }

    private static string ArraySerializer(Array arr)
    {
        var sb = new StringBuilder("[ ");
        foreach (var obj in arr)
        {
            sb.Append(Serialize(obj));
            sb.Append(", ");
        }

        sb.Remove(sb.Length - 2, 2);
        sb.Append(" ]");

        return sb.ToString();
    }
}

public abstract class JsonNode
{
}

public class JsonObject : JsonNode
{
    public Dictionary<string, JsonNode> children = new Dictionary<string, JsonNode>();
}

public class JsonArray : JsonNode
{
    public JsonNode[] children = new JsonNode[] { };
}

public class JsonPrimitiveType : JsonNode
{
    public object? children = null;
}

internal class Point
{
    public int X { get; set; }
    public int Y { get; set; }

    [RenameProperty("name")] public string Name { get; set; } = "Point";
    public Point? Next { get; set; }

    public Student[] Array { get; set; }

    public bool IsZero => X == 0 && Y == 0;

    public Point(int x, int y)
    {
        Next = (x == 0 || y == 0)
            ? null
            : new Point(x - 1, y - 2);

        Array = new Student[4] { new Student(), new Student(), new Student(), new Student() };
    }

    internal class Student
    {
        public string Name { get; set; }
        public int Age { get; set; }
    }
}