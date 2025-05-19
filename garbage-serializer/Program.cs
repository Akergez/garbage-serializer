using garbage_serializer;

var p = new Point(1, 2);
var x = Serializer.GetKey(Serializer.Serialize<Point>(p));
var aboba = 0;