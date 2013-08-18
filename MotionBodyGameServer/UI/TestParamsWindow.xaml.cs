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
   /// Lógica de interacción para MainWindow.xaml
   /// </summary>
   public partial class TestParamsWindow : Window
   {
      public TestParamsWindow()
      {
         InitializeComponent();

         this.FillFormWithChildParams();
      }

      private void FillFormWithChildParams()
      {
         txtChildName.Text = Child.Name;
         txtChildSex.Text = Child.Sex;
         txtStartUpLevel.Text = Child.GameLevel.ToString();
      }
   }
}
