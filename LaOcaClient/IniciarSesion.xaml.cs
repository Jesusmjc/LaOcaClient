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

namespace LaOcaClient
{
    public partial class IniciarSesion : Window
    {
        public IniciarSesion()
        {
            InitializeComponent();
        }
        private void ManejarFocoTextBox(object remitente, RoutedEventArgs e)
        {
            TextBox cuadroTexto = remitente as TextBox;
            if (cuadroTexto.Text == cuadroTexto.Tag.ToString())
            {
                cuadroTexto.Text = "";
                cuadroTexto.Foreground = Brushes.Black;
            }
        }

        private void ManejarPerdidaFocoTextBox(object remitente, RoutedEventArgs e)
        {
            TextBox cuadroTexto = remitente as TextBox;
            if (string.IsNullOrWhiteSpace(cuadroTexto.Text))
            {
                cuadroTexto.Text = cuadroTexto.Tag.ToString();
                cuadroTexto.Foreground = Brushes.Gray;
            }
        }

        private void btnIniciarSesion_Click(object sender, RoutedEventArgs e)
        {
            MenuPrincipal menuPrincipalWindow = new MenuPrincipal();
            menuPrincipalWindow.Show();
            this.Close();
        }

        private void btnOlvideMiContraseña_Click(object sender, RoutedEventArgs e)
        {
            RecuperarContraseña ventanaRecuperarContraseña = new RecuperarContraseña();
            ventanaRecuperarContraseña.Show();
            this.Close();
        }

        private void btnCrearCuentaNueva_Click(object sender, RoutedEventArgs e)
        {
            CrearCuenta crearCuentaWindow = new CrearCuenta(ModoCuenta.Crear);
            crearCuentaWindow.Show();
            this.Close();
        }

        private void btnJugarComoInvitado_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}