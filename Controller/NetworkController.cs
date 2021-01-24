using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace NetworkController
{
    public class SocketState
    {
        public Socket theSocket;
        public int ID;

        // This is the buffer where we will receive data from the socket
        public byte[] messageBuffer = new byte[1024];

        // This is a larger (growable) buffer, in case a single receive does not contain the full message.
        public StringBuilder sb = new StringBuilder();

        public NetworkAction callMe;

        public SocketState(Socket s, int id)
        {
            theSocket = s;
            ID = id;
        }
    }

    public class ConnectionState
    {
        private TcpListener listener;
        private NetworkAction callMe;



        public ConnectionState(NetworkAction action, TcpListener lstn)
        {
            callMe = action;
            listener = lstn;
        }

        public TcpListener Listener { get => listener; set => listener = value; }
        public NetworkAction CallMe { get => callMe; set => callMe = value; }
    }




    public delegate void NetworkAction(SocketState ss);

    public static class Networking
    {
        public const int DEFAULT_PORT = 11000;

        /// <summary>
        /// It will need to create a socket and then use the BeginConnect method.
        /// Note this method takes the "state" object and "regurgitates" it back to you when a connection is made, thus allowing communication 
        /// between this function and the ConnectedCallback function (below).
        /// </summary>
        /// <param name="_call"></param>
        /// <param name="hostName"></param>
        /// <returns></returns>
        public static Socket ConnectToServer(NetworkAction _call, string hostName)
        {

            System.Diagnostics.Debug.WriteLine("connecting  to " + hostName);

            // Create a TCP/IP socket.
            Socket socket;
            IPAddress ipAddress;

            Networking.MakeSocket(hostName, out socket, out ipAddress);

            SocketState ss = new SocketState(socket, -1);
            ss.callMe = _call;

            socket.BeginConnect(ipAddress, Networking.DEFAULT_PORT, ConnectedCallback, ss);

            return socket;
        }
        /// <summary>
        /// This method makes the connection(socket) between the client and the server
        /// The method takes in a hostname/ip and spits out an ip and a socket connection
        /// </summary>
        /// <param name="hostName"></param>
        /// <param name="socket"></param>
        /// <param name="ipAddress"></param>
        private static void MakeSocket(string hostName, out Socket socket, out IPAddress ipAddress)
        {
            ipAddress = IPAddress.None;
            socket = null;
            try
            {
                // Establish the remote endpoint for the socket.
                IPHostEntry ipHostInfo;

                // Determine if the server address is a URL or an IP
                try
                {
                    ipHostInfo = Dns.GetHostEntry(hostName);
                    bool foundIPV4 = false;
                    foreach (IPAddress addr in ipHostInfo.AddressList)
                        if (addr.AddressFamily != AddressFamily.InterNetworkV6)
                        {
                            foundIPV4 = true;
                            ipAddress = addr;
                            break;
                        }
                    // Didn't find any IPV4 addresses
                    if (!foundIPV4)
                    {
                        System.Diagnostics.Debug.WriteLine("Invalid addres: " + hostName);
                        throw new ArgumentException("Invalid address");
                    }
                }
                catch (Exception)
                {
                    // see if host name is actually an ipaddress, i.e., 155.99.123.456
                    System.Diagnostics.Debug.WriteLine("using IP");
                    ipAddress = IPAddress.Parse(hostName);
                }

                // Create a TCP/IP socket.
                socket = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

                socket.SetSocketOption(SocketOptionLevel.IPv6, SocketOptionName.IPv6Only, false);

                // Disable Nagle's algorithm - can speed things up for tiny messages, 
                // such as for a game
                socket.NoDelay = true;

            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine("Unable to create socket. Error occured: " + e);
                throw new ArgumentException("Invalid address");
            }
        }
        /// <summary>
        /// This function is referenced by the BeginConnect method above and is called by the OS when the socket connects to the server. 
        /// The "stateAsArObject" object contains a field "AsyncState" which contains the "state" object saved away in the above function. 
        /// Once a connection is established the "saved away" callMe needs to called.This function is saved in the socket state, and was 
        /// originally passed in to ConnectToServer (above).
        /// </summary>
        /// <param name="stateAsArObject"></param>
        public static void ConnectedCallback(IAsyncResult stateAsArObject)
        {
            SocketState ss = (SocketState)stateAsArObject.AsyncState;

            try
            {
                // Complete the connection.
                ss.theSocket.EndConnect(stateAsArObject);
                ss.callMe(ss);
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine("Unable to connect to server. Error occured: " + e);
                return;
            }
        }


        /// <summary>
        /// This should be called after every message is received
        /// This method calls beginrecieve in order to start recieving messages from the server
        /// </summary>
        /// <param name="state"></param>
        public static void GetData(SocketState state)
        {
            SocketState ss = state;
            try
            {
                ss.theSocket.BeginReceive(ss.messageBuffer, 0, ss.messageBuffer.Length, SocketFlags.None, ReceiveCallback, ss);
            }
            catch (Exception e) { }
        }
        /// <summary>
        /// The ReceiveCallback method is called by the OS when new data arrives. 
        /// This method should check to see how much data has arrived. 
        /// If 0, the connection has been closed (presumably by the server). 
        /// On greater than zero data, this method should get the SocketState object out of the IAsyncResult (just as above in 2), and call the callMe provided in the SocketState.
        /// </summary>
        /// <param name="stateAsArObject"></param>
        public static void ReceiveCallback(IAsyncResult stateAsArObject)
        {
            SocketState ss = (SocketState)stateAsArObject.AsyncState;

            int bytesRead = 0;
            try
            {
                bytesRead = ss.theSocket.EndReceive(stateAsArObject);
            }
            catch (Exception e)
            { ss.theSocket.Close(); }

            // If the socket is still open
            if (bytesRead > 0)
            {
                string theMessage = Encoding.UTF8.GetString(ss.messageBuffer, 0, bytesRead);
                // Append the received data to the growable buffer.
                // It may be an incomplete message, so we need to start building it up piece by piece
                ss.sb.Append(theMessage);

                ss.callMe(ss);
            }
        }

        /// <summary>
        /// This function (along with its helper 'SendCallback') will allow a program to send data over a socket. 
        /// This function needs to convert the data into bytes and then send them using socket.BeginSend.
        /// </summary>
        /// <param name="socket"></param>
        /// <param name="data"></param>
        public static void Send(Socket socket, String data)
        {
            try
            {
                byte[] messageBytes = Encoding.UTF8.GetBytes(data + "\n");
                socket.BeginSend(messageBytes, 0, messageBytes.Length, SocketFlags.None, new AsyncCallback(SendCallback), socket);
                
            }
            catch (Exception e) {

                try
                {
                    socket.Shutdown(SocketShutdown.Both);
                    socket.Close();
                }
                catch { }
            }
          
        }
        /// <summary>
        /// A version of the send method above that says whether the send went through or not
        /// This detects if a client disconnects
        /// </summary>
        /// <param name="socket"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public static bool bool_send(Socket socket, String data)
        {
            try
            {
                byte[] messageBytes = Encoding.UTF8.GetBytes(data + "\n");
                socket.BeginSend(messageBytes, 0, messageBytes.Length, SocketFlags.None, new AsyncCallback(SendCallback), socket);
                return true;
            }
            catch (Exception e)
            {

                try
                {
                    socket.Shutdown(SocketShutdown.Both);
                    socket.Close();
                }
                catch { }
                return false;
            }


        }
        /// <summary>
        /// This function assists the Send function. 
        /// It should extract the Socket out of the IAsyncResult, and then call socket.EndSend. 
        /// </summary>
        /// <param name="ar"></param>
        public static void SendCallback(IAsyncResult ar)
        {
            try
            {
                Socket s = (Socket)ar.AsyncState;
                // Nothing much to do here, just conclude the send operation so the socket is happy.
                s.EndSend(ar);
            }
            catch{ }
        }

        public static void ServerAwaitingClientLoop(NetworkAction action)
        {
            Console.WriteLine("Server is up. Waiting for first Client");
            TcpListener lstn = new TcpListener(IPAddress.Any, DEFAULT_PORT);
            try
            {
                lstn.Start();
                ConnectionState cs = new ConnectionState(action, lstn);
                cs.Listener = lstn;
                cs.CallMe = action;
                lstn.BeginAcceptSocket(AcceptNewClient, cs);
            }
            catch (Exception e) { }

        }

        
        private static void AcceptNewClient(IAsyncResult ar)
        {
            Console.WriteLine("A new Client has contacted the Server.");
            ConnectionState cs = (ConnectionState)ar.AsyncState;
            Socket socket = null;
            try
            {
                socket = cs.Listener.EndAcceptSocket(ar);
            }
            catch (Exception e)
            {
                cs.Listener.BeginAcceptSocket(AcceptNewClient, cs);
                return;
            }
            SocketState ss = new SocketState(socket, -1);
            ss.callMe = cs.CallMe;
            ss.callMe(ss);
            cs.Listener.BeginAcceptSocket(AcceptNewClient, cs);

        }

    }
}
