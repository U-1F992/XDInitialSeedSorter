using System.Text.Json;
using Pastel;
using PokemonPRNG.LCG32.GCLCG;
using PokemonXD;

public class Program
{
    public static void Main(string[] args)
    {
        // apply silent flag
        bool flagSilent = (args.Contains("--silent") || args.Contains("-s"));
        if (flagSilent)
        {
            Console.SetOut(TextWriter.Null);
        }

        Directory.SetCurrentDirectory(AppContext.BaseDirectory);
        ConsoleExtensions.Enable();

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
        // apply silent flag for stderr
        if (flagSilent)
        {
            Console.SetError(TextWriter.Null);
        }

        long count = 1;
        TimeSpan waitingTime;

        UInt32 currentSeed = 0;
        UInt32 targetSeed = 0;

        do
        {
            do
            {
                Console.WriteLine("");
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
                    currentSeed = 0;
                    targetSeed = 0;

                    pkmnXD.InvokeRoughConsumption(waitingTime - TimeSpan.Parse("00:05:00"));
                    
                    currentSeed = pkmnXD.GetCurrentSeed();
                    targetSeed = pkmnXD.GetWaitingTimes(currentSeed).OrderBy(pair => pair.Value).First().Key;
                    break;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    // never supposed to be here
                    // nothing to do but reset...
                }
            }
        } while (!pkmnXD.IsLeftEnough(currentSeed, targetSeed));

        pkmnXD.Dispose();

        // always show this message
        if (flagSilent)
        {
            StreamWriter stdOut = new StreamWriter(Console.OpenStandardOutput());
            Console.SetOut(stdOut);
            stdOut.AutoFlush = true;
        }
        Console.WriteLine("{{\"currentSeed\":{0},\"targetSeed\":{1}}}", currentSeed, targetSeed);
    }
}