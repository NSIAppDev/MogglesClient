using Newtonsoft.Json;

namespace MogglesClient.PublicInterface.Notifications
{
    public class WorkflowMessage
    {
        [JsonProperty("title")]
        public string Title { get; set; }
        [JsonProperty("text")]

        public string Text { get; set; }
    }
}
