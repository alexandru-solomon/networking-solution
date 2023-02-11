using Networking;
using Networking.Serialization;
using System.Reflection;
using static System.Console;

BinaryFormatter formatter = new BinaryFormatter(new[] { Assembly.GetExecutingAssembly() },typeof(Packet));

formatter.Log.AddInfoHandler((obj, msg) => { ForegroundColor = ConsoleColor.DarkGray; WriteLine(msg); ForegroundColor = ConsoleColor.Gray; });
formatter.Log.AddWarnHandler((obj, msg) => { ForegroundColor = ConsoleColor.Yellow; WriteLine(msg); ForegroundColor = ConsoleColor.Gray; });
formatter.Log.AddErrorHandler((obj, msg) => { ForegroundColor = ConsoleColor.Red; WriteLine(msg); ForegroundColor = ConsoleColor.Gray; });

formatter.Initialize();


ClassTest classTest = new ClassTest
{
    n = 100,
    e = TestEnum.c,
    vec = new Vector2(33, 44)
};

formatter.Serialize(classTest);
WriteLine("pos: "+formatter.Stream.Position);

formatter.Stream.Position = 0;
ClassTest newClass = formatter.Deserialize<ClassTest>();

newClass?.Print();

formatter.Stream.Position = 0;
StructTest structTest = new()
{
    n = 200,
    e = TestEnum.b,
    vec = new Vector2(55, 266)
};

formatter.Serialize(structTest);
WriteLine("pos: " + formatter.Stream.Position);

formatter.Stream.Position = 0;
StructTest newStruct = formatter.Deserialize<StructTest>();

newStruct.Print(); 


public struct Vector2
{
    public Vector2(float x, float y)
    {
        this.x = x;
        this.y = y;
    }
    public float x { get; set; }
    public float y { get; set; }
}



public enum TestEnum { a, b, c }
[Packet] public class ClassTest
{
    public (ClassTest, int) a;
    public Vector2 vec;
    public int n;
    public TestEnum e;

    public List<int> list;
    public void Print()
    {
        WriteLine(n);
        WriteLine(e);
        //WriteLine(vec == null ? "null" : vec.x + " " + vec.y);
        WriteLine(vec.x + " " + vec.y);
    }
}

[Packet] public struct StructTest
{
    public Vector2 vec;
    public int? n;
    public TestEnum e;
    public void Print()
    {
        WriteLine(n);
        WriteLine(e);
        WriteLine(vec.x + " " + vec.y);
    }
}
[Packet] public class NetVector2:IRedefinition<Vector2>
{
    public NetVector2() { }

    public float x;
    public float y;
  

    public NetVector2(Vector2 obj)
    {
        this.x = obj.x;
        this.y = obj.y;

        
    }

    public Vector2 GetRepresented()
    {
        return new Vector2(x, y);
    }
}