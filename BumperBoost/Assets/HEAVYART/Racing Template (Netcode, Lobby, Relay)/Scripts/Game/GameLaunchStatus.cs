namespace HEAVYART.Racing.Netcode
{
    public enum GameLaunchStatus
    {
        WaitingForPlayersToConnect,
        WaitingForPlayersToInitialize,
        WaitingForPlayersResponses,
        WaitingForJoin,
        ReadyToLaunch,
        UnableToLaunch
    }
}