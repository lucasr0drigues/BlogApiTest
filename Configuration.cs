namespace BlogApi
{
    public static class Configuration
    {
        public static string JwtKey = "435GSDg34yuSDgdws32%RAfas32634SAFSDF@gdfy45ySD#%";
        public static string ApiKeyName = "api_key";
        public static string ApiKey = "curso_api_5892759fsdjkdshjk3245k";
        public static SmtpConfiguration Smtp = new();

        public class SmtpConfiguration
        {
            public string Host { get; set; }
            public int Port { get; set; } = 25;
            public string UserName { get; set; }
            public string Password { get; set; }
        }
    }
}
