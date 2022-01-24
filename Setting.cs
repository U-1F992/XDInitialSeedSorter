#pragma warning disable CS8618

namespace PokemonXD
{
    public class Setting
    {
        public PokemonXD pokemonXD { get; set; }
        public Binaries binaries { get; set; }
        public Devices devices { get; set; }
        public Dictionary<string, Rect> crops { get; set; }

        public class PokemonXD
        {
            public List<UInt32> targets { get; set; }
            public int leftover { get; set; }
            public double consumptionPerSec { get; set; }
        }

        public class Binaries
        {
            public string tesseractOCR { get; set; }
            public string xdDatabase { get; set; }
        }

        public class Devices
        {
            public Controller controller { get; set; }
            public Capture capture { get; set; }

            public class Controller
            {
                public string port { get; set; }
                public int delayAfterReset { get; set; }
            }

            public class Capture
            {
                public int index { get; set; }
                public int width { get; set; }
                public int height { get; set; }
                public bool showImage { get; set; }
            }
        }

        public class Crops
        {
            public Rect player { get; set; }
            public Rect com { get; set; }
            public Rect hp_1 { get; set; }
            public Rect hp_2 { get; set; }
            public Rect hp_3 { get; set; }
            public Rect hp_4 { get; set; }

            
        }

        public class Rect
        {
            public int width { get; set; }
            public int height { get; set; }
            public int x { get; set; }
            public int y { get; set; }
        }
    }
}
#pragma warning restore CS8618