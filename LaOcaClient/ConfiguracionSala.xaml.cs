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
    public partial class ConfiguracionSala : Window
    {
        public ConfiguracionSala()
        {
            InitializeComponent();
            cbVisibilidad.Items.Add(Properties.Resources.globalPublica);
            cbVisibilidad.Items.Add(Properties.Resources.globalPrivada);
            tbxNombreSala.Text = Properties.Resources.lbSalaDe + SingletonJugador.Instance.Jugador.NombreUsuario;
            cbVisibilidad.SelectedIndex = 0;
        }

        private void IrASala(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(tbxNombreSala.Text.ToString()) && !string.IsNullOrWhiteSpace(cbVisibilidad.Text.ToString()))
            {
                lbCamposVacios.Visibility = Visibility.Hidden;

                string nombreSala = tbxNombreSala.Text.ToString();
                string visibilidad = cbVisibilidad.SelectedItem.ToString();

                CrearSala(nombreSala, visibilidad);
            }
            else
            {
                lbCamposVacios.Visibility = Visibility.Visible;
            }
        }

        private void CrearSala(string nombreSala, string visibilidad)
        {
            Sala ventanaSala = new Sala(nombreSala, visibilidad);
            this.Close();
            if (ventanaSala.EstaAbierta)
            {
                ventanaSala.ShowDialog();
            }
        }

        private void RegresarAVentanaAnterior(object sender, MouseButtonEventArgs e)
        {
            MenuPrincipal ventanaMenuPrincipal = new MenuPrincipal();
            this.Close();
            ventanaMenuPrincipal.ShowDialog();
        }
    }
}
