using MotionBodyGameServer.Controller;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MotionBodyGameServer
{
   class MainClass
   {
      public const int NORMAL_EXIT_CODE = 0;
      public const int ERROR_EXIT_CODE = 1;
      public const int CANCEL_EXIT_CODE = 2;

      public static void VerifyStartUpParams(ref String[] args)
      {
         if (args.Length < 3)
         {
            Environment.Exit(ERROR_EXIT_CODE);
         }

         if (null == args[0] || null == args[1] || null == args[2])
         {
            Environment.Exit(ERROR_EXIT_CODE);
         }

         Child.Name = args[0];
         Child.Sex = args[1];
         Child.GameLevel = Convert.ToSByte(args[2]);
      }

      /// <summary>
      /// Application Entry Point.
      /// </summary>
      //[System.Diagnostics.DebuggerNonUserCodeAttribute()]
      [System.STAThreadAttribute()]
      public static void Main(String[] args)
      {
         //MainClass.VerifyStartUpParams(ref args);

         //DATOS DE PRUEBA
         Child.Name = "Nombre Niño";
         Child.Sex = "Masculino";
         Child.GameLevel = 10;

         // TEST PARAMS
         //TestWindow winMain = new TestWindow();
         //winMain.ShowDialog();

         //TEST SKELETON
         //SkeletonWindow winSkeleton = new SkeletonWindow();
         //winSkeleton.ShowDialog();

         //TEST SOCKET CLIENT
         //SocketClient socketClient = new SocketClient("localhost", 4321);
         //socketClient.startClient();

         //TEST SOCKET SERVER
         //SocketServer socketServer = new SocketServer(4321);
         //socketServer.startServer();

         //TEST SOCKET GENERAL
         //TestSocketWindow winSocket = new TestSocketWindow();
         //winSocket.ShowDialog();
         MotionBodyGameServer.App appe = new App();
         appe.InitializeComponent();
         appe.Run();
      }
   }
}
