using System.Collections.Generic;

namespace FreieWahl.Application.Registrations
{
    public class RegistrationChallenge
    {
        public RegistrationChallenge(string challenge, List<string> tokens)
        {
            Challenge = challenge;
            Tokens = tokens;
        }

        public string Challenge { get; }

        public List<string> Tokens { get; }
    }
}
