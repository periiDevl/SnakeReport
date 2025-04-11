
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading; 
using System.Threading.Tasks; 
using System.Windows.Forms;
using static System.Net.Mime.MediaTypeNames;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Tab;
using Label = System.Windows.Forms.Label;

namespace Snake 
{
    internal class Snake 
    {
        // The body's connected to the snakes body
        private List<Body> bodys = new List<Body>(); // A list to hold the individual Body segments of the snake.
        // Refrence to the Form1 (The snakes window)
        private Form1 f; // A reference to the main form of the application.

        // RGB Values to use on the snakes body later in the code
        private int red = 255; // Initial red color component.
        private int green = 255; // Initial green color component.
        private int blue = 255; // Initial blue color component.

        // Random object to genrate a "Random" number
        private static Random random = new Random(); // A static Random object for generating random numbers.
        // Moving body object used to control the Target object of the snake bodys
        MovingBody mBody; // An instance of the MovingBody class to handle the movement of the snake's head.

        // The Label displayed on top of the snake
        private Label username_label; // A Label control to display a username above the snake.


        public Snake(Form1 form)
        { // Constructor for the Snake class, takes the main form as an argument.
            // A function to start creating the snakes body on the screen
            create(form); // Calls the create method to initialize the snake's body.

            // Creating the label to display the username on top of the snake
            username_label = new Label(); // Creates a new Label object.
            username_label.Text = ""; // Sets the initial text of the label to empty.
            username_label.AutoSize = true; // Automatically size the label to fit its content.

            // Set initial location
            username_label.Location = new Point(50, 50); // Sets the initial position of the label.
            username_label.BackColor = Color.Transparent; // Makes the background of the label transparent.
            // Add label to the form
            form.Controls.Add(username_label); // Adds the label control to the form's controls collection, making it visible.
        }

        // Function to add a Cell or a new body to the snakes body to make it longer by 1
        public void addCell() { Body b = new Body(f); bodys.Add(b); } // Creates a new Body segment and adds it to the snake's body list.
        // Function to add a couple Cell or a new body to the snakes body to make it longer by num
        public void addCells(int num) { for (int i = 0; i < num; i++) { addCell(); } } // Adds a specified number of Body segments to the snake.
        // Removing all the cells of the snake. Used to reset it. But it keeps 2 cells
        public void removeAllCells() { bodys.RemoveRange(2, bodys.Count - 2); } // Removes all Body segments except the first two.
        // Removing cells at a section of
        public void removeCellsAt(int fromIndex, int howMuch) { bodys.RemoveRange(fromIndex, howMuch); } // Removes a specified number of Body segments starting from a given index.
        // Creating the snake by generating it in the paint function, the paint function is integrated in windows forms used to display complex objects
        public void create(Form1 form, int startSize = 1)
        {
            // Setting the form refrence and adding the Paint function to the form paint function.
            f = form; // Assigns the provided form to the class's form reference.
            f.Paint += Paint; // Attaches the Paint method of this class to the form's Paint event, which is called when the form needs to be redrawn.
            if (startSize < 1) { startSize = 1; } // Ensures the starting size is at least 1.
            // start the snake with a start size.
            for (int i = 0; i < startSize; i++) // Loops to add the initial number of Body segments to the snake.
            {
                addCell(); // Adds a new Body segment.
            }
        }
        // Genrate the snake in a function used every frame
        public void gen()
        {
            // Connecting the snakes body's to each other
            for (int i = 0; i < bodys.Count; i++) // Iterates through each Body segment in the snake.
            {
                if (i != 0) // Skips the first segment (the head).
                {
                    bodys[i].connect(bodys[i - 1]); // Connects the current Body segment to the previous one, making the snake move as a unit.
                }
            }
        }
        // Setting the Target to the snakes head.
        public void connectHeadToBody(Body body)
        {
            bodys[0].connect(body); // Sets the target of the snake's head (the first Body segment) to the provided Body object, which is usually the MovingBody.
        }
        // Getting the top (first body/cell) or the real head of the snake.
        public Body top() { return bodys[0]; } // Returns the first Body segment in the list, which represents the head of the snake.
        // Getting any part of the snakes bodys by index
        public Body top(int n) { return bodys[n]; } // Returns the Body segment at the specified index.
        // Get the location of every body in the list of bodys.
        public List<Point> getBodysLocation()
        {
            return bodys.Select(body => body.getLocation()).ToList(); // Uses LINQ to select the location of each Body segment and returns them as a list of Point objects.
        }
        public List<Body> getBodys()
        {
            return bodys; // Returns the entire list of Body segments.
        }
        public void gencolor()
        {
            red = random.Next(120, 254); // Generates a random value for the red color component.
            blue = random.Next(120, 254); // Generates a random value for the blue color component.
            green = random.Next(120, 254); // Generates a random value for the green color component.
        }
        public int countCells() { return bodys.Count; } // Returns the current number of Body segments in the snake.

