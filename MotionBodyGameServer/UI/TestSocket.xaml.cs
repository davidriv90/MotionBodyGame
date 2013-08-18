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
      SocketServer socket;

      public TestSocket()
      {
         InitializeComponent();
      }

      private void btnEnviar_Click(object sender, RoutedEventArgs e)
      {
         if (null == this.socket) 
         { 
            MessageBox.Show("Hay que iniciar el servidor");
            return;
         }

         this.AddMessageFromServer(this.txtMensaje.Text);
         this.socket.EnviarMensajeAlAvatar(this.txtMensaje.Text);
      }

      private void btnIniciar_Click(object sender, RoutedEventArgs e)
      {
         if (null != this.socket) { return; }

         this.socket = new SocketServer(4321);

         this.socket.informarEstado += this.SetState;
         this.socket.informarError += this.SetErrorMessage;
         this.socket.informarMensajeCliente += this.AddMessageFromClient;

         this.socket.cerrarApp += this.closeApp;

         this.socket.StartServer();
      }

      private void SetState(String msg)
      {
         //if (msg.Contains(StandardMessages.END_GAME))
         //{
         //   // Una vez que se ha salido del ciclo, es importante cerrar la conexión del lado del servidor
         //   this.socket.FinalizarServidor();
         //}

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
         this.socket.EnviarMensajeAlAvatar(StandardMessages.END_GAME);
         //this.socket.FinalizarServidor();
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
