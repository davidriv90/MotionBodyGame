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

      private string host;
      private int port;

      public delegate void Mensajero(String msg);

      public event Mensajero informarError;
      public event Mensajero informarEstado;
      public event Mensajero informarMensajeServidor;

      public delegate void Desencadenador();

      public event Desencadenador cerrarApp;

      protected enum TipoMensaje
      {
         ERROR
         ,ESTADO
         ,MENSAJE_DEL_SERVIDOR
      }

      Stream conexStream = null;
      StreamReader sr = null;
      StreamWriter sw = null;

      Task readingService = null;
      bool isReading = true;

      public SocketClient(String host, int port)
      {
         this.host = host;
         this.port = port;
      }

      /// <summary>
      /// Permite ejecutar métodos en la interfaz de usuario en caso de estar asignados
      /// </summary>
      /// <param name="tipoMensaje"></param>
      /// <param name="mensaje"></param>
      private void informar(TipoMensaje tipoMensaje, String mensaje)
      {
         switch (tipoMensaje)
         {
            case TipoMensaje.ERROR:
               if (null != this.informarError) { this.informarError(mensaje); }
               break;
            case TipoMensaje.ESTADO:
               if (null != this.informarEstado) { this.informarEstado(mensaje); }
               break;
            case TipoMensaje.MENSAJE_DEL_SERVIDOR:
               if (null != this.informarMensajeServidor) { this.informarMensajeServidor(mensaje); }
               break;
         }
      }

      public void StartClient()
      {
         try
         {
            this.conexion = new TcpClient(host, port);
            this.informar(TipoMensaje.ESTADO, "Cliente iniciado");

            this.conexStream = this.conexion.GetStream();
            this.sr = new StreamReader(conexStream);
            this.sw = new StreamWriter(conexStream);
            this.sw.AutoFlush = true;

            this.informar(TipoMensaje.MENSAJE_DEL_SERVIDOR, this.sw.Encoding.ToString());

            Task readerHandlerTask = new Task(StartHandler, TaskCreationOptions.LongRunning);
            readerHandlerTask.Start();
         }
         catch (Exception ex)
         {
            this.informar(TipoMensaje.ERROR, ex.Message);
         }
      }

      private void StartHandler()
      {
         this.readingService = new Task(this.HandleServerMessages);
         this.readingService.Start();

         this.readingService.Wait();

         this.FinalizarCliente();
      }

      private void FinalizarCliente()
      {
         if (null != this.readingService)
         {
            this.readingService.Dispose();
         }

         if (null != this.conexStream)
         {
            this.conexStream.Close();
         }

         if (null != this.conexion)
         {
            this.conexion.Close();
         }

         this.informar(TipoMensaje.ESTADO, "Conexión terminada");

         Thread.Sleep(1000);

         this.cerrarApp();
      }

      private void HandleServerMessages()
      {
         while (this.isReading)
         {
            try
            {
               string msgFromServer = this.sr.ReadLine();

               if (msgFromServer.Contains(StandardMessages.END_GAME))
               {
                  this.isReading = false;
                  break;
               }

               this.informar(TipoMensaje.MENSAJE_DEL_SERVIDOR, msgFromServer);
            }
            catch (Exception ex)
            {
               this.informar(TipoMensaje.ERROR, ex.Message);
            }
         }
      }

      public void SendMessage(string message)
      {
         try
         {
            if (null == this.sw)
            {
               throw new Exception("No se ha iniciado el cliente");
            }

            this.sw.WriteLine(message);
         }
         catch (Exception ex)
         {
            this.informar(TipoMensaje.ERROR, ex.Message);
         }
      }
   }
}
