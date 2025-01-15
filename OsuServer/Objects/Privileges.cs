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

        public int GetIntValue()
        {
            int value = 0;
            value = value | (Player ? 1 : 0) << 0;
            value = value | (Moderator ? 1 : 0) << 1;
            value = value | (Supporter ? 1 : 0) << 2;
            value = value | (Owner ? 1 : 0) << 3;
            value = value | (Developer ? 1 : 0) << 4;
            value = value | (Tournament ? 1 : 0) << 5;
            return value;
        }
    }
}
