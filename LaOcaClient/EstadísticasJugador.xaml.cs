using LaOcaClient.LaOcaService;
using System;
using System.Windows;

namespace LaOcaClient
{
    public partial class EstadisticasJugador : Window
    {
        private int idJugador;

        public EstadisticasJugador(int idJugador)
        {
            InitializeComponent();
            this.idJugador = idJugador;
            CargarEstadisticas();
        }

        private void CargarEstadisticas()
        {
            try
            {
                var clienteServicio = new ServicioCuentaClient();
                string estadisticas = clienteServicio.ConsultarEstadisticasJugador(idJugador);
                var estadisticasArray = estadisticas.Split(',');

                string casillasRecorridas = Properties.Resources.txtCasillasRecorridas;
                string partidasGanadas = Properties.Resources.txtPartidasGanadas;

                txtCasillasRecorridas.Text = casillasRecorridas + estadisticasArray[0].Trim();
                txtPartidasGanadas.Text = partidasGanadas + estadisticasArray[1].Trim();
            }
            catch (Exception)
            {
                MessageBox.Show(Properties.Resources.msgErrorEstadisticas, Properties.Resources.globalErrorValidacion, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnCerrar_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
