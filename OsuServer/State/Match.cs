using OsuServer.API.Packets.Server;
using OsuServer.Objects;
using System.Numerics;
using System.Reflection.Metadata.Ecma335;
using static OsuServer.Objects.MatchSlot;

namespace OsuServer.State
{
    public class Match
    {
        private Bancho _bancho;

        public short Id { get; }
        public bool IsInProgress { get; set; }
        public Mods Mods { get; set; }
        public string Name { get; set; }
        public string Password { get; set; }
        public int BeatmapId { get; private set; }
        public string BeatmapMD5 { get; private set; }
        public string BeatmapName { get; private set; }
        public int HostId { get; set; }
        public byte Mode { get; set; }
        public MatchWinCondition WinCondition { get; set; }
        public MatchTeamMode TeamMode { get; private set; }
        public bool IsFreemod { get; private set; }
        public int Seed { get; set; }
        public MatchSlot[] Slots { get; }
        public bool IsEmpty { get; private set; }

        public Channel Channel;

        public Match(short id, int hostId, MatchData data, Bancho bancho)
        {
            _bancho = bancho;
            Id = id;
            IsInProgress = false;
            Mods = new Mods();
            Name = data.Name;
            Password = data.Password;
            BeatmapName = data.BeatmapName;
            BeatmapId = data.BeatmapId;
            BeatmapMD5 = data.BeatmapMD5;
            HostId = hostId;
            Mode = data.Mode;
            WinCondition = (MatchWinCondition)data.WinCondition;
            TeamMode = (MatchTeamMode)data.TeamMode;
            IsFreemod = data.IsFreemod;
            Slots = new MatchSlot[16];
            for (int i = 0; i < Slots.Length; i++)
            {
                Slots[i] = new(this)
                {
                    Status = (SlotStatus)data.Statuses[i]
                };
            }
            Seed = data.Seed;
            Channel = new MatchChannel(this, bancho);
            IsEmpty = false;
        }

        /// <summary>
        /// Broadcasts this match's creation to players in the lobby
        /// </summary>
        public void BroadcastCreation()
        {
            foreach (var player in _bancho.GetPlayers())
            {
                if (player.IsInLobby)
                    player.Connection.AddPendingPacket(new MatchCreatePacket(this));
            }

        }

        /// <summary>
        /// Broadcasts this match's deletion to players in the lobby
        /// </summary>
        public void BroadcastDeletion()
        {
            foreach (var player in _bancho.GetPlayers())
            {
                if (player.IsInLobby)
                    player.Connection.AddPendingPacket(new MatchRemovePacket(Id));
            }

        }

        /// <summary>
        /// Adds a player to this match if a slot is empty
        /// </summary>
        /// <param name="player">The player to join</param>
        /// <returns>Whether or not the join was successful</returns>
        public bool Add(OnlinePlayer player)
        {
            int i = 0;
            // Find first empty slot to join
            foreach (var slot in Slots)
            {
                i++;
                if (slot.Status == SlotStatus.Open)
                {
                    slot.Player = player;
                    slot.Status = SlotStatus.NotReady;
                    if (player.JoinChannel(Channel))
                    {
                        player.Connection.AddPendingPacket(new MatchJoinSuccessPacket(this));
                        return true;
                    } else
                    {
                        player.Connection.AddPendingPacket(new MatchJoinFailPacket(_bancho));
                        return false;
                    }
                }
            }

            // No empty slots found
            player.Connection.AddPendingPacket(new MatchJoinFailPacket(_bancho));
            return false;
        }

        /// <summary>
        /// Attempts to join this lobby with the given password.
        /// </summary>
        /// <param name="player">The player attemping to join</param>
        /// <param name="password">The password given by the player</param>
        /// <returns>Whether or not the join was successful</returns>
        public bool TryAdd(OnlinePlayer player, string password)
        {
            if (Password.Length != 0 && !password.Equals(Password))
            {
                player.Connection.AddPendingPacket(new MatchJoinFailPacket(_bancho));
                return false;
            }

            return Add(player);
        }

