#nullable enable
using System.Runtime.CompilerServices;
using System.Text;

namespace garbage_serializer;

public class Point
{
    public int X { get; set; }
    public int Y { get; set; }

    public string Name { get; set; }="Point";
    public Point? Next { get; set; }
    
    public bool IsZero => X == 0 && Y == 0;

    public Point(int x, int y)
    {
        Next = (x == 0 || y == 0)
            ? null
            : new Point(x - 1, y - 2);
    }
}


public static class Serializer
{
    public static string Serialize<T>(T obj)
    {
        return Serializer.Serialize((object)obj!);
    }

    private static string Serialize(object obj)
    {
        var type = obj.GetType();
        var sb = new StringBuilder("{\n");
        var properties = type.GetProperties();
        foreach (var prop in properties)
        {
            if (prop.Name == "IsEmpty") continue;
            var val = prop.GetValue(obj, null);
            sb.Append($"\"{prop.Name}\": ");
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
            else
            {
                sb.Append(Serializer.Serialize(val));
            }
            sb.Append('\n');
        }
        sb.Append('}');
        return sb.ToString();
    }
}