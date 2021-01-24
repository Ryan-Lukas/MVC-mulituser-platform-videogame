using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using SpaceWars;
using static Controller.NetworkController;

namespace Controller
{
    public class GameController
    {
        private Socket theServer;
        private int playerID;
        private World theWorld;
        private int worldSize;

        private StringBuilder current_message = new StringBuilder(50);

        public delegate void ConnectedServerComplete(World newWorl, int playerID);
        private event ConnectedServerComplete Connected;

        public void RegisterConnectionHandler(ConnectedServerComplete h)
        {
            Connected += h;
        }

        public void Connect(string ipAddr)
        {
            theServer = Networking.ConnectToServer(StartUp, ipAddr);
        }

        private void StartUp(SocketState ss)
        {
            StringBuilder startingSocketState = ss.sb;
            try
            {
                string[] newServer = startingSocketState.ToString().Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);


                playerID = int.Parse(newServer[0]);
                worldSize = int.Parse(newServer[1]);
            }
            catch (Exception e) { }
            theWorld = new World(worldSize);

            Connected(theWorld, playerID);

            ss.callMe = new NetworkAction(ProcessMessage);
            this.ProcessMessage(ss);
        }

        private void ProcessMessage(SocketState ss)
        {
            /**
             * 
             * 
             * ToDo
             * 
             * 
             * */
            throw new NotImplementedException();
        }

        public void sendInputsHere(string message)
        {
            current_message.Append(message);
        }
        public void deleteInputsHere(string input)
        {
            current_message.Replace(input, "");
        }

        private void sendCurrentData()
        {
            current_message.Append(")");
            current_message.Insert(0, "(");
            string s = current_message.ToString();
            if (s.IndexOf("(") != s.LastIndexOf("(") || s.IndexOf(")") != s.LastIndexOf(")")) { current_message.Clear(); return; }
            string pattern = "^[(][RLTF]+[)]$";
            Regex rx = new Regex(pattern);
            Match result = Regex.Match(current_message.ToString(), pattern);
            if (!(result.Success)) { return; }
            System.Diagnostics.Debug.WriteLine(current_message.ToString());
            Networking.Send(theServer, current_message.ToString());
            //current_message.Clear();
        }




        public void player_name(string name)
        {
            Networking.Send(theServer, name);
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
