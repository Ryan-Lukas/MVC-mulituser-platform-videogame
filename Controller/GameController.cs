using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using SpaceWars;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Diagnostics;

namespace NetworkController
{
    public class GameController
    {
        private Socket theServer;
        private World theWorld;
        private int playerID;

        //Lists of objects that are waiting to updated into the game
        private List<Ship> updateShips;
        private List<Projectile> updateProjectile;
        private List<Star> updateStar;
        private string name;
        private int worldSize;

        public string Name { get => name; set => name = value; }

        public delegate void ServerUpdateHandler(List<Ship> updateShips, List<Projectile> updateProjectile, List<Star> updateStar);

        public delegate void ConnectedServerComplete(World newWorl, int playerID);

        public delegate void ShipUpdateToScoreDied(Ship updatedScoreShipsDied);

        public delegate void ShipUpdateToScoreNew(Ship updatedScoreShipNew);

        private event ServerUpdateHandler UpdateArrived;

        private event ConnectedServerComplete Connected;

        /// <summary>
        /// This method is called when the user clicks the connect button
        /// The method calls the ConnectToServer method in the networkcontroller class and also initializes some local lists.
        /// </summary>
        /// <param name="ipAddr"></param>
        private event ShipUpdateToScoreDied shipUpdatedDied;

        private event ShipUpdateToScoreNew shipUpdatedNew;

        public void Connect(string ipAddr)
        {
            theServer = Networking.ConnectToServer(FirstContact, ipAddr);
            theWorld = new World();

            updateStar = new List<Star>();
            updateShips = new List<Ship>();
            updateProjectile = new List<Projectile>();
        }
        /// <summary>
        /// This is the method that is fed into the connecttoserver method when the initial connection is started
        /// It sends the server the player name that the user chose
        /// It also sets the socketstate callme to the startup function
        /// </summary>
        /// <param name="ss"></param>
        private void FirstContact(SocketState ss)
        {
            ss.callMe = StartUp;
            Networking.Send(theServer, name);
            Networking.GetData(ss);
        }
        /// <summary>
        /// This method closes off the socket connection to the server
        /// </summary>
        public void Disconnect()
        {
            theServer.Close();
        }



        //after you get all data call GetData and then call UpdateCamFromServer
        public void RegisterServerUpdateHandler(ServerUpdateHandler h)
        {
            UpdateArrived += h;
        }

        public void RegisterConnectionHandler(ConnectedServerComplete h)
        {
            Connected += h;
        }

        public void RegisterShipUpdatorDied(ShipUpdateToScoreDied h)
        {
            shipUpdatedDied += h;
        }

        public void RegisterShipUpdatorNew(ShipUpdateToScoreNew h)
        {
            shipUpdatedNew += h;
        }




