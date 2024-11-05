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
using System.Windows.Threading;

namespace LaOcaClient
{
    /// <summary>
    /// Interaction logic for VentanaCierreAutomatico.xaml
    /// </summary>
    public partial class VentanaCierreAutomatico : Window
    {
        private DispatcherTimer timer;

        public VentanaCierreAutomatico(string mensaje, string titulo, int segundos)
        {
            InitializeComponent();
            Title = titulo;
            lbMensaje.Content = mensaje;

            timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(segundos)
            };
            timer.Tick += Timer_Tick;
            timer.Start();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            timer.Stop();
            Close();
        }

        public static void Show(string mensaje, string titulo, int segundos)
        {
            VentanaCierreAutomatico msgBox = new VentanaCierreAutomatico(mensaje, titulo, segundos);
            msgBox.ShowDialog();
        }
    }
}

