using OsuServer.Util;

namespace OsuServer.Objects
{
    public class Presence
    {
        public int UtcOffset { get; set; } = 0;
        public Geolocation Geolocation { get; private set; } = new Geolocation();
        public PresenceFilter Filter { get; set; }

        public Presence() { }
    }
}
