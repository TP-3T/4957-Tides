public class Program
{
    public int Add(int x, int y)
    {
        return x + y;
    }

    public static void Main(string[] args)
    {
        Program prj = new Program();

        Console.WriteLine(prj.Add(1, 2));
    }
}