        public bool collision(Snake snake, int threshold = 5)
        {

            for (int i = 0; i < snake.bodys.Count(); i++) // Iterates through each Body segment of the other snake.
            {
                for (int j = 0; j < bodys.Count; j++) // Iterates through each Body segment of the current snake.
                {


                    if (Math.Sqrt(Math.Pow(snake.bodys[i].getX(), 2) + Math.Pow(bodys[j].getX(), 2)) < threshold &&
                        Math.Sqrt(Math.Pow(snake.bodys[i].getY(), 2) + Math.Pow(bodys[j].getY(), 2)) < threshold) { return true; } // Checks if the distance between any two segments of the snakes is less than the threshold, indicating a collision. (Note: The use of XOR (^) was likely a mistake and has been corrected to Math.Pow for squaring).

                }
            }
            return false; // Returns false if no collision is detected.
        }
        // Timer used to accumulate the time the snake's head stays near another snake's body
        private double collisionTimer = 0f;

        // Detects if the head of this snake is colliding with the body of another snake.
        // Uses a delay (grace period) to confirm collision only if it persists over time.
        public bool headCollision(Snake otherSnake, double deltaTime, int threshold = 5, float delaySeconds = 0.5f)
        {
            // Ignore collision checks if either snake is too short
            if (otherSnake.bodys.Count < 5 || bodys.Count < 5)
                return false;

            var head = top(0); // Get this snake's head segment

            bool isClose = false; // Will become true if we detect a body segment within range

            // Loop through each segment of the other snake's body
            for (int i = 0; i < otherSnake.bodys.Count; i++)
            {
                var otherSegment = otherSnake.bodys[i];

                // Calculate distance between head and current body segment
                double deltaX = head.getX() - otherSegment.getX();
                double deltaY = head.getY() - otherSegment.getY();
                double distance = Math.Sqrt(deltaX * deltaX + deltaY * deltaY);

                // If distance is below the collision threshold, mark as close
                if (distance < threshold)
                {
                    isClose = true;
                    break;
                }
            }

            if (isClose)
            {
                // Accumulate time spent near the other snake
                collisionTimer += deltaTime;

                // Only trigger actual collision after staying close long enough
                if (collisionTimer >= delaySeconds)
                    return true;
            }
            else
            {
                // Reset timer if the head moves away
                collisionTimer = 0f;
            }

            return false; // No valid collision detected (yet)
        }

        public void userName()
        {
            username_label.Location = new Point(mBody.getBody().getX(), mBody.getBody().getY() - 10); // Updates the location of the username label to be above the snake's head.
        }
        public void setUsername(string name) { username_label.Text = name; } // Sets the text of the username label.

        public void genrateMovement()
        {
            mBody = new MovingBody(f); // Creates a new instance of the MovingBody class, associated with the form.
            mBody.getBody().changePos(f.Width / 2, f.Height / 2); // Sets the initial position of the MovingBody (and thus the snake's head) to the center of the form.
        }
        public void moveWASD(Realtime realtime)
        {
            connectHeadToBody(mBody.getBody()); // Connects the snake's head to the MovingBody to follow its movement.
            mBody.move(realtime); // Calls the move method of the MovingBody to update its position based on WASD input and the elapsed time.
        }
        public void moveARROW(Realtime realtime)
        {
            connectHeadToBody(mBody.getBody()); // Connects the snake's head to the MovingBody.
            mBody.move_arrow(realtime); // Calls the move_arrow method of the MovingBody to update its position based on arrow key input and the elapsed time.
        }
        public void setBoundsToScreen()
        {
            mBody.setBounds(f.Width - 20, f.Height - 40); // Sets the movement boundaries for the MovingBody based on the form's size.
        }
        public MovingBody getMovingBody() { return mBody; } // Returns the MovingBody instance associated with the snake.

