namespace MogglesClient.PublicInterface.Notifications
{
    public class Message
    {
        public string text { get; }

        public Message(string text)
        {
            this.text = text;
        }
    }
}