        /// <summary>
        /// Removes a player from this lobby
        /// </summary>
        /// <param name="player">The player to remove</param>
        /// <returns>Whether or not the player was found and removed</returns>
        public bool Remove(OnlinePlayer player)
        {
            // Clear the slot the player was in
            bool foundAndRemoved = false;
            foreach (var slot in Slots) 
            {
                if (slot.Player == player)
                {
                    // Reset slot state
                    slot.Clear();
                    foundAndRemoved = true;
                }
            }

            if (!foundAndRemoved) return false;

            // If the player was the host of the lobby, transfer host to another player
            OnlinePlayer? newHost = null;
            foreach (var slot in Slots)
            {
                if (slot.Player == null) continue;
                newHost = slot.Player;
                break;
            }

            // Give the new player host
            if (newHost != null)
            {
                HostId = newHost.Id;
                BroadcastUpdate();
            }
            else // If no other player was found, this match is now empty. Remove it from the listing
            {
                _bancho.Matches.Remove(this);
                BroadcastDeletion();
                IsEmpty = true;
            }

            return true;
        }

        /// <summary>
        /// Enables freemod for this lobby
        /// </summary>
        public void EnableFreemod()
        {
            if (IsFreemod) return;

            IsFreemod = true;

            // After enabling freemod, each player takes on the mods of the match itself, except speed-changing mods
            foreach (var slot in Slots)
            {
                if (slot.Player != null)
                    slot.Mods.IntValue = Mods.IntValue & ~Mods.SpeedChangingMods;
            }

            // All mods are removed from the match, except speed-changing mods
            Mods.IntValue &= Mods.SpeedChangingMods;
        }

        /// <summary>
        /// Disables freemod for this lobby
        /// </summary>
        public void DisableFreemod()
        {
            if (!IsFreemod) return;

            IsFreemod = false;

            // After disabling freemod, each player's mods are removed
            foreach (var slot in Slots)
            {
                if (slot.Player != null)
                    slot.Mods.IntValue = (int)Mod.None;
            }

            // The match takes on the host's mods
            MatchSlot hostSlot = GetHostSlot();
            Mods.IntValue |= hostSlot.Mods.IntValue;
        }

        /// <returns>The slot the host is in</returns>
        /// <exception cref="InvalidDataException">Thrown when the host cannot be found</exception>
        public MatchSlot GetHostSlot()
        {
            MatchSlot? slot = GetSlotWithPlayer(HostId);
            if (slot != null)
                return slot;

            // There are no circumstances where a match doesn't have a host
            throw new InvalidDataException("Match must have a host");
        }

        /// <param name="playerId">The player's ID</param>
        /// <returns>The slot the player is in, or null if they are not in a slot</returns>
        public MatchSlot? GetSlotWithPlayer(int playerId)
        {
            foreach (var slot in Slots)
            {
                if (slot.Player != null && slot.Player.Id == playerId)
                {
                    return slot;
                }
            }

            return null;
        }

        /// <param name="playerId">The player's ID</param>
        /// <returns>The ID of the slot the player is in</returns>
        public byte? GetSlotIdWithPlayer(int playerId)
        {
            for (byte i = 0; i < Slots.Length; i++)
            {
                MatchSlot slot = Slots[i];
                if (slot.Player != null && slot.Player.Id == playerId)
                {
                    return i;
                }
            }

            return null;
        }

        /// <summary>
        /// Called when the map of this match is being changed by the host
        /// </summary>
        public void MapBeingChanged()
        {
            // All ready players become unreadied
            foreach (var slot in Slots)
            {
                if (slot.Status == SlotStatus.Ready)
                    slot.Status = SlotStatus.NotReady;
            }

            BeatmapId = -1;
            BeatmapMD5 = string.Empty;
            BeatmapName = string.Empty;
        }

