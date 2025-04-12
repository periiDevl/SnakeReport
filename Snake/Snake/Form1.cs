
using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Snake
{
    public partial class Form1 : Form
    {
        // Represents the server-side snake.
        private Snake snake_server;
        // Represents the client-side snake.
        private Snake snake_client;
        // Manages the game's real-time loop.
        private Realtime realtime;
        // Represents the playing field containing apples.
        private AppleField field;

        // Handles the server instance.
        private Server server;
        // Handles the client instance.
        private Client client;
        // Stores the overall game state, including positions of both snakes.
        private GameState gameState;
        // Flag indicating if this instance is hosting the game.
        private bool isHost = false;
        // Flag indicating if a network connection is established.
        private bool isConnected = false;
        // Lock object for ensuring thread-safe access to shared game state.
        private object stateLock = new object();
        // Flag to track if the apple field has been generated.
        private bool genratedAppleField = false;
        // Label to display the current connection status to the user.
        private Label lblConnection;
        // Stores the username of the local player.
        public string userName = "?";

        // Constructor for the Form1 class.
        public Form1()
        {
            // Initializes the visual components of the form.
            InitializeComponent();
            // Sets the initial width of the game window.
            this.Width = 800;
            // Sets the initial height of the game window.
            this.Height = 600;
            // Enables double buffering to reduce flickering during drawing.
            this.DoubleBuffered = true;

            // Create and position the connection status label (top-left).
            lblConnection = new Label();
            lblConnection.Font = new Font("Arial", 12, FontStyle.Bold);
            lblConnection.ForeColor = Color.Green;
            lblConnection.BackColor = Color.Transparent;
            lblConnection.AutoSize = true;
            lblConnection.Location = new Point(10, 10);
            lblConnection.Text = "Not Connected";
            this.Controls.Add(lblConnection);

            // Create snake objects for both server and client.
            snake_server = new Snake(this);
            // Initializes the server snake with default debug settings.
            snake_server.debug_standard();
            snake_client = new Snake(this);
            // Initializes the client snake with default debug settings.
            snake_client.debug_standard();

            // Create playing field.
            // Note: The initialization of 'field' is moved to the 'Update' method after connection.

            // Start the realtime (game loop) update.
            realtime = new Realtime();
            // Initiates the game loop, calling the 'Update' method at regular intervals.
            realtime.start(Update, this);

            // Initialize game state with default positions for both snakes.
            gameState = new GameState
            {
                ServerState = new PositionState() { X = 0, Y = 0 },
                ClientState = new PositionState() { X = 0, Y = 0 }
            };
        }

        // This method is called repeatedly by the game loop to update the game state and redraw the screen.
        private void Update(object sender, EventArgs e)
        {
            // Sets the title of the form to indicate the developer and game.
            this.Text = "Jonathan Peri : Snake";
            // Temporarily pauses the game loop.
            realtime.suspend();
            // Increments the frame counter for the next update cycle.
            realtime.updateForNextFrame();

            // Checks if a network connection is established.
            

            // Acquire a lock to ensure thread-safe modification of the game state.
            lock (stateLock)
            {
                
                if (isConnected)
                {
                    // If the apple field has not been generated yet.
                    if (genratedAppleField == false)
                    {
                        // Create a new apple field with a specified number of apples and a single power-up.
                        field = new AppleField(this, 5, 1);
                        // Add both the server and client snakes to the apple field.
                        field.getSnakes().Add(snake_server);
                        field.getSnakes().Add(snake_client);
                        // Mark the apple field as generated.
                        genratedAppleField = true;
                    }
                    // Place the apples on the field (likely for drawing).
                    field.put();
                    // Sets the username for both server and client snakes.
                    snake_server.userName();
                    snake_client.userName();
                    // If this instance is the host.
                    if (isHost)
                    {
                        // Set the client snake's username based on the received game state.
                        snake_client.setUsername(gameState.usernameClient);
                        // Set the server snake's username to the local player's username.
                        snake_server.setUsername(userName);
                        // Generate the next movement for the server snake based on input.
                        snake_server.gen();
                        // Ensure the server snake stays within the bounds of the screen.
                        snake_server.setBoundsToScreen();
                        // Update the server snake's state based on the game loop and enable drawing.
                        snake_server.debug_standard_update(realtime, true);

                        // Apply the latest client position received from the network.
                        if (isConnected)
                        {
                            // Generate the next movement for the client snake (though the host doesn't control it directly).
                            snake_client.gen();
                            // Ensure the client snake stays within the bounds of the screen.
                            snake_client.setBoundsToScreen();
                            // Connect the head of the client snake to its body segments.
                            snake_client.connectHeadToBody(snake_client.getMovingBody().getBody());
                            // Directly update the client snake's position based on the received game state.
                            snake_client.getMovingBody().getBody().changePos(gameState.ClientState.X, gameState.ClientState.Y);
                        }
                    }
                    // If this instance is the client.
                    else
                    {
                        // Set the client snake's username to the local player's username.
                        snake_client.setUsername(userName);
                        // Set the server snake's username based on the received game state.
                        snake_server.setUsername(gameState.usernameServer);
                        // Generate the next movement for the client snake based on input.
                        snake_client.gen();
                        // Ensure the client snake stays within the bounds of the screen.
                        snake_client.setBoundsToScreen();
                        // Update the client snake's state based on the game loop and enable drawing.
                        snake_client.debug_standard_update(realtime, true);

                        // Apply the latest server position received from the network.
                        if (isConnected)
                        {
                            // Generate the next movement for the server snake (though the client doesn't control it directly).
                            snake_server.gen();
                            // Ensure the server snake stays within the bounds of the screen.
                            snake_server.setBoundsToScreen();
                            // Connect the head of the server snake to its body segments.
                            snake_server.connectHeadToBody(snake_server.getMovingBody().getBody());

                            // Directly update the server snake's position based on the received game state.
                            snake_server.getMovingBody().getBody().changePos(gameState.ServerState.X, gameState.ServerState.Y);
                        }
                    }

                    
                    
                }
            }

            // Forces a redraw of the form, triggering the Paint event.
            this.Invalidate();
            // Resumes the game loop and requests the next frame update.
            realtime.resumeAndUpdate();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
        }

        // Event handler for the "Host" button click.
        private async void button1_Click(object sender, EventArgs e)
        {
            
            try
            {
                // Set the flag indicating this instance is the host.
                isHost = true;
                this.Controls.Remove(button1);
                this.Controls.Remove(button2);
                this.Controls.Remove(label2);
                this.Controls.Remove(textBox1);
                // Update the connection status label.
                lblConnection.Text = "Starting server...";

                // Create a new server instance.
                server = new Server();
                // Start the server and wait for a client to connect asynchronously.
                await server.StartAsync();
                // Set the flag indicating a connection is established.
                isConnected = true;

                // Update the connection status label for the host.
                lblConnection.Text = "Connected as Host";

                // Create a new client instance (used for sending data back to the connected client).
                client = new Client();
                // Connect the client asynchronously (to the local server).
                await client.ConnectAsync();

                // Configure initial snake positions.
                lock (stateLock)
                {
                    // Set the initial X position of the server snake in the game state.
                    gameState.ServerState.X = snake_server.getMovingBody().getBody().getX();
                    // Set the initial Y position of the server snake in the game state.
                    gameState.ServerState.Y = snake_server.getMovingBody().getBody().getY();

                    // Initialize the client's known game state.
                    client.SetGameState(gameState);
                }

                // Start the network communication loop in a separate task.
                _ = Task.Run(NetworkCommunicationLoop);

                // Output a message to the console indicating a successful host connection.
                Console.WriteLine("Connection established as Host.");
                
            }
            catch (Exception ex)
            {
                // If an error occurs, reset the connection status.
                isConnected = false;
                // Update the connection status label with the error message.
                lblConnection.Text = "Host Error: " + ex.Message;
                // Output the error message to the console.
                Console.WriteLine($"Host Error: {ex.Message}");
            }
        }

        // Event handler for the "Join" button click.
        private async void button2_Click(object sender, EventArgs e)
        {
            try
            {
                // Set the flag indicating this instance is not the host (it's a client).
                isHost = false;
                // Update the connection status label.
                lblConnection.Text = "Connecting...";

                // Create a new client instance, using the IP address from the textbox.
                client = new Client(textBox1.Text);
                // Connect the client to the server asynchronously.
                await client.ConnectAsync();
                // Set the flag indicating a connection is established.
                isConnected = true;

                // Update the connection status label for the client.
                lblConnection.Text = "Connected as Client";

                // Configure initial snake positions.
                lock (stateLock)
                {
                    // Set the initial X position of the client snake in the game state.
                    gameState.ClientState.X = snake_client.getMovingBody().getBody().getX();
                    // Set the initial Y position of the client snake in the game state.
                    gameState.ClientState.Y = snake_client.getMovingBody().getBody().getY();

                    // Initialize the client's known game state.
                    client.SetGameState(gameState);
                    this.Controls.Remove(button1);
                    this.Controls.Remove(button2);
                    this.Controls.Remove(label2);
                    this.Controls.Remove(textBox1);
                }

                // Start the network communication loop in a separate task.
                _ = Task.Run(NetworkCommunicationLoop);

                // Output a message to the console indicating a successful client connection.
                Console.WriteLine("Connection established as Client.");
            }
            catch (Exception ex)
            {
                // If an error occurs, reset the connection status.
                isConnected = false;
                // Update the connection status label with the error message.
                lblConnection.Text = "Client Error: " + ex.Message;
                // Output the error message to the console.
                Console.WriteLine($"Client Error: {ex.Message}");
            }
        }

        // Combined network communication loop - handles both sending and receiving game state.
        private async Task NetworkCommunicationLoop()
        {
            try
            {
                // Continue the loop as long as a connection is active.
                while (isConnected)
                {
                    // Get the current position of the local snake based on whether this instance is the host or client.
                    PositionState localPos = isHost ?
                        new PositionState { X = snake_server.getMovingBody().getBody().getX(), Y = snake_server.getMovingBody().getBody().getY() } :
                        new PositionState { X = snake_client.getMovingBody().getBody().getX(), Y = snake_client.getMovingBody().getBody().getY() };

                    // If this instance is the host.
                    if (isHost)
                    {
                        // Send the current game state (including server's local position, client's last known position, and usernames) to the client.
                        await server.SendGameStateAsync(new GameState
                        {
                            ServerState = localPos,
                            ClientState = gameState.ClientState,
                            usernameClient = gameState.usernameClient,
                            usernameServer = userName
                        });

                        // Receive the game state update from the client.
                        var received = await server.ReceiveGameStateAsync();
                        // If data was received.
                        if (received != null)
                        {
                            // Acquire a lock to safely update the game state.
                            lock (stateLock)
                            {
                                // Update the client's position in the local game state.
                                gameState.ClientState = received.ClientState;
                                // Update the client's username in the local game state.
                                gameState.usernameClient = received.usernameClient;
                                // Directly update the client snake's position based on the received state.
                                snake_client.getMovingBody().changePosition(gameState.ClientState.X, gameState.ClientState.Y);
                            }
                        }
                    }
                    // If this instance is the client.
                    else
                    {
                        // Update the local client's game state with its current position and the last known server state.
                        client.UpdateLocalState(localPos, gameState.ServerState, userName, gameState.usernameServer);
                        // Send the client's game state to the server.
                        await client.SendGameStateAsync();

                        // Receive the game state update from the server.
                        var received = await client.ReceiveGameStateAsync();
                        // If data was received.
                        if (received != null)
                        {
                            // Acquire a lock to safely update the game state.
                            lock (stateLock)
                            {
                                // Update the server's position in the local game state.
                                gameState.ServerState = received.ServerState;
                                // Update the server's username in the local game state.
                                gameState.usernameServer = received.usernameServer;
                                // Directly update the server snake's position based on the received state.
                                snake_server.getMovingBody().changePosition(gameState.ServerState.X, gameState.ServerState.Y);
                            }
                        }
                    }
                    //Just incase I need this later :)
                    await Task.Delay(0);
                    // Check for head-on collision between the server snake and the client snake.
                    snake_server.debug_standard_head_collsion_rules(snake_client, realtime);
                    // Check for head-on collision between the client snake and the server snake.
                    snake_client.debug_standard_head_collsion_rules(snake_server, realtime);
                }
            }
            catch (Exception ex)
            {
                // Handle disconnections by updating the UI on the main thread.
                this.Invoke((MethodInvoker)delegate
                {
                    // Update the connection status label to indicate disconnection and the error message.
                    lblConnection.Text = $"Disconnected: {ex.Message}";
                    // Set the connection status flag to false.
                    isConnected = false;
                });
            }
        }

        // Event handler for when the text in textBox1 (likely for IP address input) changes.
        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            // You can add logic here to handle changes in the IP address input, if needed.
        }
    }
}
