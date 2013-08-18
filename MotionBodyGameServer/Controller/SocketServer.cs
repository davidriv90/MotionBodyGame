using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MotionBodyGameServer.Controller
{
   class SocketServer
   {
      private TcpListener listener;

      public delegate void Mensajero(String msg);

      public event Mensajero informarError;
      public event Mensajero informarEstado;
      public event Mensajero informarMensajeCliente;

      public delegate void Desencadenador();

      public event Desencadenador cerrarApp;

      protected enum TipoMensaje
      {
         ERROR
         ,ESTADO
         ,MENSAJE_DEL_CLIENTE
      }

      //OBJETOS PARA COMUNICARSE A TRAVÉS DE UN SOCKET
      Socket conector = null;

      Stream medioTransmisionMensajes = null;
      StreamReader lectorMensaje = null;
      StreamWriter escritorMensaje = null;

      Task readingService = null;
      bool isReading = true;

      public SocketServer(int port)
      {
         IPAddress ip_address = new IPAddress(new byte[] { 127, 0, 0, 1 });
         listener = new TcpListener(ip_address, port);
      }

      public void StartServer()
      {
         listener.Start();
         this.informar(TipoMensaje.ESTADO, "Servidor activado");

         Task listeningService = new Task(EsperarCliente, TaskCreationOptions.LongRunning);
         listeningService.Start();
      }

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
            case TipoMensaje.MENSAJE_DEL_CLIENTE:
               if (null != this.informarMensajeCliente) { this.informarMensajeCliente(mensaje); }
               break;
         }
      }

      private void EsperarCliente()
      {
         this.conector = this.listener.AcceptSocket();

         this.informar(TipoMensaje.ESTADO, "Connectado a: " + this.conector.RemoteEndPoint);

         try
         {
            this.medioTransmisionMensajes = new NetworkStream(this.conector);
            this.lectorMensaje = new StreamReader(this.medioTransmisionMensajes);
            this.escritorMensaje = new StreamWriter(this.medioTransmisionMensajes);

            this.informar(TipoMensaje.MENSAJE_DEL_CLIENTE, this.escritorMensaje.Encoding.ToString());

            this.escritorMensaje.AutoFlush = true;

            this.readingService = new Task(this.EscucharCliente);
            this.readingService.Start();

            this.readingService.Wait();

            this.FinalizarServidor();
            //this.informar(TipoMensaje.ESTADO, "Después de esperar");
            //Validar inicio de transmisión?

            //Envío de un mensaje de confirmación de conexión
            //this.escritorMensaje.WriteLine(StandardMessages.START_GAME);
         }
         catch (Exception ex)
         {
            this.informar(TipoMensaje.ERROR, ex.Message);
         }
      }

      private void EscucharCliente()
      {
         while (this.isReading)
         {
            try
            {
               string msgFromClient = this.lectorMensaje.ReadLine();

               // Se analiza el tipo de mensaje recibido
               if (msgFromClient.Contains(StandardMessages.END_GAME)) 
               {
                  this.isReading = false;
                  break;
               }

               this.informar(TipoMensaje.MENSAJE_DEL_CLIENTE, msgFromClient);
            }
            catch (Exception ex)
            {
               this.informar(TipoMensaje.ERROR, ex.Message);
            }
         }
      }

      public void EnviarMensajeAlAvatar(string msg)
      {
         try
         {
            if (null == this.escritorMensaje) throw new Exception("Aún no se ha conectado el avatar");

            this.escritorMensaje.WriteLine(msg);
         }
         catch (Exception ex)
         {
            this.informar(TipoMensaje.ERROR, ex.Message);
         }
      }

      private void FinalizarServidor()
      {
         // Finalizar lector de mensajes del cliente
         //Thread.Sleep(3000);

         if (null != this.readingService)
         {
            //this.readingService.Wait();
            this.readingService.Dispose();
         }

         // Cerrar el transmisor
         if (null != this.medioTransmisionMensajes) { this.medioTransmisionMensajes.Close(); }

         // Liberar el conector
         if (null != this.conector) { this.conector.Close(); }

         this.informar(TipoMensaje.ESTADO, "Conexión terminada");

         Thread.Sleep(3000);

         this.cerrarApp();
      }
   }
}
