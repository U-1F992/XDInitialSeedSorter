public class QuickBattleData
{
    public int[] Party { get; set; }
    public int[] HP { get; set; }
    public QuickBattleData(int[] party, int[] hp)
    {
        this.Party = party;
        this.HP = hp;
    }
    public override string ToString()
    {
        return Party[0] + " " + Party[1] + "\n" + HP[0] + " " + HP[1] + " " + HP[2] + " " + HP[3] + "\n";
    }
    public string ToJson()
    {
        return "{\"HP\":[" + Party[0] + "," + Party[1] + "],\"Party\":[" + HP[0] + "," + HP[1] + "," + HP[2] + "," + HP[3] + "]}";
    }
}