using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using SpaceWars;

namespace SpaceWars
{
    class DrawingPanel : Panel
    {
        private World theWorld;
        private Image[] shipImagesThrust;
        private Image[] shipImagesCoast;
        private Image[] shipProjectile;
        Image dead_ship;

        public DrawingPanel(World w)
        {
            this.DoubleBuffered = true;
            theWorld = w;
            loadImages();
        }
        /// <summary>
        /// Loads images into their desired arrays for furture use
        /// </summary>
        private void loadImages()
        {
            string location = "..\\..\\..\\Resources\\Images\\";
            dead_ship = Image.FromFile(location + "skull.png");
            shipImagesThrust = new Image[8] {
                Image.FromFile(location + "ship-thrust-blue.png"),
            Image.FromFile(location + "ship-thrust-brown.png"),
            Image.FromFile(location + "ship-thrust-green.png"),
            Image.FromFile(location + "ship-thrust-grey.png"),
            Image.FromFile(location + "ship-thrust-red.png"),
            Image.FromFile(location +  "ship-thrust-violet.png"),
            Image.FromFile(location + "ship-thrust-white.png"),
            Image.FromFile(location +"ship-thrust-yellow.png") };

            shipImagesCoast = new Image[8] {
                Image.FromFile(location + "ship-coast-blue.png"),
            Image.FromFile(location + "ship-coast-brown.png"),
            Image.FromFile(location + "ship-coast-green.png"),
            Image.FromFile(location + "ship-coast-grey.png"),
            Image.FromFile(location + "ship-coast-red.png"),
            Image.FromFile(location +   "ship-coast-violet.png"),
            Image.FromFile(location + "ship-coast-white.png"),
            Image.FromFile(location +"ship-coast-yellow.png")};

            shipProjectile = new Image[8] {
                Image.FromFile(location + "shot-blue.png"),
            Image.FromFile(location + "shot-brown.png"),
            Image.FromFile(location +  "shot-green.png"),
            Image.FromFile(location + "shot-grey.png"),
            Image.FromFile(location + "shot-red.png"),
            Image.FromFile(location +   "shot-violet.png"),
            Image.FromFile(location + "shot-white.png"),
            Image.FromFile(location +"shot-yellow.png")};
        }

        /// <summary>
        /// Helper method for DrawObjectWithTransform
        /// </summary>
        /// <param name="size">The world (and image) size</param>
        /// <param name="w">The worldspace coordinate</param>
        /// <returns></returns>
        private static int WorldSpaceToImageSpace(int size, double w)
        {
            return (int)w + size / 2;
        }


        // A delegate for DrawObjectWithTransform
        // Methods matching this delegate can draw whatever they want using e  
        public delegate void ObjectDrawer(object o, PaintEventArgs e);

        /// <summary>
        /// This method performs a translation and rotation to drawn an object in the world.
        /// </summary>
        /// <param name="e">PaintEventArgs to access the graphics (for drawing)</param>
        /// <param name="o">The object to draw</param>
        /// <param name="worldSize">The size of one edge of the world (assuming the world is square)</param>
        /// <param name="worldX">The X coordinate of the object in world space</param>
        /// <param name="worldY">The Y coordinate of the object in world space</param>
        /// <param name="angle">The orientation of the objec, measured in degrees clockwise from "up"</param>
        /// <param name="drawer">The drawer delegate. After the transformation is applied, the delegate is invoked to draw whatever it wants</param>
        private void DrawObjectWithTransform(PaintEventArgs e, object o, int worldSize, double worldX, double worldY, double angle, ObjectDrawer drawer)
        {
            // Perform the transformation
            int x = WorldSpaceToImageSpace(worldSize, worldX);
            int y = WorldSpaceToImageSpace(worldSize, worldY);
            e.Graphics.TranslateTransform(x, y);
            e.Graphics.RotateTransform((float)angle);
            // Draw the object 
            drawer(o, e);
            // Then undo the transformation
            e.Graphics.ResetTransform();
        }

        /// <summary>
        /// Acts as a drawing delegate for DrawObjectWithTransform
        /// After performing the necessary transformation (translate/rotate)
        /// DrawObjectWithTransform will invoke this method
        /// </summary>
        /// <param name="o">The object to draw</param>
        /// <param name="e">The PaintEventArgs to access the graphics</param>
        private void ShipDrawer(object o, PaintEventArgs e)
        {
            Ship p = o as Ship;

            int shipWidth = 35;

            if (p.GetHealth() == 0) { e.Graphics.DrawImage(dead_ship, -(shipWidth / 2), -(shipWidth / 2), shipWidth, shipWidth); return; }
            if (p.isThrustOn())
            {
                e.Graphics.DrawImage(shipImagesThrust[p.GetID() % 8], -(shipWidth / 2), -(shipWidth / 2), shipWidth, shipWidth);
            }
            else
            {
                e.Graphics.DrawImage(shipImagesCoast[p.GetID() % 8], -(shipWidth / 2), -(shipWidth / 2), shipWidth, shipWidth);
            }






        }

