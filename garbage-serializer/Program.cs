using garbage_serializer;

var p = new Point(1, 2);
var x = Serializer.Deserialize<Point>(Serializer.Serialize<Point>(p));
var aboba = 0;