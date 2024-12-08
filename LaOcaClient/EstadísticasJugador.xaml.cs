using LaOcaClient.LaOcaService;
using System;
using System.Windows;

namespace LaOcaClient
{
    /// <summary>
    /// Lógica de interacción para EstadísticasJugador.xaml
    /// </summary>
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

                txtCasillasRecorridas.Text = estadisticasArray[0].Trim();
                txtPartidasGanadas.Text = estadisticasArray[1].Trim();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al consultar estadísticas: {ex.Message}", "Error");
            }
        }

        private void BtnCerrar_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