        /// <summary>
        /// Set the beatmap of this match
        /// </summary>
        /// <param name="beatmapId">The beatmap's ID</param>
        /// <param name="beatmapMD5">The beatmap's hash</param>
        /// <param name="beatmapName">The beatmap's name</param>
        public void SetBeatmap(int beatmapId, string beatmapMD5, string beatmapName)
        {
            BeatmapId = beatmapId;
            BeatmapMD5 = beatmapMD5;
            BeatmapName = beatmapName;
        }

        /// <summary>
        /// Changes the team mode of this match
        /// </summary>
        /// <param name="mode">The mode to change to</param>
        public void ChangeTeamMode(MatchTeamMode mode)
        {
            if (mode == TeamMode) return;
            bool changingToTeamedMode = (mode == MatchTeamMode.TeamVs || mode == MatchTeamMode.TagTeamVs);

            if (changingToTeamedMode)
            {
                foreach (var slot in Slots)
                {
                    // Default team is red
                    slot.Team = SlotTeam.Red;
                }
            } 
            else
            {
                foreach (var slot in Slots)
                {
                    slot.Team = SlotTeam.None;
                }
            }

            TeamMode = mode;
        }

        /// <summary>
        /// Broadcasts the state of this match to all players in lobby or in this match
        /// </summary>
        public void BroadcastUpdate()
        {
            if (IsEmpty) return; // This lobby will be deleted soon
            foreach (var player in _bancho.GetPlayers())
            {
                if (!player.IsInLobby) continue;
                SendUpdate(player);
            }

            foreach (var slot in Slots)
            {
                if (slot.Player == null) continue;
                SendUpdate(slot.Player);
            }
        }

        public void SendUpdate(OnlinePlayer player)
        {
            player.Connection.AddPendingPacket(new MatchUpdatePacket(this, false));
        }

        /// <summary>
        /// Changes a player's slot status to the given status
        /// </summary>
        /// <param name="player">The player</param>
        /// <param name="status">The slot status</param>
        public void ChangePlayerSlotStatus(OnlinePlayer player, SlotStatus status)
        {
            // Get the current slot
            MatchSlot? current = GetSlotWithPlayer(player.Id);
            if (current == null) return;

            // Change status
            current.Status = status;
        }

        /// <param name="index">The index (0-15) of the slot to get</param>
        /// <returns>The <see cref="MatchSlot"/> at the index</returns>
        public MatchSlot? GetSlot(int index)
        {
            // Ignore invalid slot positions
            if (index < 0 || index >= Slots.Length)
                return null;

            return Slots[index];
        }

        /// <summary>
        /// Changes a player's slot to the given index (if it is open)
        /// </summary>
        /// <param name="player">The player to change the slot of</param>
        /// <param name="destinationIndex">The destination slot's index</param>
        public void PlayerChangeSlots(OnlinePlayer player, MatchSlot destination)
        {
            // Ignore invalid slot destinations
            if (destination.Status != SlotStatus.Open)
                return;

            // Get the current slot
            MatchSlot? current = GetSlotWithPlayer(player.Id);
            if (current == null) return; // TODO: would be weird

            // Change slots
            destination.CopyFrom(current);
            current.Clear();
        }

        public void ToggleLock(MatchSlot slot)
        {
            // Host cannot kick themselves
            if (slot.Player != null && slot.Player.Id == HostId)
                return;

            // If slot is unlocked, lock it
            if (slot.Status != SlotStatus.Locked)
            {
                // Clear the slot (kick any player)
                slot.Clear();
                slot.Status = SlotStatus.Locked;
            }
            else // Slot is locked, unlock the slot
            {
                slot.Status = SlotStatus.Open;
            }
        }

        /// <summary>
        /// Toggles the team of a slot between red and blue
        /// </summary>
        /// <param name="slot">The slot to toggle the team of</param>
        public void ToggleTeam(MatchSlot slot)
        {
            // Toggle teams
            if (slot.Team == SlotTeam.Red)
            {
                slot.Team = SlotTeam.Blue;
            }
            else
            {
                slot.Team = SlotTeam.Red;
            }
        }

