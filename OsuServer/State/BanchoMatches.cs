using OsuServer.Objects;
using System.Diagnostics.CodeAnalysis;

namespace OsuServer.State
{
    public class BanchoMatches
    {
        private Bancho _bancho { get; }
        public SortedDictionary<short, Match> All { get; } = [];

        public BanchoMatches(Bancho bancho) 
        {
            _bancho = bancho;
        }

        /// <summary>
        /// Creates a new match
        /// </summary>
        /// <returns>The match created</returns>
        public Match? Create(OnlinePlayer creator, MatchData data)
        {
            short? availableId = GetAvailableId();
            if (availableId == null) return null; // No available IDs

            Match match = new((short)availableId, creator.Id, data, _bancho);
            All.Add(match.Id, match);

            creator.JoinMatch(match);
            match.BroadcastCreation();
            return match;
        }

        public short? GetAvailableId()
        {
            // Find an available ID. To do this quickly, let's try to find gaps
            int availableId = -1;
            int lastEntryId = 0;
            foreach (var entry in All)
            {
                int id = entry.Key;
                if (id > 0) // The ID below this one is available
                {
                    availableId = id - 1;
                    break;
                }

                // Need to go to next entry to find a gap
                if (lastEntryId == 0)
                {
                    lastEntryId = id;
                    continue;
                }

                int gapToLastEntry = id - lastEntryId;

                // A gap exists we can fill
                if (gapToLastEntry > 1) 
                {
                    availableId = id - 1; // Fill the gap
                    break;
                }

                lastEntryId = id;
            }

            // No gaps found, increment ID at the top
            if (availableId == -1)
                availableId = (short)(lastEntryId + 1);

            // No IDs left (this will likely never happen)
            if (availableId > short.MaxValue)
                return null;

            return (short)availableId;
        }

        public void Remove(Match match)
        {
            All.Remove(match.Id);
        }

        public bool TryGetValue(int id, [NotNullWhen(true)] out Match? match)
        {
            return All.TryGetValue((short)id, out match);
        }
    }
}
