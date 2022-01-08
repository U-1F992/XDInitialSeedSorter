using System.Text.Json;

public class Program
{
    public static void Main(string[] args)
    {
        XDConnecter pkmnXD;
        try
        {
            Directory.SetCurrentDirectory(AppContext.BaseDirectory);
            pkmnXD = new XDConnecter(JsonSerializer.Deserialize<Setting>(File.ReadAllText("setting.json")));
        }
        catch (Exception e)
        {
            Console.Error.WriteLine(e);
            Console.Error.WriteLine("接続できませんでした。setting.jsonを確認してください。");
            return;
        }

        long count = 1;
        TimeSpan waitingTime;

        while (true)
        {
            do
            {
                pkmnXD.ClearScreen();
                Console.WriteLine("Attempt: {0}", count);
                waitingTime = pkmnXD.GetShortestWaitingTime();
                count++;
            } while (waitingTime > TimeSpan.Parse("03:00:00"));

            Console.WriteLine("Suitable seed is found!");
            if (waitingTime > TimeSpan.Parse("00:05:00"))
            {
                try
                {
                    pkmnXD.InvokeRoughConsumption(waitingTime - TimeSpan.Parse("00:05:00"));
                    break;
                }
                catch
                {
                    // never supposed to be here
                    // nothing to do but reset...
                }
            }
        }
    }
}