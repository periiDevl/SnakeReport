using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Snake
{
    // Manages real-time updates and input handling for the game.
    internal class Realtime
    {
        private Timer timer;
        private Stopwatch stopwatch;
        private HashSet<Keys> pressedKeys;
        private double totalTime = 0;
        Form1 f;

        // Initializes and starts the real-time update loop.
        public void start(EventHandler updateMethod, Form1 form)
        {
            f = form;

            // Initialize a new timer.
            timer = new Timer();
            // Set the timer interval to 16 milliseconds, aiming for approximately 60 frames per second.
            timer.Interval = 16;
            // Attach the provided update method to the timer's Tick event. This method will be executed on each timer interval.
            timer.Tick += updateMethod;
            // Start the timer, initiating the real-time update loop.
            timer.Start();
            // Initialize and start a new stopwatch to measure elapsed time.
            stopwatch = Stopwatch.StartNew();

            // Enable key preview for the form to capture key events before they are processed by controls.
            f.KeyPreview = true;
            // Attach event handlers for KeyDown and KeyUp events of the form to track pressed keys.
            f.KeyDown += Form1_KeyDown;
            f.KeyUp += Form1_KeyUp;
            // Initialize a HashSet to store the currently pressed keys, ensuring no duplicates.
            pressedKeys = new HashSet<Keys>();

        }

        // Checks if a specific key is currently being pressed.
        public bool key(Keys key) { return getPressed().Contains(key); }

        // Gets the set of keys that are currently pressed.
        public HashSet<Keys> getPressed() { return pressedKeys; }

        // Calculates the time elapsed since the last frame (deltaTime). Used to make the program run the same on diffrent frames per second. 
        public double deltaTime() { return stopwatch.Elapsed.TotalSeconds; }

        // Accumulates the deltaTime to calculate the total time elapsed since the start.
        public void calculateTotalTime() { totalTime += deltaTime(); }

        // Gets the total time elapsed since the start of the real-time updates.
        public double getTotalTime() { return totalTime; }

        // Restarts the stopwatch to measure the time for the next frame. This should be called at the end of each update cycle.
        public void updateForNextFrame() { stopwatch.Restart(); }

        /// <summary>
        /// Suspends the layout logic for the form, potentially improving performance during multiple updates.
        /// </summary>
        public void suspend() { f.SuspendLayout(); }

        /// <summary>
        /// Resumes the layout logic for the form and forces a redraw of the client area.
        /// </summary>
        public void resumeAndUpdate() { f.Invalidate(); f.ResumeLayout(); }

        /// <summary>
        /// Handles the KeyDown event of the form, adding the pressed key to the set of pressed keys.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">A KeyEventArgs that contains the event data.</param>
        private void Form1_KeyDown(object sender, KeyEventArgs e) { pressedKeys.Add(e.KeyCode); }

        /// <summary>
        /// Handles the KeyUp event of the form, removing the released key from the set of pressed keys.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">A KeyEventArgs that contains the event data.</param>
        private void Form1_KeyUp(object sender, KeyEventArgs e) { pressedKeys.Remove(e.KeyCode); }
    }

}
