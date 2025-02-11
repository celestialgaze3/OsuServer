using OsuServer.Util;

namespace OsuServer.Objects
{
    public class Presence
    {
        public int UtcOffset { get; set; }
        public Geolocation Geolocation { get; private set; }
        public PresenceFilter Filter { get; set; }

        public Presence(int utcOffset, Geolocation geolocation, PresenceFilter filter)
        {
            UtcOffset = utcOffset;
            Geolocation = geolocation;
            Filter = filter;
        }
    }
}
