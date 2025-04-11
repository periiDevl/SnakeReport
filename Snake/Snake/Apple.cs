using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
namespace Snake
{

    internal class Apple
    {
        //Storing the image of an apple
        PictureBox image = new PictureBox();
        //Form1 ref.
        Form1 form;
        //The apple size.
        int appleSize = 7;
        //Setting the screen Bounds
        int screenBounds = 100;
        //Random.
        private Random random;

        public Apple(Form1 f, Random rand)
        {
            this.random = rand;
            form = f;
            image.Width = 15;
            image.Height = 15;

            // Set the PictureBox to stretch the image to fit
            image.SizeMode = PictureBoxSizeMode.StretchImage;

            // Load an image
            string currentDir = Directory.GetCurrentDirectory();
            string imagePath = Path.Combine(currentDir, "apple.png");
            image.Image = Image.FromFile(imagePath);

            f.Controls.Add(image);
            genrateRandomly();

        }
        //Detecting Snake if they eat an apple, adding a cell and genrating the apple in a new location
        public void put(Snake snake)
        {
            if (detect(snake))
            {
                snake.addCell();
                genrateRandomly();
            }
            
        }
        bool detect(Snake snake)
        {
            if (Math.Sqrt(snake.getMovingBody().getBody().getX()^2 + (image.Location.X + image.Size.Width/2)^2) < appleSize && Math.Sqrt(snake.getMovingBody().getBody().getY() ^ 2 + (image.Location.Y + image.Size.Height/2) ^ 2) < appleSize) { return true; }
            return false;
        }
        //Genrate the apple in a Random location on the screen.
        private void genrateRandomly()
        {
            image.Location = new Point(random.Next(0 + screenBounds, form.Width - screenBounds), random.Next(0 + 50, form.Height - screenBounds)) ;
        }
    }
}
