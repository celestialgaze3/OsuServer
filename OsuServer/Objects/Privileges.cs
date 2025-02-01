namespace OsuServer.Objects
{
    public class Privileges
    {
        public bool Player { get; set; }
        public bool Moderator { get; set; }
        public bool Supporter { get; set; }
        public bool Owner { get; set; }
        public bool Developer { get; set; }
        public bool Tournament { get; set; }
        public int IntValue { 
            get
            {
                int value = 0;
                value |= (Player ? 1 : 0) << 0;
                value |= (Moderator ? 1 : 0) << 1;
                value |= (Supporter ? 1 : 0) << 2;
                value |= (Owner ? 1 : 0) << 3;
                value |= (Developer ? 1 : 0) << 4;
                value |= (Tournament ? 1 : 0) << 5;
                return value;
            } 
        }

        public Privileges()
        {
            Player = true;
        }

        public Privileges(bool player, bool moderator, bool supporter, bool owner, bool developer, bool tournament)
        {
            Player = player;
            Moderator = moderator;
            Supporter = supporter;
            Owner = owner;
            Developer = developer;
            Tournament = tournament;
        }

    }
}
