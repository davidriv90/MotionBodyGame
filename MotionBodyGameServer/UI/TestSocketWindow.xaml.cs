using Microsoft.Kinect;
using Microsoft.Kinect.Toolkit;
using Microsoft.Samples.Kinect.WpfViewers;
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
   /// Lógica de interacción para TestSocketWindow.xaml
   /// </summary>
   public partial class TestSocketWindow : Window
   {
      private SocketServer conexServer = null;

      // ==================================== //
      // ======= << ATRIBUTOS >> ======= //
      // ================================== //

      public static readonly DependencyProperty KinectSensorManagerProperty =
          DependencyProperty.Register(
              "KinectSensorManager",
              typeof(KinectSensorManager),
              typeof(TestSocketWindow),
              new PropertyMetadata(null));

      // Selector de dispositivos kinect conectados
      private readonly KinectSensorChooser sensorChooser = new KinectSensorChooser();

      private const int FPS = 1;

      private const double TICK_FRAMES = 1000 / FPS;

      private DateTime lastFrameSent = DateTime.MinValue;

      private Skeleton[] skels;

      // ==================================== //
      // ======= << PROPIEDADES >> ======= //
      // ================================== //

      public KinectSensorManager KinectManager
      {
         get { return (KinectSensorManager)GetValue(KinectSensorManagerProperty); }
         set { SetValue(KinectSensorManagerProperty, value); }
      }

      public TestSocketWindow()
      {
         // Configurar el administrador del Kinect
         this.KinectManager = new KinectSensorManager();
         this.KinectManager.KinectSensorChanged += kinectSensorChanged;
         this.DataContext = this.KinectManager;

         InitializeComponent();

         //this.sensorChooserUI.KinectSensorChooser = this.sensorChooser;
         this.sensorChooser.Start();

         var kinectSensorBinding = new Binding("Kinect") { Source = this.sensorChooser };
         BindingOperations.SetBinding(this.KinectManager, KinectSensorManager.KinectSensorProperty, kinectSensorBinding);

         //SERVIDOR
         this.inicializarSocketServidor();
      }

      /// <summary>
      /// Método ejecutado automáticamente al cambiar el sensor kinect
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      private void kinectSensorChanged(object sender, KinectSensorManagerEventArgs<Microsoft.Kinect.KinectSensor> e)
      {
         // Si el sensor actual no es nulo
         if (null != e.OldValue)
         {
            this.stopKinectService(e.OldValue);
         }

         // Si el nuevo sensor detectado no es nulo
         if (null != e.NewValue)
         {
            this.startKinectService(this.KinectManager, e.NewValue);
         }
      }

      private void stopKinectService(Microsoft.Kinect.KinectSensor unpluggedKinectSensor)
      {
         unpluggedKinectSensor.SkeletonFrameReady -= this.skeletonTracking;
      }

      private void startKinectService(KinectSensorManager kinectSensorManager, Microsoft.Kinect.KinectSensor pluggedKinectSensor)
      {
         pluggedKinectSensor.SkeletonFrameReady += this.skeletonTracking;

         kinectSensorManager.SkeletonStreamEnabled = true;
         kinectSensorManager.KinectSensorEnabled = true;
      }

      private void skeletonTracking(object sender, Microsoft.Kinect.SkeletonFrameReadyEventArgs e)
      {
         using (SkeletonFrame skeletonFrame = e.OpenSkeletonFrame())
         {
            if (skeletonFrame != null)
            {
               if ((null == skels) || (skels.Length != skeletonFrame.SkeletonArrayLength))
               {
                  this.skels = new Skeleton[skeletonFrame.SkeletonArrayLength];
               }
               skeletonFrame.CopySkeletonDataTo(skels);
            }
         }

         foreach (Skeleton skel in skels)
         {
            if (SkeletonTrackingState.Tracked != skel.TrackingState)
            {
               txtEstado.Text = "No hay skeleton";
            }
            else
            {
               txtEstado.Text = "Skeleton detectado";

               DateTime now = DateTime.Now;

               // Si no se había inicializado el LastFrameSent
               if (this.lastFrameSent == DateTime.MinValue)
               { this.lastFrameSent = now; }

               // Si el tiempo transcurrido es menor al 95% del thick
               double ms = now.Subtract(this.lastFrameSent).Milliseconds;
               //double ms = this.lastFrameSent.Subtract(now).Milliseconds;
               if (ms < (TICK_FRAMES * 0.95))
               { return; }

               //txtMensajes.Text = cabeza.Position.X + " " + cabeza.Position.Y + " " + cabeza.Position.Z;
               //string posiciones =
               //   skel.Joints[JointType.HandRight].Position.X + " " +
               //   skel.Joints[JointType.HandRight].Position.Y + " " +
               //   skel.Joints[JointType.HandRight].Position.Z;

               // SE ESTÁ PROBANDO EL MÉTODO QUE GENERA TODAS LAS POSICIONES EN UNA SOLA CADENA
               this.conexServer.EnviarMensajeAlAvatar(this.GenerarStringSkeletonData(skel.Joints));

               this.lastFrameSent = now;
               break;
            }
         }
      }

      private void inicializarSocketServidor()
      {
         conexServer = new SocketServer(4321);
         conexServer.informarError += this.mostrarError;
         conexServer.informarEstado += this.mostrarEstadoServidor;
         conexServer.informarMensajeCliente += this.mostrarMensajeRecibido;

         conexServer.StartServer();
         //this.conexServer.EnviarMensajeAlAvatar("Un mensaje enviado");
      }

      void mostrarError(string msg)
      {
         txtMensajes.Dispatcher.Invoke(new Action(
            delegate()
            {
               txtMensajes.Text = msg;
            }
            ));
      }

      void mostrarMensajeRecibido(string msg)
      {
         txtMensajes.Dispatcher.Invoke(new Action(
            delegate()
            {
               txtMensajes.Text += "\n" + msg;
            }
            ));
      }

      void mostrarEstadoServidor(string msg)
      {
         txtEstado.Dispatcher.Invoke(new Action(
            delegate()
            {
               txtEstado.Text = msg;
            }
            ));
      }

      private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
      {
         this.sensorChooser.Stop();

         // Ya no se usa porque se envía un mensaje de finalización
         //this.conexServer.FinalizarServidor();
         this.SendEndMessage();
      }

      private void Window_Closed(object sender, EventArgs e)
      {
         this.KinectManager.KinectSensor = null;
      }

      private String GenerarStringSkeletonData(JointCollection joints)
      {
         StringBuilder myStringData = new StringBuilder("{skeleton{");
         StringBuilder jointsData = new StringBuilder();

         foreach (Joint jointPoint in joints)
         {
            if (jointsData.Length > 0)
            {
               jointsData.Append(",");
            }

            jointsData.Append(jointPoint.JointType).Append("[")
            .Append(jointPoint.Position.X).Append(",")
            .Append(jointPoint.Position.Y).Append(",")
            .Append(jointPoint.Position.Z)
            .Append("]");
         }

         myStringData.Append(jointsData);
         myStringData.Append("}").Append("}");

         return myStringData.ToString();
      }

      public void SendEndMessage()
      {
         this.conexServer.EnviarMensajeAlAvatar(StandardMessages.END_GAME);
      }
   }
}
