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
            int idCuenta = 2;
            int idJugador = 2;
            CrearCuenta ventanaCrearCuenta = new CrearCuenta(ModoCuenta.Modificar, idCuenta, idJugador);
            ventanaCrearCuenta.ActualizarVentanaModificar(ModoCuenta.Modificar);
            ventanaCrearCuenta.ShowDialog();
        }
    }
}