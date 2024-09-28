using System;

namespace RegistracjaTEST
{
    public class RegistrationConfig
    {
        public string Url { get; set; }
        public bool RequiresPin { get; set; }
        public string EmailSubject { get; set; }
        public Func<string, string> ExtractActivationLink { get; set; }
    }
}