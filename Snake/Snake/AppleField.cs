using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Snake
{
    internal class AppleField
    {
        private Random random;
        //List of apples object to use
        private List<Apple> apples = new List<Apple>();
        //List of snakes to keep track who eats what apples
        private List<Snake> snakes = new List<Snake>();

        public AppleField(Form1 form, int amountApples, int seed)
        {
            //creating a random instance controlled by a seed. to make it deterministic
            random = new Random(seed);
            //Add the amount of apples the users wants to the list to be displyed on the screen
            for (int i = 0; i < amountApples; i++)
            {
                Apple a = new Apple(form, random); // Pass the same seeded Random instance
                apples.Add(a);
            }
        }
        //put the apples on the screen and use the apples buit in logic to detect the snakes.
        public void put()
        {
            for (int i = 0; i < apples.Count; i++)
            {
                foreach (Snake snake in snakes)
                {
                    apples[i].put(snake);
                }
            }
        }

        public List<Snake> getSnakes() { return snakes; } // Getting the Snakes
    }

}
