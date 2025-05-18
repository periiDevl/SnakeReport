using System;
using System.Drawing;
using System.Reflection.Emit;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace Snake
{
    public partial class Form1 : Form
    {
        // Represents player 1's snake
        private Snake snake_player1;
        // Represents player 2's snake
        private Snake snake_player2;
        // Manages the game's real-time loop
        private Realtime realtime;
        // Represents the playing field containing apples
        private AppleField field;

        // Handles the network client
        private Client client;
        // Stores the overall game state, including positions of both snakes
        private GameState gameState;
        // Flag indicating if a network connection is established
        private bool isConnected = false;
        // Lock object for ensuring thread-safe access to shared game state
        private object stateLock = new object();
        // Flag to track if the apple field has been generated
        private bool genratedAppleField = false;
        // Label to display the current connection status to the user
        private System.Windows.Forms.Label lblConnection;
        // Stores the username of the local player
        public string userName = "Player";
        // Player number assigned by the server (1 or 2)
        private int playerNumber = 0;
        // Flag indicating if the game has started
        private bool gameStarted = false;

        // Constructor for the Form1 class
        public Form1()
        {
            // Initializes the visual components of the form
            InitializeComponent();
            // Sets the initial width of the game window
            this.Width = 800;
            // Sets the initial height of the game window
            this.Height = 600;
            // Enables double buffering to reduce flickering during drawing
            this.DoubleBuffered = true;

            // Create and position the connection status label (top-left)
            lblConnection = new System.Windows.Forms.Label();
            lblConnection.Font = new Font("Arial", 12, FontStyle.Bold);
            lblConnection.ForeColor = Color.Green;
            lblConnection.BackColor = Color.Transparent;
            lblConnection.AutoSize = true;
            lblConnection.Location = new Point(10, 10);
            lblConnection.Text = "Not Connected";
            this.Controls.Add(lblConnection);

            // Create snake objects for both players
            snake_player1 = new Snake(this);
            // Initializes the player 1 snake with default debug settings
            snake_player1.debug_standard();
            snake_player2 = new Snake(this);
            // Initializes the player 2 snake with default debug settings
            snake_player2.debug_standard();

            // Start the realtime (game loop) update
            realtime = new Realtime();
            // Initiates the game loop, calling the 'Update' method at regular intervals
            realtime.start(Update, this);

            // Initialize game state with default positions for both snakes
            gameState = new GameState
            {
                ServerState = new PositionState() { X = 0, Y = 0 },
                ClientState = new PositionState() { X = 0, Y = 0 }
            };
        }

        // This method is called repeatedly by the game loop to update the game state and redraw the screen
        private void Update(object sender, EventArgs e)
        {
            // Sets the title of the form to indicate the developer and game
            this.Text = "Jonathan Peri : Snake";
            // Temporarily pauses the game loop
            realtime.suspend();
            // Increments the frame counter for the next update cycle
            realtime.updateForNextFrame();

            // Acquire a lock to ensure thread-safe modification of the game state
            lock (stateLock)
            {
                // Only update the game if connected and the game has started
                if (isConnected && gameStarted)
                {
                    

                    // If the apple field has not been generated yet
                    if (genratedAppleField == false)
                    {
                        // Create a new apple field with a specified number of apples and a single power-up
                        field = new AppleField(this, 5, 1);
                        // Add both player snakes to the apple field
                        field.getSnakes().Add(snake_player1);
                        field.getSnakes().Add(snake_player2);
                        // Mark the apple field as generated
                        genratedAppleField = true;
                    }
                    // Place the apples on the field (likely for drawing)
                    field.put();
                    // Sets the username for both player snakes
                    snake_player1.userName();
                    snake_player2.userName();
                    // If this instance is player 1
                    if (playerNumber == 1)
                    {
                        // Set player 2's username based on the received game state
                        snake_player2.setUsername(gameState.usernameClient);
                        // Set player 1's username to the local player's username
                        snake_player1.setUsername(userName);
                        // Generate the next movement for player 1's snake based on input
                        snake_player1.gen();
                        // Ensure player 1's snake stays within the bounds of the screen
                        snake_player1.setBoundsToScreen();
                        // Update player 1's snake's state based on the game loop and enable drawing
                        snake_player1.debug_standard_update(realtime, true);

                        // Apply the latest player 2 position received from the server
                        // Generate the next movement for player 2's snake (though this client doesn't control it directly)
                        snake_player2.gen();
                        // Ensure player 2's snake stays within the bounds of the screen
                        snake_player2.setBoundsToScreen();
                        // Connect the head of player 2's snake to its body segments
                        snake_player2.connectHeadToBody(snake_player2.getMovingBody().getBody());
                        // Directly update player 2's snake's position based on the received game state
                        snake_player2.getMovingBody().getBody().changePos(gameState.ClientState.X, gameState.ClientState.Y);
                    }
                    // If this instance is player 2
                    else if (playerNumber == 2)
                    {
                        // Set player 2's username to the local player's username
                        snake_player2.setUsername(userName);
                        // Set player 1's username based on the received game state
                        snake_player1.setUsername(gameState.usernameServer);
                        // Generate the next movement for player 2's snake based on input
                        snake_player2.gen();
                        // Ensure player 2's snake stays within the bounds of the screen
                        snake_player2.setBoundsToScreen();
                        // Update player 2's snake's state based on the game loop and enable drawing
                        snake_player2.debug_standard_update(realtime, true);

                        // Apply the latest player 1 position received from the server
                        // Generate the next movement for player 1's snake (though this client doesn't control it directly)
                        snake_player1.gen();
                        // Ensure player 1's snake stays within the bounds of the screen
                        snake_player1.setBoundsToScreen();
                        // Connect the head of player 1's snake to its body segments
                        snake_player1.connectHeadToBody(snake_player1.getMovingBody().getBody());
                        // Directly update player 1's snake's position based on the received game state
                        snake_player1.getMovingBody().getBody().changePos(gameState.ServerState.X, gameState.ServerState.Y);
                    }

                }
            }

            // Forces a redraw of the form, triggering the Paint event
            this.Invalidate();
            // Resumes the game loop and requests the next frame update
            realtime.resumeAndUpdate();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
        }

        // Event handler for the "Connect" button click
        private async void button1_Click(object sender, EventArgs e)
        {
            try
            {
                // Update the connection status label
                lblConnection.Text = "Connecting to server...";

                // Disable buttons during connection attempt
                button1.Enabled = false;
                button2.Enabled = false;

                // Create a new client instance, using the IP address from the textbox
                client = new Client(textBox1.Text);
                // Connect the client to the server asynchronously
                await client.ConnectAsync();
                // Set the flag indicating a connection is established
                isConnected = true;

                // Wait for the START message from the server to determine player number
                var startData = await client.ReceiveStartSignalAsync();
                if (startData != null && startData.StartsWith("START"))
                {
                    // Extract player number
                    string[] parts = startData.Split(',');
                    if (parts.Length > 1 && int.TryParse(parts[1], out int pNum))
                    {
                        playerNumber = pNum;
                        gameStarted = true;

                        // Remove UI elements
                        this.Controls.Remove(button1);
                        this.Controls.Remove(button2);
                        this.Controls.Remove(label2);
                        this.Controls.Remove(textBox1);

                        // Update the connection status label
                        lblConnection.Text = $"Connected as Player {playerNumber}";

                        // Configure initial snake positions
                        lock (stateLock)
                        {
                            if (playerNumber == 1)
                            {
                                // Set the initial X position of player 1's snake in the game state
                                gameState.ServerState.X = snake_player1.getMovingBody().getBody().getX();
                                // Set the initial Y position of player 1's snake in the game state
                                gameState.ServerState.Y = snake_player1.getMovingBody().getBody().getY();
                                gameState.usernameServer = userName;
                            }
                            else
                            {
                                // Set the initial X position of player 2's snake in the game state
                                gameState.ClientState.X = snake_player2.getMovingBody().getBody().getX();
                                // Set the initial Y position of player 2's snake in the game state
                                gameState.ClientState.Y = snake_player2.getMovingBody().getBody().getY();
                                gameState.usernameClient = userName;
                            }
                            // Check for head-on collision between the snakes
                           
                        }
                        // Start the network communication loop in a separate task
                        _ = Task.Run(NetworkCommunicationLoop);
                    }
                }

                // Output a message to the console indicating a successful connection
                Console.WriteLine($"Connection established as Player {playerNumber}");
            }
            catch (Exception ex)
            {
                // If an error occurs, reset the connection status
                isConnected = false;
                gameStarted = false;
                // Update the connection status label with the error message
                lblConnection.Text = "Connection Error: " + ex.Message;
                // Output the error message to the console
                Console.WriteLine($"Connection Error: {ex.Message}");
                // Re-enable connect button
                button1.Enabled = true;
                button2.Enabled = true;
            }
        }

        // Event handler for the "Set Username" button
        private void button2_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(textBox1.Text))
            {
                userName = textBox1.Text;
                lblConnection.Text = $"Username set: {userName}";
                // Hide the username input after setting
                textBox1.Visible = false;
                button2.Visible = false;
                label2.Visible = false;
            }
        }

        // Network communication loop - handles both sending and receiving game state
        private async Task NetworkCommunicationLoop()
        {
            try
            {
                // Continue the loop as long as a connection is active
                while (isConnected && gameStarted)
                {
                    // Acquire a lock to safely update the game state
                    lock (stateLock)
                    {
                        // If this instance is player 1
                        if (playerNumber == 1)
                        {
                            // Update the local game state with current positions
                            gameState.ServerState.X = snake_player1.getMovingBody().getBody().getX();
                            gameState.ServerState.Y = snake_player1.getMovingBody().getBody().getY();
                            gameState.usernameServer = userName;
                        }
                        // If this instance is player 2
                        else if (playerNumber == 2)
                        {
                            // Update the local game state with current positions
                            gameState.ClientState.X = snake_player2.getMovingBody().getBody().getX();
                            gameState.ClientState.Y = snake_player2.getMovingBody().getBody().getY();
                            gameState.usernameClient = userName;
                        }
                    }

                    // Send the current game state to the server
                    await client.SendGameStateAsync(gameState);

                    // Receive the game state update from the server
                    var received = await client.ReceiveGameStateAsync();

                    // If data was received
                    if (received != null)
                    {
                        // Acquire a lock to safely update the game state
                        lock (stateLock)
                        {
                            // Update the local game state with the data received from the server
                            gameState = received;

                            // If this instance is player 1, update player 2's position
                            if (playerNumber == 1)
                            {
                                snake_player2.getMovingBody().changePosition(gameState.ClientState.X, gameState.ClientState.Y);
                            }
                            // If this instance is player 2, update player 1's position
                            else if (playerNumber == 2)
                            {
                                snake_player1.getMovingBody().changePosition(gameState.ServerState.X, gameState.ServerState.Y);
                            }
                        }
                    }
                    snake_player1.debug_standard_head_collsion_rules(snake_player2, realtime);
                    snake_player2.debug_standard_head_collsion_rules(snake_player1, realtime);

                    // Small delay to prevent CPU overuse
                    await Task.Delay(0);
                }
            }
            catch (Exception ex)
            {
                // Handle disconnections by updating the UI on the main thread
                this.Invoke((MethodInvoker)delegate
                {
                    // Update the connection status label to indicate disconnection and the error message
                    lblConnection.Text = $"Disconnected: {ex.Message}";
                    // Set the connection status flag to false
                    isConnected = false;
                    gameStarted = false;

                    // Add back the connect button
                    if (!Controls.Contains(button1))
                    {
                        System.Windows.Forms.Button reconnectButton = new System.Windows.Forms.Button();
                        reconnectButton.Text = "Reconnect";
                        reconnectButton.Location = new Point(10, 40);
                        reconnectButton.Click += button1_Click;
                        this.Controls.Add(reconnectButton);

                        System.Windows.Forms.TextBox ipTextBox = new System.Windows.Forms.TextBox();
                        ipTextBox.Text = "127.0.0.1";
                        ipTextBox.Location = new Point(110, 40);
                        this.Controls.Add(ipTextBox);
                        textBox1 = ipTextBox;

                        button1 = reconnectButton;
                    }
                });
            }
        }

        // Event handler for when the text in textBox1 (likely for IP address input) changes
        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            // You can add logic here to handle changes in the IP address input, if needed
        }
    }
}