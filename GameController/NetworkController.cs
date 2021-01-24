using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Controller
{
    class NetworkController
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






        public delegate void NetworkAction(SocketState ss);

        public static class Networking
        {
            public const int DEFAULT_PORT = 11000;


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

                /**
                 * hostname - the name of the server to connect to

                callMe - a delegate function to be called when a connection is made. Your SpaceWars client will provide this function when it invokes ConnectToServer.

                ConnectToServer should attempt to connect to the server via a provided hostname. It should save the callMe function in a socket state object for use when data arrives.

                It will need to create a socket and then use the BeginConnect method. Note this method takes the "state" object and "regurgitates" it back to you when a connection is made, thus allowing communication between this function and the ConnectedCallback function (below).
                **/
            }

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

            public static void ConnectedCallback(IAsyncResult stateAsArObject)
            {
                SocketState ss = (SocketState)stateAsArObject.AsyncState;

                try
                {
                    // Complete the connection.
                    ss.theSocket.EndConnect(stateAsArObject);
                }
                catch (Exception e)
                {
                    System.Diagnostics.Debug.WriteLine("Unable to connect to server. Error occured: " + e);
                    return;
                }

                // Start an event loop to receive data from the server.
                ss.theSocket.BeginReceive(ss.messageBuffer, 0, ss.messageBuffer.Length, SocketFlags.None, ReceiveCallback, ss);
                /**
                This function is referenced by the BeginConnect method above and is called by the OS when the socket connects to the server. The "stateAsArObject" object contains a field "AsyncState" which contains the "state" object saved away in the above function. You will have to cast it from object to SocketState.

                Once a connection is established the "saved away" callMe needs to called. This function is saved in the socket state, and was originally passed in to ConnectToServer (above).

                   **/
            }


            /// <summary>
            /// This should be called after every message is received
            /// </summary>
            /// <param name="state"></param>
            public static void GetData(SocketState state)
            {
                SocketState ss = state;

                ss.theSocket.BeginReceive(ss.messageBuffer, 0, ss.messageBuffer.Length, SocketFlags.None, ReceiveCallback, ss);
            }

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

                // Continue the "event loop" that was started on line 100.
                // Start listening for more parts of a message, or more new messages

                //   ss.theSocket.BeginReceive(ss.messageBuffer, 0, ss.messageBuffer.Length, SocketFlags.None, ReceiveCallback, ss);

                try
                {
                    ss.theSocket.BeginReceive(ss.messageBuffer, 0, ss.messageBuffer.Length, SocketFlags.None, ReceiveCallback, ss);
                }
                catch (Exception e)
                { ss.theSocket.Close(); }






                /**
                 * The ReceiveCallback method is called by the OS when new data arrives. This method should check to see how much data has arrived. If 0, the connection has been closed (presumably by the server).
                 * On greater than zero data, this method should get the SocketState object out of the IAsyncResult (just as above in 2), and call the callMe provided in the SocketState.
                 **/
            }


            public static void Send(Socket socket, String data)
            {
                byte[] messageBytes = Encoding.UTF8.GetBytes(data + "\n");
                socket.BeginSend(messageBytes, 0, messageBytes.Length, SocketFlags.None, SendCallback, socket);

                /**
                 * This function (along with its helper 'SendCallback') will allow a program to send data over a socket. This function needs to convert the data into bytes and then send them using socket.BeginSend.
                 **/
            }

            public static void SendCallback(IAsyncResult ar)
            {
                Socket s = (Socket)ar.AsyncState;
                // Nothing much to do here, just conclude the send operation so the socket is happy.
                s.EndSend(ar);
                /**
                 * This function assists the Send function. It should extract the Socket out of the IAsyncResult, and then call socket.EndSend. You may, when first prototyping your program, put a WriteLine in here to see when data goes out.
                 **/
            }

        }
    }
}

