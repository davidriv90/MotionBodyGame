using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MotionBodyGameServer.Controller
{
   class SocketClient
   {
      private TcpClient conexion = null;

      private String host = null;
      private int port = -1;

      public delegate void Mensajero(String msg);

      public event Mensajero informarError;
      public event Mensajero informarMensajeServer;

      public SocketClient(String host, int port)
      {
         this.host = host;
         this.port = port;
      }

      public void startClient()
      {
         try
         {
            this.conexion = new TcpClient(host, port);

            Stream conexStream = this.conexion.GetStream();
            StreamReader sr = new StreamReader(conexStream);
            StreamWriter sw = new StreamWriter(conexStream);
            sw.AutoFlush = true;

            //while (true)
            //{
            Thread.Sleep(10);

            // Envío de datos
            sw.WriteLine("ça marche!!!");
            //}

            conexStream.Close();
         }
         catch (Exception ex)
         {
            informarError(ex.Message);
         }
         finally
         {
            if (null != this.conexion)
            { this.conexion.Close(); }
         }
      }
   }
}
