namespace Inventart.Config
{
    public class GlobalConfig
    {
        //Global static stuff
        public const string WwwRootModelFolder = "models";

        //read from settings
        public string BaseUrl { get; set; }

        public string VerificationPrefix { get; set; }
    }
}