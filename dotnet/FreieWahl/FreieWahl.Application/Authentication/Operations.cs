namespace FreieWahl.Application.Authentication
{
    // enum containing operations that may require some form of authentication
    public enum Operation
    {
        Read = 0x01,
        Create = 0x02,
        UpdateVoting = 0x04,
        UpdateQuestion = 0x08,
        DeleteQuestion = 0x10,
        DeleteVoting = 0x20,
        Invite = 0x40,
        GrantRegistration = 0x80,
        List = 0x100,
        EditUser = 0x200,
    }
}