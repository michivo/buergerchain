using System;

namespace FreieWahl.Voting.Common
{
    [Flags]
    public enum RegistrationType
    {
        Buergerkarte = 0x01,
        Sms = 0x02
    }
}