        /// <summary>
        /// Reads the json file and processes all the information that was parsed from the json
        /// </summary>
        /// <param name="ss"></param>
        private void ProcessMessage(SocketState ss)
        {
            StringBuilder incomingMessage = ss.sb;
            string error = "";
            if (incomingMessage != null)
            {
                try
                {
                    string[] parts = Regex.Split(incomingMessage.ToString(), @"(?<=[\n])");
                    List<Ship> updatedShips = new List<Ship>();
                    List<Projectile> updatedProjectiles = new List<Projectile>();
                    List<Star> updatedStars = new List<Star>();
                    List<Ship> shipScoreUpdateDied = new List<Ship>();
                    List<Ship> shipScoreUpdateNew = new List<Ship>();
                    foreach (string jsonObj in parts)
                    {

                        error = jsonObj;
                        if (jsonObj.Length == 0)
                            continue;

                        if (jsonObj[jsonObj.Length - 1] != '\n')
                        {
                            break;
                        }

                        if (jsonObj[0] == '{' && jsonObj[jsonObj.Length - 2] == '}' && jsonObj[jsonObj.Length - 1] == '\n')
                        {
                            Ship theShip = null;
                            Projectile theProjectile = null;
                            Star theStar = null;



                            JObject obj = JObject.Parse(jsonObj);


                            JToken shipProp = obj["ship"];
                            JToken projProp = obj["proj"];
                            JToken starProp = obj["star"];




                            if (shipProp != null)
                            {
                                theShip = (Ship)JsonConvert.DeserializeObject<Ship>(jsonObj);
                            }
                            else if (projProp != null)
                            {
                                theProjectile = (Projectile)JsonConvert.DeserializeObject<Projectile>(jsonObj);
                            }
                            else if (starProp != null)
                            {
                                theStar = (Star)JsonConvert.DeserializeObject<Star>(jsonObj);
                            }

                            lock (theWorld)
                            {
                                if (theShip != null)
                                {

                                    if (theWorld.GetShips().ContainsKey(theShip.GetID()))
                                    {
                                        theWorld.Ships[theShip.GetID()] = theShip;
                                        if (theShip.GetHealth() == 0)
                                        {
                                            shipScoreUpdateDied.Add(theShip);
                                        }
                                    }
                                    else
                                    {
                                        theWorld.AddShip(theShip);
                                        shipScoreUpdateNew.Add(theShip);

                                    }
                                    

                                    

                                }
                                else if (theProjectile != null)
                                {

                                    if (theProjectile.isAlive() == false)
                                    {
                                        if (theWorld.GetProjectile().ContainsKey(theProjectile.GetID()))
                                        {
                                            theWorld.Projectiles.Remove(theProjectile.GetID());
                                        }

                                    }
                                    else
                                    {
                                        if (theWorld.GetProjectile().ContainsKey(theProjectile.GetID()))
                                        {
                                            theWorld.Projectiles[theProjectile.GetID()] = theProjectile;
                                        }
                                        else
                                        {
                                            theWorld.AddProj(theProjectile);
                                        }
                                    }
                                    updateProjectile.Add(theProjectile);
                                }
                                else if (theStar != null)
                                {

                                    if (theWorld.GetStar().ContainsKey(theStar.GetID()))
                                    {
                                        theWorld.Stars[theStar.GetID()] = theStar;
                                    }
                                    else
                                    {
                                        theWorld.AddStar(theStar);
                                    }
                                    updateStar.Add(theStar);
                                }
                            }





                        }

                        ss.sb.Remove(0, jsonObj.Length); //Remove the part we just processed from the stringbuilder
                        




                    }
                    UpdateArrived(updateShips, updateProjectile, updateStar);
                    foreach(Ship ship in shipScoreUpdateDied)
                    {
                        shipUpdatedDied(ship);
                    }
                    foreach(Ship ship in shipScoreUpdateNew)
                    {
                        shipUpdatedNew(ship);
                    }
                }

                catch (JsonReaderException es)
                {
                }
                catch (Exception e) { }
            }

            ss.callMe = new NetworkAction(this.ProcessMessage);  //I dont think we need this, the game works when it is commented out
            Networking.GetData(ss);



        }

        /// <summary>
        /// This method first wraps the message in parenthesis
        /// The keyboard input message is then sent to the server
        /// </summary>
        /// <param name="sb"></param>
        public void SendMessage(StringBuilder sb)
        {
            StringBuilder s = sb;
            s.Append(")");
            s.Insert(0, "(");
            System.Diagnostics.Debug.WriteLine(s.ToString());
            Networking.Send(theServer, s.ToString());
        }
        /// <summary>
        /// This methods acts as the callme in the socketstate when the connection is first started
        /// This gets the world size and player id
        /// The method creates a new world the sets the callme to the process message function
        /// Getdata is then called
        /// </summary>
        /// <param name="ss"></param>
        private void StartUp(SocketState ss)
        {
            StringBuilder startingSocketState = ss.sb;
            try
            {
                string[] newServer = startingSocketState.ToString().Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);

                ss.sb.Remove(0, newServer[0].Length + 1);
                playerID = int.Parse(newServer[0]);
                ss.sb.Remove(0, newServer[0].Length + 1);
                worldSize = int.Parse(newServer[1]);
            }
            catch (Exception e) { }
            theWorld = new World(worldSize);

            if (Connected != null)
            {
                Connected(theWorld, playerID);
            }

            ss.callMe = ProcessMessage;
            //this.ProcessMessage(ss);
            Networking.GetData(ss);
        }
        /// <summary>
        /// A callback invoked when a send operation completes
        /// Move this function to a standalone networking library. 
        /// </summary>
        /// <param name="ar"></param>
        private void SendCallback(IAsyncResult ar)
        {
            Socket s = (Socket)ar.AsyncState;
            // Nothing much to do here, just conclude the send operation so the socket is happy.
            s.EndSend(ar);
        }
    }
}
