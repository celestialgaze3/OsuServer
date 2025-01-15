namespace OsuServer.API
{
    public class LoginData
    {
        // Login information
        public string Username { get; private set; } // Username of the user logging in
        public string Password { get; private set; } // Password (hashed with md5)

        // Client information
        public string Version { get; private set; } // osu! version the client is running 
        public int UtcOffset { get; private set; } // user's timezone
        public bool ShouldDisplayCity { get; private set; } // Ingame setting: Share your city location with others
        public bool DisallowPrivateMessages { get; private set; } // Ingame setting: Block private messages from non-friends

        // Various hashes, not sure what most of these are but some are used for verifying score submissions
        public string OsuPathMD5 { get; private set; } // osu! path (hashed with md5)
        public string Adapters { get; private set; }
        public string AdaptersMD5 { get; private set; }
        public string UninstallMD5 { get; private set; }
        public string DiskSignatureMD5 { get; private set; }

        public LoginData(string requestBody)
        {
            /* Format is:
            Username\nPassword\nVersion|UtcOffset|DisplayCity|OsuPathMD5:Adapters:AdaptersMD5:UninstallMD5:DiskSignatureMD5|PmPrivate */

            string[] args = requestBody.Split("\n");
            Username = args[0];
            Password = args[1];

            string remainder = args[2];
            string[] remainderArgs = remainder.Split("|");
            Version = remainderArgs[0];
            UtcOffset = Convert.ToInt32(remainderArgs[1]);
            ShouldDisplayCity = remainderArgs[2] == "1";
            DisallowPrivateMessages = remainderArgs[4] == "1";

            string hashes = remainderArgs[3];
            string[] hashesArgs = hashes.Split(":");
            OsuPathMD5 = hashesArgs[0];
            Adapters = hashesArgs[1];
            AdaptersMD5 = hashesArgs[2];
            UninstallMD5 = hashesArgs[3];
            DiskSignatureMD5 = hashesArgs[4];
        }

        public void PrintData()
        {
            Console.WriteLine("Username: " + Username);
            Console.WriteLine("Password: " + Password);
            Console.WriteLine("Version: " + Version);
            Console.WriteLine("UtcOffset: " + UtcOffset);
            Console.WriteLine("ShouldDisplayCity: " + ShouldDisplayCity);
            Console.WriteLine("DisallowPrivateMessages: " + DisallowPrivateMessages);
            Console.WriteLine("OsuPathMD5: " + OsuPathMD5);
            Console.WriteLine("Adapters: " + Adapters);
            Console.WriteLine("AdaptersMD5: " + AdaptersMD5);
            Console.WriteLine("UninstallMD5: " + UninstallMD5);
            Console.WriteLine("DiskSignatureMD5: " + DiskSignatureMD5);
        }

    }
}