        // Debugging standard settings functions :
        public void debug_standard()
        {
            addCells(2); // Adds 2 Body segments to the snake.
            gencolor(); // Generates a random color for the snake.
            genrateMovement(); // Initializes the MovingBody for snake movement.
        }
        public void debug_standard_update(Realtime realtime, bool moveArrow)
        {
            if (!moveArrow) { moveWASD(realtime); } else { moveARROW(realtime); } // Moves the snake based on whether arrow key control is enabled or not.
        }
        public void debug_standard_head_collsion_rules(Snake snake2, Realtime realtime)
        {
            if (headCollision(snake2, realtime.deltaTime(), 15, 0.035f)) // Checks if the head of this snake collided with any part of the other snake.
            {
                removeAllCells(); // Removes all but the first two Body segments of this snake.
                getMovingBody().getBody().changePos(random.Next(0, f.Width), random.Next(0, f.Height)); // Resets the position of the MovingBody (and thus the snake's head) to a random location on the form.
            }
        }
        public void changePositiom(int x, int y)
        {
            mBody.changePosition(x, y); // Directly changes the position of the MovingBody.
        }
        // Private:
        private void Paint(object sender, PaintEventArgs e) // This method is called whenever the form needs to be redrawn.
        {
            List<Point> bodyCenters = getBodysLocation(); // Gets the current locations of all Body segments.
            if (bodyCenters.Count < 2)
                return; // If the snake has less than 2 segments, there's nothing to draw.

            int snakeThickness = 14; // Defines the thickness of the snake.
            int factor = 4; // A factor to control the color gradient.

            using (SolidBrush brush = new SolidBrush(Color.Green)) // Creates a SolidBrush for filling the snake's body. The 'using' statement ensures the brush is properly disposed of.
            using (Pen outlinePen = new Pen(Color.Black, 2.0f)) // Creates a Pen for drawing the outline of the snake.
            {
                for (int i = 1; i < bodyCenters.Count; i++) // Iterates through the Body segments to draw them, starting from the second segment.
                {
                    if (red - (i * factor) >= 0 && green - (i * factor) >= 0 && blue - (i * factor) >= 0)
                    {
                        brush.Color = Color.FromArgb(red - (i * factor), green - (i * factor), blue - (i * factor)); // Gradually changes the color of each segment.
                    }

                    Point p1 = bodyCenters[i - 1]; // Gets the location of the previous Body segment.
                    Point p2 = bodyCenters[i]; // Gets the location of the current Body segment.

                    int dx = p2.X - p1.X; // Calculates the horizontal difference between the two points.
                    int dy = p2.Y - p1.Y; // Calculates the vertical difference between the two points.

                    double length = Math.Sqrt(dx * dx + dy * dy); // Calculates the distance between the two points.

                    float normDx = (float)(dx / length); // Normalizes the horizontal difference.
                    float normDy = (float)(dy / length); // Normalizes the vertical difference.

                    float extendAmount = snakeThickness / 4.0f; // Amount to extend the points for a smoother connection.
                    Point extendedP1 = new Point((int)(p1.X - normDx * extendAmount), (int)(p1.Y - normDy * extendAmount)); // Extends the previous point.
                    Point extendedP2 = new Point((int)(p2.X + normDx * extendAmount), (int)(p2.Y + normDy * extendAmount)); // Extends the current point.

                    float offsetX = normDy * snakeThickness / 2; // Calculates the horizontal offset for the thickness.
                    float offsetY = -normDx * snakeThickness / 2; // Calculates the vertical offset for the thickness.

                    Point[] points = new Point[] // Creates an array of four points to define the rectangular shape of the snake segment.
                    {
                        new Point((int)(extendedP1.X + offsetX), (int)(extendedP1.Y + offsetY)),
                        new Point((int)(extendedP1.X - offsetX), (int)(extendedP1.Y - offsetY)),
                        new Point((int)(extendedP2.X - offsetX), (int)(extendedP2.Y - offsetY)),
                        new Point((int)(extendedP2.X + offsetX), (int)(extendedP2.Y + offsetY))
                    };

                    e.Graphics.FillPolygon(brush, points); // Fills the polygon defined by the points with the current brush color.

                    if (i == 1)
                    {
                        e.Graphics.DrawLine(outlinePen, points[0], points[1]); // Draws the outline for the start of the segment.
                    }

                    if (i == bodyCenters.Count - 1)
                    {
                        e.Graphics.DrawLine(outlinePen, points[2], points[3]); // Draws the outline for the end of the segment.
                    }

                    e.Graphics.DrawLine(outlinePen, points[0], points[3]); // Draws one of the side outlines.
                    e.Graphics.DrawLine(outlinePen, points[1], points[2]); // Draws the other side outline.
                }
            }
        }
    }
}