        /// <summary>
        /// Starts this match
        /// </summary>
        public void Start()
        {
            IsInProgress = true;

            foreach (var slot in Slots)
            {
                if (slot.Player == null) continue;

                // Ignore players who don't have the map
                if (slot.Status == SlotStatus.NoMap) continue;

                // Let them know this match has started
                slot.Player.Connection.AddPendingPacket(new MatchStartPacket(this));
                
                // Set new status to playing
                slot.Status = SlotStatus.Playing;
            }
        }

        /// <summary>
        /// Broadcasts a player's score data to this match
        /// </summary>
        /// <param name="player">The player that is broadcasting their score data</param>
        /// <param name="scoreData">The score data itself</param>
        public void BroadcastLiveScoreData(OnlinePlayer player, LiveScoreData scoreData)
        {
            byte? playerSlotId = GetSlotIdWithPlayer(player.Id);
            if (playerSlotId == null) return;

            scoreData.Id = (byte)playerSlotId;
            foreach (var slot in Slots)
            {
                if (slot.Player == null) continue;
                if (slot.Status != SlotStatus.Playing) continue;
                slot.Player.Connection.AddPendingPacket(new MatchScoreUpdatePacket(scoreData));
            }
        }

        /// <summary>
        /// Marks a player as loaded in this match
        /// </summary>
        /// <param name="player">The player that has loaded</param>
        public void MarkAsLoaded(OnlinePlayer player)
        {
            if (!IsInProgress) return;

            MatchSlot? playerSlot = GetSlotWithPlayer(player.Id);
            if (playerSlot == null) return;

            // Mark the player as loaded
            playerSlot.HasLoaded = true;

            // Check if all players are loaded
            bool allPlayersLoaded = true;
            foreach (var slot in Slots)
            {
                if (slot.Status != SlotStatus.Playing) continue;
                if (!slot.HasLoaded)
                {
                    allPlayersLoaded = false;
                    break;
                }
            }

            // If all players are loaded, let participating clients know
            if (!allPlayersLoaded) return;
            foreach (var slot in Slots)
            {
                if (slot.Player == null) continue;
                if (slot.Status != SlotStatus.Playing) continue;

                slot.Player.Connection.AddPendingPacket(new MatchPlayersLoadedPacket(_bancho));
            }
        }

        /// <summary>
        /// Broadcasts a fail to this match
        /// </summary>
        /// <param name="player">The player that failed</param>
        public void BroadcastPlayerFail(OnlinePlayer player)
        {
            MatchSlot? playerSlot = GetSlotWithPlayer(player.Id);
            if (playerSlot == null) return;

            for (byte i = 0; i < Slots.Length; i++)
            {
                MatchSlot slot = Slots[i];
                if (slot.Player == null) continue;
                if (slot.Status != SlotStatus.Playing) continue;

                slot.Player.Connection.AddPendingPacket(new MatchPlayerFailedPacket(i));
            }
        }

        /// <summary>
        /// Marks a player as completed
        /// </summary>
        /// <param name="player">The player that has completed a map</param>
        public void MarkAsComplete(OnlinePlayer player)
        {
            if (!IsInProgress) return;

            MatchSlot? playerSlot = GetSlotWithPlayer(player.Id);
            if (playerSlot == null) return;

            // Mark the player as complete
            playerSlot.Status = SlotStatus.Complete;

            // Check if all players have completed
            bool allPlayersCompleted = true;
            foreach (var slot in Slots)
            {
                if (slot.Status == SlotStatus.Playing)
                {
                    allPlayersCompleted = false;
                    break;
                }
            }

            // If all players have completed, let participating clients know
            if (!allPlayersCompleted) return;
            foreach (var slot in Slots)
            {
                if (slot.Player == null) continue;
                if (slot.Status != SlotStatus.Complete) continue;

                slot.Status = SlotStatus.NotReady;
                slot.Player.Connection.AddPendingPacket(new MatchCompletePacket(_bancho));
            }

            IsInProgress = false;
        }

