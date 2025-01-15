namespace OsuServer.Objects
{
    public class OsuMessage
    {
        public string Sender { get; set; }
        public string Text { get; set; }
        public string Recipient { get; set; }
        public int SenderId { get; set; }

        public OsuMessage(string sender, string text, string recipient, int senderId)
        {
            Sender = sender;
            Text = text;
            Recipient = recipient;
            SenderId = senderId;
        }
    }
}
