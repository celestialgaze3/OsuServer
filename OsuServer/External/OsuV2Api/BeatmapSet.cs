using Newtonsoft.Json.Linq;

namespace OsuServer.External.OsuV2Api
{
    public class BeatmapSet
    {
        public string Artist { get; set; }
        public string ArtistUnicode { get; set; }
        public string Covers { get; set; } // TODO: implement "covers" but idgaf so
        public string Creator { get; set; }
        public int FavouriteCount { get; set; }
        public int Id { get; set; }
        public bool IsNsfw { get; set; }
        public int Offset { get; set; }
        public int PlayCount { get; set; }
        public string PreviewUrl { get; set; }
        public string Source { get; set; }
        public string Status { get; set; }
        public string Spotlight { get; set; }
        public string Title { get; set; }
        public string TitleUnicode { get; set; }
        public int UserId { get; set; }
        public bool HasVideo { get; set; }

        public BeatmapSet(JToken json)
        {
            Artist = (string?)json["artist"];
            ArtistUnicode = (string?)json["artist_unicode"];
            // TODO: Covers seems to contain varying sizes of the background. might be needed if i ever make a frontend, but for now IDC enough to implement this
            //Covers = json["covers"];
            Creator = (string?)json["creator"];
            FavouriteCount = (int)json["favourite_count"];
            Id = (int)json["id"];
            IsNsfw = (bool)json["nsfw"];
            Offset = (int)json["offset"];
            PlayCount = (int)json["play_count"];
            PreviewUrl = (string?)json["preview_url"];
            Source = (string?)json["source"];
            Status = (string?)json["status"];
            Spotlight = (string?)json["spotlight"];
            Title = (string?)json["title"];
            TitleUnicode = (string?)json["title_unicode"];
            UserId = (int)json["user_id"];
            HasVideo = (bool)json["video"];
        }
    }
}