        /// <summary>
        /// Changes a player's mods
        /// </summary>
        /// <param name="player">The player</param>
        /// <param name="mods">The new mods</param>
        public void PlayerChangeMods(OnlinePlayer player, Mods mods)
        {
            MatchSlot? slot = GetSlotWithPlayer(player.Id);
            if (slot == null) return;

            if (IsFreemod)
            {
                // If the player is host, we want to propagate speed-changing mods to the match itself.
                if (player.Id == HostId)
                {
                    Mods = new(mods.IntValue & Mods.SpeedChangingMods);
                }

                // Set the slot's mods to exclude speed-changing mods
                slot.Mods = new(mods.IntValue & ~Mods.SpeedChangingMods);
            }
            else if (player.Id == HostId)
            {
                Mods = mods;
            }
        }

        /// <summary>
        /// Marks a player as skipped, and broadcasts a skip to all participating players if all players have skipped
        /// </summary>
        /// <param name="player"></param>
        public void MarkAsSkipped(OnlinePlayer player)
        {
            if (!IsInProgress) return;

            MatchSlot? playerSlot = GetSlotWithPlayer(player.Id);
            int? playerSlotId = GetSlotIdWithPlayer(player.Id);
            if (playerSlot == null || playerSlotId == null) return;

            // Mark the player as skipped
            playerSlot.HasSkipped = true;

            // Broadcast this skip to participating clients
            foreach (var slot in Slots)
            {
                if (slot.Player == null) continue;
                if (slot.Status != SlotStatus.Playing) continue;

                slot.Player.Connection.AddPendingPacket(new MatchPlayerSkippedPacket((int)playerSlotId));
            }

            // Check if all players are skipped
            bool allPlayersSkipped = true;
            foreach (var slot in Slots)
            {
                if (slot.Player == null) continue;
                if (slot.Status != SlotStatus.Playing) continue;
                if (!slot.HasSkipped)
                {
                    allPlayersSkipped = false;
                    break;
                }
            }

            // If all players are skipped, let participating clients know
            if (!allPlayersSkipped) return;
            foreach (var slot in Slots)
            {
                if (slot.Player == null) continue;
                if (slot.Status != SlotStatus.Playing) continue;

                slot.Player.Connection.AddPendingPacket(new MatchSkipPacket(_bancho));
            }
        }

        public void ChangeHost(int slotId)
        {
            MatchSlot? slot = GetSlot(slotId);
            if (slot == null) return;
            if (slot.Player == null) return;
            HostId = slot.Player.Id;
        }

        /// <summary>
        /// Extracts the data out of this match for communication with the osu! client
        /// </summary>
        public MatchData Data
        {
            get
            {
                byte[] statuses = new byte[16];
                byte[] teams = new byte[16];
                int[] mods = new int[16];
                List<int> playersInLobby = [];
                for (int i = 0; i < Slots.Length; i++)
                {
                    MatchSlot slot = Slots[i];
                    statuses[i] = (byte)slot.Status;
                    teams[i] = (byte)slot.Team;
                    mods[i] = slot.Mods.IntValue;
                    if (slot.Player != null)
                        playersInLobby.Add(slot.Player.Id);
                }

                return new MatchData(Id, IsInProgress, Mods.IntValue, Name, Password, BeatmapName, BeatmapId, BeatmapMD5,
                    statuses, teams, playersInLobby, HostId, Mode, (byte)WinCondition, (byte)TeamMode, IsFreemod, mods, Seed);
            }
        }

        public enum MatchWinCondition
        {
            Score = 0,
            Accuracy = 1,
            Combo = 2,
            ScoreV2 = 3
        }

        public enum MatchTeamMode
        {
            HeadToHead = 1,
            TagCoop = 2,
            TeamVs = 3,
            TagTeamVs = 4
        }
    }
}
