﻿using System;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Snake
{
    public class Client
    {
        // Represents the TCP client used for network communication
        private TcpClient tcpClient;
        // Represents the network stream for sending and receiving data
        private NetworkStream stream;
        // Stores the IP address of the server to connect to
        private readonly string hostIP;
        // Stores the port number of the server to connect to
        private readonly int port;
        // Indicates whether the client is currently connected to the server
        private bool isConnected = false;

        // Constructor - takes server IP and port as parameters with defaults
        public Client(string hostIP = "127.0.0.1", int port = 8888)
        {
            this.hostIP = hostIP;
            this.port = port;
        }

        // Connects to the server asynchronously
        public async Task ConnectAsync()
        {
            try
            {
                // Creates a new TCP client instance
                tcpClient = new TcpClient();
                // Attempts to connect to the server at the specified IP address and port asynchronously
                await tcpClient.ConnectAsync(hostIP, port);
                // Disables Nagle's algorithm to potentially reduce latency for small packets
                tcpClient.NoDelay = true;
                // Gets the network stream for the connected client
                stream = tcpClient.GetStream();
                // Sets the connection status to true
                isConnected = true;
                // Writes a message to the console indicating a successful connection
                Console.WriteLine($"[Client] Connected to {hostIP}:{port}");
            }
            catch (Exception ex)
            {
                // Catches any exceptions that occur during the connection process
                Console.WriteLine($"[Client] Connection failed: {ex.Message}");
                // Re-throws the exception to be handled by the calling code
                throw;
            }
        }

        // Special method to receive the initial START signal from the server
        public async Task<string> ReceiveStartSignalAsync()
        {
            byte[] buffer = new byte[1024];
            try
            {
                int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
                if (bytesRead > 0)
                {
                    string data = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    return data;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Client] Receive error: {ex.Message}");
                throw;
            }
            return null;
        }

        // Receives the game state from the server asynchronously
        public async Task<GameState> ReceiveGameStateAsync()
        {
            // Creates a buffer to store the incoming data
            byte[] buffer = new byte[1024];
            try
            {
                // Asynchronously reads data from the network stream into the buffer
                int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
                // Checks if any data was received
                if (bytesRead > 0)
                {
                    // Decodes the received byte array into a string using UTF-8 encoding
                    string data = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    // Deserializes the received string data into a GameState object
                    return DeserializeGameState(data);
                }
            }
            catch (Exception ex)
            {
                // Catches any exceptions that occur during the receive operation
                Console.WriteLine($"[Client] Receive error: {ex.Message}");
                throw;
            }
            // Returns null if no data was received or an error occurred
            return null;
        }

        // Sends the local game state to the server asynchronously
        public async Task SendGameStateAsync(GameState state)
        {
            try
            {
                // Serializes the current local game state into a string
                string data = SerializeGameState(state);
                // Encodes the string data into a byte array using UTF-8 encoding
                byte[] buffer = Encoding.UTF8.GetBytes(data);
                // Asynchronously writes the byte array to the network stream
                await stream.WriteAsync(buffer, 0, buffer.Length);
            }
            catch (Exception ex)
            {
                // Catches any exceptions that occur during the send operation
                Console.WriteLine($"[Client] Send error: {ex.Message}");
                // Re-throws the exception to be handled by the calling code
                throw;
            }
        }

        // Serializes a GameState object into a comma-separated string
        private string SerializeGameState(GameState state) =>
            $"{state.ServerState.X},{state.ServerState.Y},{state.ClientState.X},{state.ClientState.Y},{state.usernameClient},{state.usernameServer}";

        // Deserializes a comma-separated string into a GameState object
        private GameState DeserializeGameState(string data)
        {
            // Splits the input string into an array of strings based on the comma delimiter
            var parts = data.Split(',');
            // Checks if the array contains the expected number of parts (6 in this case)
            if (parts.Length == 6)
            {
                // Creates a new GameState object and populates its properties by parsing the string parts
                return new GameState
                {
                    ServerState = new PositionState { X = int.Parse(parts[0]), Y = int.Parse(parts[1]) },
                    ClientState = new PositionState { X = int.Parse(parts[2]), Y = int.Parse(parts[3]) },
                    usernameClient = parts[4],
                    usernameServer = parts[5]
                };
            }
            // Returns null if the input string does not have the expected format
            return null;
        }

        // Checks if the client is currently connected to the server
        public bool IsConnected() => tcpClient?.Connected == true;
    }
}