using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Snake
{
    public partial class Login : Form
    {
        // Constructor for the Login form
        public Login()
        {
            InitializeComponent(); // Initializes the components of the form
        }

        // Event handler for the Register button click
        private void button2_Click(object sender, EventArgs e)
        {
            // Construct the path to the database file in the application's AppData directory
            string dbPath = System.IO.Path.Combine(Application.StartupPath, "AppData", "Database.mdf");
            // Construct the connection string for the SQL Server LocalDB
            string connectionString = $@"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename={dbPath};Integrated Security=True";

            // Create a new SqlConnection object using the connection string
            SqlConnection connection = new SqlConnection(connectionString);
            // Create a new SqlCommand object
            SqlCommand cmd = new SqlCommand();
            // Set the connection for the SqlCommand
            cmd.Connection = connection;
            // Set the SQL INSERT command to add a new user to the UsersData table
            cmd.CommandText = "INSERT INTO UsersData (Username, Password) VALUES (@username, @password)";
            // Add parameters to the SqlCommand to prevent SQL injection
            cmd.Parameters.AddWithValue("@username", textBox4.Text); // Get the username from textBox4
            cmd.Parameters.AddWithValue("@password", textBox3.Text); // Get the password from textBox3

            try
            {
                // Open the database connection
                connection.Open();
                // Execute the SQL command and get the number of rows affected
                int rowsAffected = cmd.ExecuteNonQuery();
                // Close the database connection
                connection.Close();

                // Check if any rows were affected (meaning the insertion was successful)
                if (rowsAffected > 0)
                {
                    MessageBox.Show("New user created!!!"); // Show a success message
                }
                else
                {
                    MessageBox.Show("Something went wrong... :("); // Show an error message if insertion failed
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}"); // Display any error that occurred during the database operation
            }
            finally
            {
                // Ensure the connection is closed even if an error occurs
                if (connection.State == ConnectionState.Open)
                {
                    connection.Close();
                }
            }
        }

        private void textBox4_TextChanged(object sender, EventArgs e)
        {
        }

        private void textBox3_TextChanged(object sender, EventArgs e)
        {
        }

        // Event handler for the Log In button click
        private void button1_Click(object sender, EventArgs e)
        {
            // Get the username from textBox1
            string username = textBox1.Text;
            // Get the password from textBox2
            string password = textBox2.Text;
            // Construct the path to the database file
            string dbPath = System.IO.Path.Combine(Application.StartupPath, "AppData", "Database.mdf");
            // Construct the connection string
            string connectionString = $@"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename={dbPath};Integrated Security=True";

            // Use a using statement to ensure the SqlConnection is properly disposed of after use
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                // Define the SQL query to count the number of users with the provided username and password
                string query = "SELECT COUNT(*) FROM UsersData WHERE Username = @username AND Password = @password";
                // Create a new SqlCommand object with the query and connection
                SqlCommand cmd = new SqlCommand(query, connection);
                // Add parameters to the SqlCommand to prevent SQL injection
                cmd.Parameters.AddWithValue("@username", username);
                cmd.Parameters.AddWithValue("@password", password);

                try
                {
                    // Open the database connection
                    connection.Open();
                    // Execute the SQL query and get the scalar result (the count)
                    int count = (int)cmd.ExecuteScalar();

                    // Check if the count is greater than 0, indicating a matching user was found
                    if (count > 0)
                    {
                        // Create a new instance of the game form (Form1)
                        Form1 gameForm = new Form1();
                        // Pass the logged-in username to the game form
                        gameForm.userName = username;
                        // Show the game form
                        gameForm.Show();
                        // Hide the login form (optional)
                        //this.Hide();
                        
                    }
                    else
                    {
                        // Show an error message if the username or password is invalid
                        MessageBox.Show("Invalid username or password.");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error: {ex.Message}"); // Display any error that occurred during the database operation
                }
            }
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
        }
        private void textBox2_TextChanged(object sender, EventArgs e)
        {
        }

        private void Login_Load(object sender, EventArgs e)
        {
        }
    }
}