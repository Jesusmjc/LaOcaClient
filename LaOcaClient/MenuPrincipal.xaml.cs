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
    public partial class MenuPrincipal : Window
    {
        public MenuPrincipal()
        {
            InitializeComponent();
        }

        private void btnModificarCuenta_Click(object sender, RoutedEventArgs e)
        {
            int idCuenta = SingletonJugador.Instance.Jugador.IdCuenta;
            int idJugador = SingletonJugador.Instance.Jugador.IdJugador;
            CrearCuenta ventanaCrearCuenta = new CrearCuenta(ModoCuenta.Modificar, idCuenta, idJugador);
            ventanaCrearCuenta.ActualizarVentanaModificar(ModoCuenta.Modificar);
            ventanaCrearCuenta.Show();
            this.Close();
        }

        private void BtnSalir_Click(object sender, RoutedEventArgs e)
        {
            IniciarSesion ventanaIniciarSesion = new IniciarSesion();
            ventanaIniciarSesion.Show();
            this.Close();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Sala ventanaSala = new Sala();
            this.Close();
            ventanaSala.ShowDialog();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            LaOcaPartida laOcaPartida = new LaOcaPartida();
            this.Close();
            laOcaPartida.ShowDialog();
        }
    }
}