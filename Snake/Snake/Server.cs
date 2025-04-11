// Server.cs (updated)
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Snake
{
    public class Server
    {
        // Defines the port number the server will listen on.
        private const int PORT = 8888;
        // Represents the listener for incoming TCP connection requests.
        private TcpListener listener;
        // Represents the connected TCP client.
        private TcpClient client;
        // Represents the network stream used to send and receive data.
        private NetworkStream stream;
        // Indicates whether the server is currently running.
        private bool isRunning = false;

        
        // Starts the server and listens for incoming client connections asynchronously.
        public async Task StartAsync()
        {
            try
            {
                // Creates a new TcpListener that listens on any IP address and the specified port.
                listener = new TcpListener(IPAddress.Any, PORT);
                // Starts listening for incoming connection requests.
                listener.Start();
                // Writes a message to the console indicating the server is listening.
                Console.WriteLine($"[Server] Listening on port {PORT}");

                // Accepts an incoming TCP client connection asynchronously.
                client = await listener.AcceptTcpClientAsync();
                // Disables the Nagle algorithm, potentially improving latency for small packets.
                client.NoDelay = true;
                // Gets the network stream for the connected client.
                stream = client.GetStream();
                // Sets the isRunning flag to true, indicating the server is active.
                isRunning = true;

                // Writes a message to the console indicating a client has connected, including the client's IP address.
                Console.WriteLine($"[Server] Client connected from {((IPEndPoint)client.Client.RemoteEndPoint).Address}");
            }
            catch (Exception ex)
            {
                // Catches any exceptions that occur during server startup or client connection.
                Console.WriteLine($"[Server] Error: {ex.Message}");
                // Re-throws the exception to be handled by the calling code.
                throw;
            }
        }

        
        //Sends the current game state to the connected client asynchronously.
        public async Task SendGameStateAsync(GameState state)
        {
            try
            {
                // Creates a comma-separated string containing the relevant game state information.
                string data = $"{state.ServerState.X},{state.ServerState.Y},{state.ClientState.X},{state.ClientState.Y},{state.usernameClient},{state.usernameServer}";
                // Encodes the string data into a byte array using UTF-8 encoding.
                byte[] buffer = Encoding.UTF8.GetBytes(data);
                // Asynchronously writes the byte array to the network stream.
                await stream.WriteAsync(buffer, 0, buffer.Length);
            }
            catch (Exception ex)
            {
                // Catches any exceptions that occur during the send operation.
                Console.WriteLine($"[Server] Send error: {ex.Message}");
                // Re-throws the exception to be handled by the calling code.
                throw;
            }
        }


        // Receives the game state from the connected client asynchronously.
        public async Task<GameState> ReceiveGameStateAsync()
        {
            // Creates a buffer to store the received data.
            byte[] buffer = new byte[1024];
            try
            {
                // Asynchronously reads data from the network stream into the buffer.
                int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
                // Checks if any data was received.
                if (bytesRead > 0)
                {
                    // Decodes the received byte array into a string using UTF-8 encoding.
                    string receivedData = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    // Deserializes the received string data into a GameState object.
                    return Deserialize(receivedData);
                }
            }
            catch (Exception ex)
            {
                // Catches any exceptions that occur during the receive operation.
                Console.WriteLine($"[Server] Receive error: {ex.Message}");
            }
            // Returns null if no data was received or an error occurred.
            return null;
        }

        // Deserializes a comma-separated string into a GameState object.
        private GameState Deserialize(string data)
        {
            // Splits the input string into an array of strings based on the comma delimiter.
            var parts = data.Split(',');
            // Checks if the array contains the expected number of parts (6 in this case).
            if (parts.Length == 6)
            {
                // Creates a new GameState object and populates its properties by parsing the string parts.
                return new GameState
                {
                    // Parses the first two parts as integers for the server's X and Y coordinates.
                    ServerState = new PositionState { X = int.Parse(parts[0]), Y = int.Parse(parts[1]) },
                    // Parses the next two parts as integers for the client's X and Y coordinates.
                    ClientState = new PositionState { X = int.Parse(parts[2]), Y = int.Parse(parts[3]) },
                    // Assigns the fifth part as the client's username.
                    usernameClient = parts[4],
                    // Assigns the sixth part as the server's username.
                    usernameServer = parts[5]
                };
            }
            // Returns null if the input string does not have the expected format.
            return null;
        }

        // Checks if the client is currently connected.
        public bool IsConnected() => client?.Connected == true;
    }
}