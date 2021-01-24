using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using NetworkController;
using SpaceWars;

namespace SpaceWarsView
{
    public partial class Form1 : Form
    {
        
        private GameController gamecontroller;
        private int playerID;
        private int disconnect;
        ScorePanel scorePanel;
        private World theWorld;

        DrawingPanel drawingPanel;

        public Form1()
        {
            InitializeComponent();
            BackColor = Color.White;
            this.DoubleBuffered = true;
            theWorld = new World();
            gamecontroller = new GameController();
            disconnect = 0;
            Disconnect.Visible = false;
            HelpButton.Visible = false;
            HelpButton.Enabled = false;
        }   

        /// <summary>
        /// This method is called when the socket connection is successful
        /// This method sets up the world/drawingpanel/scoreboard
        /// </summary>
        /// <param name="newWorld"></param>
        /// <param name="Id"></param>
        private void Connected(World newWorld, int Id)
        {
            playerID = Id;

            this.Invoke(new MethodInvoker(delegate
            {
                theWorld = newWorld;
                ClientSize = new Size(theWorld.WorldSize + 250, theWorld.WorldSize + 140);


                drawingPanel = new DrawingPanel(theWorld);
                drawingPanel.Location = new Point(0, 140);
                drawingPanel.Size = new Size(theWorld.WorldSize, theWorld.WorldSize);
                drawingPanel.BackColor = Color.Black;
                drawingPanel.BringToFront();
                //drawingPanel.Size = new Size(theWorld.WorldSize,theWorld.WorldSize);

                scorePanel = new ScorePanel();
                scorePanel.startUp(newWorld);
                scorePanel.Location = new Point(theWorld.WorldSize + 10, 140);
                scorePanel.Size = new Size(220,theWorld.WorldSize);
                scorePanel.BackColor = Color.Black;
                BackColor = Color.HotPink;

                this.Controls.Add(scorePanel);
                gamecontroller.RegisterShipUpdatorDied(new GameController.ShipUpdateToScoreDied(this.scorePanel.updateToShipsDied));
                gamecontroller.RegisterShipUpdatorNew(new GameController.ShipUpdateToScoreNew(this.scorePanel.updateToShipsNew));
                this.Controls.Add(drawingPanel);

                spacewarspicture.Location = new Point((theWorld.WorldSize-350)/2,28);
                ScoreBoardPicture.BackColor = Color.Black;
                ScoreBoardPicture.Visible = true;
               
                ScoreBoardPicture.Location = new Point(theWorld.WorldSize+20,140);

                HelpButton.Visible = true;
                HelpButton.Enabled = true;
                HelpButton.Location = new Point(100, 5);
                
            }));
        }

        
        /// <summary>
        /// When the connect button is clicked, the ip address and player name are sent to the gamecontroller class
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Connect_Click(object sender, EventArgs e)
        {
            //Save IP address and player name
            string ipAddr = serverAddr.Text;
            string playerName = nameTextBox.Text;

            //Check for valid server address
            if (ipAddr == "")
            {
                MessageBox.Show("Please enter a server address");
                return;
            }
            //Disable Controls
            Connect.Enabled = false;
            serverAddr.Enabled = false;
            nameTextBox.Enabled = false;
            gamecontroller.Name = nameTextBox.Text;
            gamecontroller.Connect(ipAddr);





            //hiding previous server and name input
            serverAddr.Visible = false;
            ServerLabel.Visible = false;
            NameLabel.Visible = false;
            nameTextBox.Visible = false;
            Connect.Visible = false;
            //spacewarspicture.Visible = false;
            
            Disconnect.Visible = true;
            

            
            gamecontroller.RegisterConnectionHandler(Connected);
            gamecontroller.RegisterServerUpdateHandler(UpdateWorld);
            

            Disconnect.Enabled = true;
            this.KeyPreview = true;
            InitTimer();
        }
        /// <summary>
        /// This method is called when the objects need to be updated within the world
        /// The ships, stars, and projectiles that need updating are fed into the input parameters of this method
        /// </summary>
        /// <param name="updateShips"></param>
        /// <param name="updateProjectile"></param>
        /// <param name="updateStar"></param>
        private void UpdateWorld(List<Ship> updateShips, List<Projectile> updateProjectile, List<Star> updateStar)
        {
            if (!IsHandleCreated)
                return;

            lock (theWorld)
            {
                foreach(Ship ship in updateShips)
                {
                    if (!ship.GetActive())
                    {
                        theWorld.Ships.Remove(ship.GetID());
                    }
                    else
                    {
                        if (theWorld.Ships.ContainsKey(ship.GetID()))
                        {
                            theWorld.Ships[ship.GetID()] = ship;
                        }
                        else
                        {
                            theWorld.AddShip(ship);
                        }
                        
                    }
                }

                foreach(Projectile pro in updateProjectile)
                {
                    if (!pro.isAlive())
                    {
                        theWorld.Projectiles.Remove(pro.GetID());
                    }
                    else
                    {
                        if (theWorld.Projectiles.ContainsKey(pro.GetID()))
                        {
                            theWorld.Projectiles[pro.GetID()] = pro;
                        }
                        else
                        {
                            theWorld.AddProj(pro);
                        }
                    }
                }
            }

            MethodInvoker invalidator = new MethodInvoker(() => Invalidate(true));
            this.Invoke(invalidator);
        }
        /// <summary>
        /// When the disconnect button is clicked, we return back to the start screen
        /// You are only allowed to disconnect from a server once
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Disconnect_Click(object sender, EventArgs e)
        {
            
                serverAddr.Visible = true;
                ServerLabel.Visible = true;
                NameLabel.Visible = true;
                nameTextBox.Visible = true;
                Connect.Visible = true;

                gamecontroller.Disconnect();

            drawingPanel.BackColor = SystemColors.Control;
            drawingPanel.Visible = false;


            ClientSize = new Size(625, 200);

            Connect.Enabled = true;
            serverAddr.Enabled = true;
            nameTextBox.Enabled = true;

            Disconnect.Visible = false;
            disconnect++;

            scorePanel.Visible = false;
            scorePanel = null;

            Form1 newForm = new Form1();
            newForm.Show();
            this.Close();

        }
        private Timer timer1;  //This time determines how often we send messages to the server
        /// <summary>
        /// We send a message to the server every 10 milliseconds
        /// </summary>
        public void InitTimer()
        {
            timer1 = new Timer();
            timer1.Tick += new EventHandler(timer1_Tick);
            timer1.Interval = 10; // in miliseconds
            timer1.Start();
        }
        /// <summary>
        /// When the timer ticks, we call the gamecontroller sendmessage method
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void timer1_Tick(object sender, EventArgs e)
        {
            sb.Replace("(", "");   //Remove any parenthesis that could be left over
            sb.Replace(")", "");   //Remove any parenthesis that could be left over
            gamecontroller.SendMessage(sb);
        }
        private StringBuilder sb = new StringBuilder(50); //This stringbuilder contains the message that we want to send to the server
        /// <summary>
        /// If a key is pressed that is not in the stringbuilder, add it
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            e.SuppressKeyPress = true; //Get rid of those annoying keypress sounds
            if (e.KeyCode == Keys.W && sb.ToString().Contains("T") == false) { sb.Append("T"); }
            if (e.KeyCode == Keys.A && sb.ToString().Contains("L") == false) { sb.Append("L"); }
            if (e.KeyCode == Keys.D && sb.ToString().Contains("R") == false) { sb.Append("R"); }
            if (e.KeyCode == Keys.Space && sb.ToString().Contains("F") == false) { sb.Append("F"); }
        }
        /// <summary>
        /// When a key is released, remove it from the stringbuilder
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Form1_KeyUp(object sender, System.Windows.Forms.KeyEventArgs e)
        {
            if (e.KeyCode == Keys.W) { sb.Replace("T", ""); }
            if (e.KeyCode == Keys.A) { sb.Replace("L", ""); }
            if (e.KeyCode == Keys.D) { sb.Replace("R", ""); }
            if (e.KeyCode == Keys.Space) { sb.Replace("F", ""); }
        }

        private void HelpButton_Click(object sender, EventArgs e)
        {
            MessageBox.Show("W: Fire Thrusters\nA: Rotate Left\nD: Rotate Right\nSPACE: Fire");
        }
    }
}
