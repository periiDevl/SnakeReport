
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Tab;

namespace Snake
{
    
    // Represents a moving body in the game.
    internal class MovingBody
    {
        // The speed at which the body moves.
        private float moveSpeed = 5f;
        // The underlying Body object that this class controls.
        private Body body;

        public MovingBody(Form1 f)
        {
            // Create a new Body object associated with the provided Form.
            body = new Body(f);
        }

       
        // Sets the boundaries for the moving body, preventing it from going off-screen.
        public void setBounds(int w, int h)
        {
            // If the body's X-coordinate is greater than the width, reset it to the width.
            if (body.getX() > w) { body.changePos(w, body.getY()); }
            // If the body's Y-coordinate is greater than the height, reset it to the height.
            if (body.getY() > h) { body.changePos(body.getX(), h); }

            // If the body's X-coordinate is less than 0, reset it to 0.
            if (body.getX() < 0) { body.changePos(0, body.getY()); }

            // If the body's Y-coordinate is less than 0, reset it to 0.
            if (body.getY() < 0) { body.changePos(body.getX(), 0); }
        }

       
        // Directly changes the position of the body.
        public void changePosition(int x, int y)
        {
            // Call the Body object's method to change its position.
            body.changePos(x, y);
        }

        // Moves the body based on WASD key input.
        public void move(Realtime realtime)
        {
            // If the 'D' key is pressed, move the body to the right.
            if (realtime.key(Keys.D))
            {
                body.moveX((int)moveSpeed);
            }
            // If the 'A' key is pressed, move the body to the left.
            if (realtime.key(Keys.A))
            {
                body.moveX((int)-moveSpeed);
            }
            // If the 'W' key is pressed, move the body upwards.
            if (realtime.key(Keys.W))
            {
                body.moveY((int)-moveSpeed);
            }
            // If the 'S' key is pressed, move the body downwards.
            if (realtime.key(Keys.S))
            {
                body.moveY((int)moveSpeed);
            }
        }

        // Moves the body based on TFGH key input (likely for a second player or control scheme).
        public void move_2(Realtime realtime)
        {
            // If the 'H' key is pressed, move the body to the right.
            if (realtime.key(Keys.H))
            {
                body.moveX((int)moveSpeed);
            }
            // If the 'F' key is pressed, move the body to the left.
            if (realtime.key(Keys.F))
            {
                body.moveX((int)-moveSpeed);
            }
            // If the 'T' key is pressed, move the body upwards.
            if (realtime.key(Keys.T))
            {
                body.moveY((int)-moveSpeed);
            }
            // If the 'G' key is pressed, move the body downwards.
            if (realtime.key(Keys.G))
            {
                body.moveY((int)moveSpeed);
            }
        }

        // Moves the body based on arrow key input.
        public void move_arrow(Realtime realtime)
        {
            // If the Right arrow key is pressed, move the body to the right.
            if (realtime.key(Keys.Right))
            {
                body.moveX((int)moveSpeed);
            }
            // If the Left arrow key is pressed, move the body to the left.
            if (realtime.key(Keys.Left))
            {
                body.moveX((int)-moveSpeed);
            }
            // If the Up arrow key is pressed, move the body upwards.
            if (realtime.key(Keys.Up))
            {
                body.moveY((int)-moveSpeed);
            }
            // If the Down arrow key is pressed, move the body downwards.
            if (realtime.key(Keys.Down))
            {
                body.moveY((int)moveSpeed);
            }
        }

        // Gets the underlying Body object.
        public Body getBody() { return body; }

        // Sets the movement speed of the body.
        public void setMoveSpeed(float speed) { moveSpeed = speed; }
    }
}
