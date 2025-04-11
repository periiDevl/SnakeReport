using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Tab;

namespace Snake
{
    
    internal class Body
    {
        
        public Body(Form1 form)
        {

        }
        public void changePos(int x, int y) { bodyPoint = new Point(x, y); } //Change the position of the body
        public int getX() { return bodyPoint.X; }//Get the X of the body
        public int getY() { return bodyPoint.Y; }//Get the Y of the body
        public void movePos(int x, int y) { changePos(getX() + ((int)(x * deltaTime)), getY() + ((int)(y * deltaTime))); }//Move the body by___
        public void moveX(int x) { changePos(getX() + ((int)(x * deltaTime)), getY()); }//Move body by ___ on the X axis
        public void moveY(int y) { changePos(getX(), getY() + ((int)(y * deltaTime))); }//Move body by ___ on the Y axis



        public void connect(Body parent)
        {
            int a = parent.getX() - getX(); // distance on the X axis
            int b = parent.getY() - getY();// distance on the Y axis
            int distance = (int)Math.Sqrt((a*a) + (b*b)); // Distance between this and parent

            int R1 = getRadius();
            int R2 = parent.getRadius();
            int combindR = R1 + R2;

            if (distance > combindR)
            { //outisde of combind radius
                int cPorshion = distance - combindR; // portion between total distance and distance we need to move
                double scale = (double)cPorshion / distance; // the diff of total distance and portion
                int moveX = (int)(a * scale); // scale dx by the difference
                int moveY = (int)(b * scale); // scale dy by the difference
                movePos(moveX, moveY);
            }
        }
        

        public Point getLocation() { return bodyPoint; } //Get location of body

        public void setRadius(int r) { radius = r; }//Set the Radius of the body
        public int getRadius() { return radius; }//Get the Radius of the body



        private Point bodyPoint = new Point();
        int radius = 5;
        private double deltaTime = 1;

 
    }
}
