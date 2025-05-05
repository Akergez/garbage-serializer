using garbage_serializer;

var p = new Point(1, 2);
Console.WriteLine(Serializer.Serialize<Point>(p));
