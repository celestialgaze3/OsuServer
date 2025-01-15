using Newtonsoft.Json.Linq;

namespace OsuServer.External.OsuV2Api
{
    public class Ruleset
    {
        public static Ruleset Standard => new Ruleset("osu");
        public static Ruleset Mania => new Ruleset("mania");
        public static Ruleset Taiko => new Ruleset("taiko");
        public static Ruleset Catch => new Ruleset("fruits");
        public static Ruleset[] Values = { Standard, Mania, Taiko, Catch };

        public string Value { get; private set; }
        private Ruleset(string value)
        {
            Value = value;
        }

        public override string ToString()
        {
            return Value;
        }

        public static Ruleset FromString(string str)
        {
            foreach (Ruleset ruleset in Values)
            {
                if (ruleset.Value == str)
                    return ruleset;
            }

            throw new ArgumentException($"Argument {str} does not match a value in Ruleset");
        }


        public static implicit operator string(Ruleset ruleset)
        {
            return ruleset.Value;
        }
    }
}
