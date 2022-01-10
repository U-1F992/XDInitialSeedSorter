using System.Text.Json;
using Pastel;

public class Program
{
    public static void Main(string[] args)
    {
        Directory.SetCurrentDirectory(AppContext.BaseDirectory);
        ConsoleExtensions.Enable();
        Console.OutputEncoding = System.Text.Encoding.UTF8;

        XDConnecter pkmnXD;
        try
        {
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
                Console.WriteLine("--- Attempt: {0} ---", count);
                Console.WriteLine("");

                waitingTime = pkmnXD.GetShortestWaitingTime();
                count++;

            } while (waitingTime > TimeSpan.Parse("03:00:00"));

            Console.WriteLine("Suitable seed is found!");
            if (waitingTime > TimeSpan.Parse("00:05:00"))
            {
                try
                {
                    pkmnXD.InvokeRoughConsumption(waitingTime - TimeSpan.Parse("00:05:00"));
                    pkmnXD.GetWaitingTimes(pkmnXD.GetCurrentSeed());
                    break;
                }
                catch
                {
                    // never supposed to be here
                    // nothing to do but reset...
                }
            }
        }

        pkmnXD.Dispose();
    }
}