        /// <summary>
        /// Acts as a drawing delegate for DrawObjectWithTransform
        /// After performing the necessary transformation (translate/rotate)
        /// DrawObjectWithTransform will invoke this method
        /// </summary>
        /// <param name="o">The object to draw</param>
        /// <param name="e">The PaintEventArgs to access the graphics</param>
        private void ShipInfoDrawer(object o, PaintEventArgs e)
        {
            Ship p = o as Ship;
            int shipWidth = 35;
            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            using (Font font1 = new Font("Times New Roman", 16, FontStyle.Bold, GraphicsUnit.Pixel))
            using (Pen pen = new Pen(Brushes.White, 2f))
            using (SolidBrush rectangleFiller = new SolidBrush(Color.Green))
            {
                if (p.GetHealth() == 0) { return; }

                PointF pointF1 = new PointF(-(shipWidth / 2) + 4, -(shipWidth / 2) - 20);
                e.Graphics.DrawString(p.GetName() + " " + p.GetScore(), font1, Brushes.White, pointF1);

                Rectangle rec = new Rectangle(-(shipWidth / 2) + 5, -(shipWidth / 2) + 40, 27, 7);
                e.Graphics.DrawRectangle(pen, rec);
                int length = p.GetHealth() * 5;
                Rectangle rec2 = new Rectangle(-(shipWidth / 2) + 6, -(shipWidth / 2) + 41, length, 5);
                e.Graphics.FillRectangle(rectangleFiller, rec2);


            }


        }

        /// <summary>
        /// Acts as a drawing delegate for DrawObjectWithTransform
        /// After performing the necessary transformation (translate/rotate)
        /// DrawObjectWithTransform will invoke this method
        /// </summary>
        /// <param name="o">The object to draw</param>
        /// <param name="e">The PaintEventArgs to access the graphics</param>
        private void ProjectileDrawer(object o, PaintEventArgs e)
        {
            Projectile p = o as Projectile;

            int projectileWidth = 20;

            if (p.isAlive())
            {
                e.Graphics.DrawImage(shipProjectile[p.getOwner() % 8], -(projectileWidth / 2), -(projectileWidth / 2), projectileWidth, projectileWidth);
            }



        }

        /// <summary>
        /// Acts as a drawing delegate for DrawObjectWithTransform
        /// After performing the necessary transformation (translate/rotate)
        /// DrawObjectWithTransform will invoke this method
        /// </summary>
        /// <param name="o">The object to draw</param>
        /// <param name="e">The PaintEventArgs to access the graphics</param>
        private void StarDrawer(object o, PaintEventArgs e)
        {
            Star p = o as Star;

            int shipWidth = 45;

            //pull image
            Image starImage = Image.FromFile("..\\..\\..\\Resources\\Images\\star.jpg");
            e.Graphics.DrawImage(starImage, -(shipWidth / 2), -(shipWidth / 2), shipWidth, shipWidth);

        }

        /// <summary>
        /// This method is invoked when the DrawingPanel needs to be re-drawn
        /// </summary>
        /// <param name="e"></param>
        protected override void OnPaint(PaintEventArgs e)
        {

            if (theWorld != null)
            {
                lock (this.theWorld)
                {
                    // Draw the ships
                    foreach (Ship ship in this.theWorld.GetShips().Values)
                    {
                        Vector2D shipLoc = ship.GetLocation();
                        Vector2D shipDir = ship.GetDirection();

                        if (shipDir == null)
                        {
                            DrawObjectWithTransform(e, ship, this.Size.Width, shipLoc.GetX(), shipLoc.GetY(), 0, new ObjectDrawer(ShipDrawer));
                            DrawObjectWithTransform(e, ship, this.Size.Width, shipLoc.GetX(), shipLoc.GetY(), 0, ShipInfoDrawer);
                        }
                        else
                        {
                            if (ship.GetHealth() == 0)
                            {
                                DrawObjectWithTransform(e, ship, this.Size.Width, shipLoc.GetX(), shipLoc.GetY(), 0, ShipDrawer);
                            }
                            else
                            {
                                DrawObjectWithTransform(e, ship, this.Size.Width, shipLoc.GetX(), shipLoc.GetY(), shipDir.ToAngle(), ShipDrawer);
                            }


                            DrawObjectWithTransform(e, ship, this.Size.Width, shipLoc.GetX(), shipLoc.GetY(), 0, ShipInfoDrawer);
                        }
                    }

                    // Draw the projectiles
                    foreach (Projectile projectile in this.theWorld.GetProjectile().Values)
                    {
                        Vector2D proLoc = projectile.GetLocation();
                        Vector2D proDir = projectile.GetDirection();

                        if (proDir == null)
                        {
                            DrawObjectWithTransform(e, projectile, this.Size.Width, proLoc.GetX(), proLoc.GetY(), 0, new ObjectDrawer(this.ProjectileDrawer));
                        }
                        else
                        {
                            DrawObjectWithTransform(e, projectile, this.Size.Width, proLoc.GetX(), proLoc.GetY(), proDir.ToAngle(), new ObjectDrawer(this.ProjectileDrawer));
                        }

                    }

                    // Draw the projectiles
                    foreach (Star star in this.theWorld.GetStar().Values)
                    {
                        Vector2D proLoc = star.GetLoc();


                        DrawObjectWithTransform(e, star, this.Size.Width, proLoc.GetX(), proLoc.GetY(), 0, new ObjectDrawer(this.StarDrawer));
                    }
                }
            }
            // Do anything that Panel (from which we inherit) needs to do
            base.OnPaint(e);
        }





    }
}
