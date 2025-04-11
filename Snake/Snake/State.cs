namespace Snake
{
    //A position struct to hold X and Y coords.
    public class PositionState
    {
        public int X { get; set; }
        public int Y { get; set; }
    }
    //GameState object to hold clients and server parameters to keep and use.
    public class GameState
    {
        //Server's position
        public PositionState ServerState { get; set; }
        //Client's position
        public PositionState ClientState { get; set; }
        //The username assigned to the client.
        public string usernameClient = "Client";
        //The username assigned to the server.
        public string usernameServer = "Server";
    }
}
