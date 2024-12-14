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
    public partial class VentanaCierreAutomatico : Window
    {
        private DispatcherTimer _timer;

        public VentanaCierreAutomatico(string mensaje, string titulo, int segundos)
        {
            InitializeComponent();
            Title = titulo;
            lbMensaje.Content = mensaje;

            _timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(segundos)
            };
            _timer.Tick += Timer_Tick;
            _timer.Start();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            _timer.Stop();
            Close();
        }

        public static void Show(string mensaje, string titulo, int segundos)
        {
            VentanaCierreAutomatico msgBox = new VentanaCierreAutomatico(mensaje, titulo, segundos);
            msgBox.ShowDialog();
        }
    }
}

