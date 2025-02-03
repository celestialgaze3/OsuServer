namespace OsuServer.External.OsuV2Api
{
    public class RankStatus
    {
        public static RankStatus Graveyard => new RankStatus(-2, "graveyard");
        public static RankStatus Wip => new RankStatus(-1, "wip");
        public static RankStatus Pending => new RankStatus(0, "pending");
        public static RankStatus Ranked => new RankStatus(1, "ranked");
        public static RankStatus Approved => new RankStatus(2, "approved");
        public static RankStatus Qualified => new RankStatus(3, "qualified");
        public static RankStatus Loved => new RankStatus(4, "loved");

        public static RankStatus[] Values = { Graveyard, Wip, Pending, Ranked, Approved, Qualified, Loved };

        public int ValueInt { get; private set; }
        public int ValueGetScores {
            get
            {
                if (ValueInt <= 0)
                    return 0; // Pending

                // ValueInt >= 1 here
                return ValueInt + 1; // offset by "UpdateAvailable"
            }
        }
        public string ValueString { get; private set; }
        private RankStatus(int valueInt, string valueString)
        {
            ValueInt = valueInt;
            ValueString = valueString;
        }

        public override string ToString()
        {
            return ValueString;
        }
        public static RankStatus FromInt(int integer)
        {
            foreach (RankStatus rankStatus in Values)
            {
                if (rankStatus.ValueInt == integer)
                    return rankStatus;
            }

            throw new ArgumentException($"Argument {integer} does not match a value in RankStatus");
        }

        public static RankStatus FromString(string str)
        {
            foreach (RankStatus rankStatus in Values)
            {
                if (rankStatus.ValueString == str) 
                    return rankStatus;
            }

            throw new ArgumentException($"Argument {str} does not match a value in RankStatus");
        }

        public static implicit operator string(RankStatus ruleset)
        {
            return ruleset.ValueString;
        }

        public static implicit operator int(RankStatus ruleset)
        {
            return ruleset.ValueInt;
        }

        public static bool operator==(RankStatus lhs, RankStatus rhs)
        {
            return lhs.ValueInt == rhs.ValueInt;
        }

        public static bool operator!=(RankStatus lhs, RankStatus rhs)
        {
            return lhs.ValueInt != rhs.ValueInt;
        }
    }
}
