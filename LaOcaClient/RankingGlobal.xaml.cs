using LaOcaClient.LaOcaService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace LaOcaClient
{
    public partial class RankingGlobal : Window
    {
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
            catch (Exception)
            {
                MessageBox.Show(Properties.Resources.msgErrorRanking, Properties.Resources.globalErrorValidacion, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnCerrar(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}