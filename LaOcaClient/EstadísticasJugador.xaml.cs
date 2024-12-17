using LaOcaClient.LaOcaService;
using System;
using System.ServiceModel;
using System.Windows;

namespace LaOcaClient
{
    public partial class EstadisticasJugador : Window
    {
        private int _idJugador;
        private IniciarSesion _iniciarSesion = new IniciarSesion();

        public EstadisticasJugador(int idJugador)
        {
            InitializeComponent();
            this._idJugador = idJugador;
            CargarEstadisticas();
        }

        private void CargarEstadisticas()
        {
            try
            {
                var clienteServicio = new ServicioJugadorClient();
                string estadisticas = clienteServicio.ConsultarEstadisticasJugador(_idJugador);
                var estadisticasArray = estadisticas.Split(',');
                string casillasRecorridas = Properties.Resources.txtCasillasRecorridas;
                string partidasGanadas = Properties.Resources.txtPartidasGanadas;
                tbckCasillasRecorridas.Text = casillasRecorridas + estadisticasArray[0].Trim();
                tbckPartidasGanadas.Text = partidasGanadas + estadisticasArray[1].Trim();
            }
            catch (TimeoutException)
            {
                MessageBox.Show(Properties.Resources.msgTimeoutEx, Properties.Resources.tituloTimeOut, MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (CommunicationException)
            {
                MessageBox.Show(Properties.Resources.msgComunnicationEx, Properties.Resources.globalTituloError, MessageBoxButton.OK, MessageBoxImage.Error);
                _iniciarSesion.Show();
                this.Close();
            }
            catch (Exception)
            {
                MessageBox.Show(Properties.Resources.msgExcepcionGeneral, Properties.Resources.tituloExcepcionGeneral, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnCerrar(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}