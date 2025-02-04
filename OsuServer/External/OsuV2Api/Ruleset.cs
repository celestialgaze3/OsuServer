namespace OsuServer.External.OsuV2Api
{
    public class Ruleset
    {
        public static Ruleset Standard => new Ruleset(0, "osu");
        public static Ruleset Mania => new Ruleset(1, "mania");
        public static Ruleset Taiko => new Ruleset(2, "taiko");
        public static Ruleset Catch => new Ruleset(3, "fruits");

        public static Ruleset[] Values = { Standard, Mania, Taiko, Catch };

        public int ValueInt { get; private set; }
        public string ValueString { get; private set; }

        private Ruleset(int valueInt, string valueString)
        {
            ValueInt = valueInt;
            ValueString = valueString;
        }

        public override string ToString()
        {
            return ValueString;
        }

        public static Ruleset FromString(string str)
        {
            foreach (Ruleset ruleset in Values)
            {
                if (ruleset.ValueString == str)
                    return ruleset;
            }

            throw new ArgumentException($"Argument {str} does not match a value in Ruleset");
        }

        public static Ruleset FromInt(int integer)
        {
            foreach (Ruleset ruleset in Values)
            {
                if (ruleset.ValueInt == integer)
                    return ruleset;
            }

            throw new ArgumentException($"Argument {integer} does not match a value in Ruleset");
        }


        public static implicit operator string(Ruleset ruleset)
        {
            return ruleset.ValueString;
        }

        public static implicit operator int(Ruleset ruleset)
        {
            return ruleset.ValueInt;
        }

        public static bool operator ==(Ruleset lhs, Ruleset rhs)
        {
            return lhs.ValueInt == rhs.ValueInt;
        }

        public static bool operator !=(Ruleset lhs, Ruleset rhs)
        {
            return lhs.ValueInt != rhs.ValueInt;
        }
    }
}
