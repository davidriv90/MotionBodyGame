using MotionBodyGameServer.Controller;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace MotionBodyGameServer.UI
{
   /// <summary>
   /// Lógica de interacción para TestSocket.xaml
   /// </summary>
   public partial class TestSocket : Window
   {
      SocketServer socketServer;
      //SocketClient socketClient;

      int port = 4321;

      public TestSocket()
      {
         InitializeComponent();
      }

      private void btnEnviar_Click(object sender, RoutedEventArgs e)
      {
         if (null == this.socketServer)
         //if(null == this.socketClient)
         { 
            MessageBox.Show("Hay que iniciar el servidor");
            return;
         }

         this.AddMessageFromServer(this.txtMensaje.Text);
         this.socketServer.EnviarMensajeAlAvatar(this.txtMensaje.Text);
         //this.socketClient.SendMessage(this.txtMensaje.Text);
      }

      private void btnIniciar_Click(object sender, RoutedEventArgs e)
      {
         if (null != this.socketServer)
         //if(null != this.socketClient)
         { return; }

         this.socketServer = new SocketServer(port);
         //this.socketClient = new SocketClient("localhost", port);

         this.socketServer.informarEstado += this.SetState;
         this.socketServer.informarError += this.SetErrorMessage;
         this.socketServer.informarMensajeCliente += this.AddMessageFromClient;
         this.socketServer.cerrarApp += this.closeApp;

         //this.socketClient.informarEstado += this.SetState;
         //this.socketClient.informarError += this.SetErrorMessage;
         //this.socketClient.informarMensajeServidor += this.AddMessageFromClient;
         //this.socketClient.cerrarApp += this.closeApp;

         this.socketServer.StartServer();
         //this.socketClient.StartClient();
      }

      private void SetState(String msg)
      {
         this.txtEstado.Dispatcher.Invoke(new Action(
            delegate()
            {
               this.txtEstado.Text = msg;
            }
            ));
      }

      private void SetErrorMessage(String msg)
      {
         this.AddMessage("\n---XXX--->" + msg);
      }

      private void AddMessageFromClient(String msg)
      {
         this.AddMessage("\n>>>" + msg);
      }

      private void AddMessageFromServer(String msg)
      {
         this.AddMessage("\n>" + msg);
      }

      private void AddMessage(String msg)
      {
         this.txtComunicacion.Dispatcher.Invoke(new Action(
            delegate()
            {
               this.txtComunicacion.Text += msg;
            }
            ));
      }

      private void btnDetener_Click(object sender, RoutedEventArgs e)
      {
         this.socketServer.EnviarMensajeAlAvatar(StandardMessages.END_GAME);
         //this.socketClient.SendMessage(StandardMessages.END_GAME);
      }

      private void closeApp()
      {
         this.Dispatcher.Invoke(new Action(
            delegate()
            {
               this.Close();
            }
            ));
      }
   }
}
