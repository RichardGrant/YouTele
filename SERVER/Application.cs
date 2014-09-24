/*
	Richard D. Grant
	R.grant.jr.122193@gmail.com
	-Contact for details-
*/
using System;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;
using System.Text;

namespace SERVER{
	static class Application{
		private static readonly List<Socket> _client_list = new List<Socket>();
		private const ushort _port = 80, _buffer_len = 1024*2;
		private static byte[] _buf = new byte[_buffer_len];

		private static Boolean exiting = false;
		static int Main(){
			Console.Title = "Server Application";
			Socket server_socket = StartServer();
			while(!exiting){

			}
			return CloseServer(server_socket);
		}

		private static Socket StartServer(){
			IPEndPoint IPE = new IPEndPoint(IPAddress.Any, _port); // 192.168.0.14:80
			Socket server_socket = new Socket(IPE.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
			server_socket.Bind(IPE);
			server_socket.Listen(200);
			server_socket.BeginAccept(AcceptCallback, server_socket);
			Console.WriteLine("Server Succeeded.");
			return server_socket;
		}
		private static void AcceptCallback(IAsyncResult AR){
			Socket server_socket = (Socket)AR.AsyncState;
			Socket client_socket = server_socket.EndAccept(AR);

			_client_list.Add(client_socket);
			client_socket.BeginReceive(_buf, 0, _buffer_len, SocketFlags.None, RecCallback, client_socket);

			server_socket.BeginAccept(AcceptCallback, server_socket);
		}
		private static void RecCallback(IAsyncResult AR){
			Socket client_socket = (Socket)AR.AsyncState;
			int rec_len = client_socket.EndReceive(AR);

			byte[] rec_buf = new byte[rec_len];
			Array.Copy(_buf, rec_buf, rec_len);

			string rec_text = Encoding.ASCII.GetString(rec_buf);

			if(rec_text == "close_server"){
				client_disconnect(client_socket);
				if(_client_list.Count <= 0){
					exiting = true;
				}
			}else{
				client_socket.Send(Encoding.ASCII.GetBytes("Recieved: " + rec_text));
			}
			client_socket.BeginReceive(_buf, 0, _buffer_len, SocketFlags.None, RecCallback, client_socket);
		}

		private static void client_disconnect(Socket soc){
			soc.Shutdown(SocketShutdown.Both);
			soc.Close();
		}
		private static ushort CloseServer(Socket server_socket){
			foreach(Socket client in _client_list){
				client_disconnect(client);
			}
			server_socket.Close();
			return 0;
		}
	}
}