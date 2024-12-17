using LaOcaClient.LaOcaService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Windows;

namespace LaOcaClient
{
    public partial class RankingGlobal : Window
    {
        private IniciarSesion _ventanaIniciarSesion = new IniciarSesion();

        public RankingGlobal()
        {
            InitializeComponent();
            CargarRankingGlobal();
        }

        private void CargarRankingGlobal()
        {
            try
            {
                var clienteServicio = new ServicioCuentaClient();
                var ranking = clienteServicio.ObtenerRankingGlobal();

                var datosRanking = ranking.Select((jugador, index) => new
                {
                    Posicion = index + 1,
                    jugador.NombreUsuario,
                    jugador.CasillasRecorridas,
                    jugador.PartidasGanadas
                }).Take(10).ToList();

                dgRanking.ItemsSource = datosRanking;
            }
            catch (FaultException)
            {
                MessageBox.Show(Properties.Resources.globalErrorBD, Properties.Resources.tituloExcepcionGeneral, MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (TimeoutException)
            {
                MessageBox.Show(Properties.Resources.msgTimeoutEx, Properties.Resources.tituloTimeOut, MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (CommunicationException)
            {
                MessageBox.Show(Properties.Resources.msgComunnicationEx, Properties.Resources.globalTituloError, MessageBoxButton.OK, MessageBoxImage.Error);
                _ventanaIniciarSesion.Show();
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