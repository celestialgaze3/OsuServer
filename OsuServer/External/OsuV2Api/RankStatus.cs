namespace OsuServer.External.OsuV2Api
{
    public class RankStatus
    {
        public static RankStatus Graveyard => new(-2, "graveyard");
        public static RankStatus Wip => new(-1, "wip");
        public static RankStatus Pending => new(0, "pending");
        public static RankStatus Ranked => new(1, "ranked");
        public static RankStatus Approved => new(2, "approved");
        public static RankStatus Qualified => new(3, "qualified");
        public static RankStatus Loved => new(4, "loved");

        public static RankStatus[] Values = { Graveyard, Wip, Pending, Ranked, Approved, Qualified, Loved };

        public int ValueInt { get; private set; }
        public string ValueString { get; private set; }

        /// <summary>
        /// Adjusts the integer value of this status to align with what the osu! client
        /// expects to see in a score get-scores response.
        /// </summary>
        public int ValueGetScores
        {
            get
            {
                if (ValueInt <= 0)
                    return 0; // Pending

                // ValueInt >= 1 here
                return ValueInt + 1; // offset by "UpdateAvailable"
            }
        }

        /// <summary>
        /// Whether or not the client will submit a score for a beatmap with this ranked status.
        /// </summary>
        public bool ClientSubmitsScores
        {
            get {
                return ValueInt > 0;
            }
        }

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

        public static RankStatus FromOsuDirect(int integer)
        {
            if (integer == 0)
                return RankStatus.Ranked;
            if (integer == 2)
                return RankStatus.Pending;
            if (integer == 3)
                return RankStatus.Qualified;
            // 4 is all
            if (integer == 5)
                return RankStatus.Graveyard;
            if (integer == 7)
                return RankStatus.Ranked; // played before
            if (integer == 8)
                return RankStatus.Loved;

            throw new ArgumentException($"Argument {integer} does not match a value provided by osu!direct");
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
