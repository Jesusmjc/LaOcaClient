using LaOcaClient.LaOcaService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
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

            if (SingletonJugador.Instance.Jugador.EsInvitado)
            {
                MostrarModoInvitado();
            }
        }

        private void BtnModificarCuenta_Click(object sender, RoutedEventArgs e)
        {
            int idCuenta = SingletonJugador.Instance.Jugador.IdCuenta;
            int idJugador = SingletonJugador.Instance.Jugador.IdJugador;
            CrearCuenta ventanaCrearCuenta = new CrearCuenta(ModoCuenta.Modificar, idCuenta, idJugador);
            ventanaCrearCuenta.ActualizarVentanaModificar(ModoCuenta.Modificar);
            ventanaCrearCuenta.Show();
            this.Close();
        }

        private void IrAConfiguracionSala(object sender, RoutedEventArgs e)
        {
            ConfiguracionSala ventanaConfiguracionSala = new ConfiguracionSala();
            this.Close();
            ventanaConfiguracionSala.ShowDialog();
        }

        private void UnirseASala(object sender, RoutedEventArgs e)
        {
            string codigoSalaObjetivo = tbxCodigoSala.Text.ToString();
            LaOcaService.Sala salaObjetivo;

            if (!string.IsNullOrWhiteSpace(codigoSalaObjetivo))
            {
                try
                {
                    LaOcaService.ServicioRecuperarSalaClient clienteRecuperarSala = new LaOcaService.ServicioRecuperarSalaClient();
                    salaObjetivo = clienteRecuperarSala.RecuperarSala(codigoSalaObjetivo);

                    if (salaObjetivo.Jugadores.Count >= 1)
                    {
                        if (salaObjetivo.Jugadores.Count <= 3)
                        {
                            Sala ventanaNuevaSala = new Sala(salaObjetivo);
                            this.Close();
                            ventanaNuevaSala.ShowDialog();
                        }
                        else
                        {
                            MessageBox.Show(Properties.Resources.msgSalaLlena, Properties.Resources.tituloErrorSala, MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                    else
                    {
                        MessageBox.Show(Properties.Resources.msgErrorCodigoSala, Properties.Resources.tituloErrorSala, MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    
                }
                catch (FaultException<SalaException> ex)
                {
                    MessageBox.Show(ex.Detail.Mensaje + "\n" + ex.Reason, Properties.Resources.tituloErrorSala, MessageBoxButton.OK, MessageBoxImage.Error);
                }
                catch (TimeoutException)
                {
                    MessageBox.Show(Properties.Resources.msgTimeoutEx, Properties.Resources.tituloTimeOut, MessageBoxButton.OK, MessageBoxImage.Error);
                }
                catch (CommunicationException)
                {
                    Utilidad.ManejarCommunicationException(this);
                }
            }
        }

        private void MostrarModoInvitado()
        {
            btnModificarCuenta.Visibility = Visibility.Hidden;
            btnCrearSala.Visibility = Visibility.Hidden;
            btnSocial.Visibility = Visibility.Hidden;
            btnRankingGlobal.Visibility = Visibility.Hidden;
            btnEstadisticas.Visibility = Visibility.Hidden;
        }

        private void MostrarListaAmigos(object sender, RoutedEventArgs e)
        {
            Social ventanaSocial = new Social();
            this.Close();
            ventanaSocial.ShowDialog();
        }

        private void BtnVerEstadisticas_Click(object sender, RoutedEventArgs e)
        {
            int idJugador = SingletonJugador.Instance.Jugador.IdJugador;

            var ventanaEstadisticas = new EstadisticasJugador(idJugador);
            ventanaEstadisticas.ShowDialog();
        }

        private void BtnVerRankingGlobal_Click(object sender, RoutedEventArgs e)
        {
            var ventanaRanking = new RankingGlobal();
            ventanaRanking.ShowDialog();
        }

        private void CerrarSesion(object sender, RoutedEventArgs e)
        {
            LaOcaService.ServicioJugadoresEnLineaClient clienteJugadoresEnLinea = new LaOcaService.ServicioJugadoresEnLineaClient();

            try
            {
                clienteJugadoresEnLinea.EliminarJugadorDesconectado(SingletonJugador.Instance.Jugador);
            }
            catch (TimeoutException)
            {
                MessageBox.Show(Properties.Resources.msgTimeoutEx, Properties.Resources.tituloTimeOut, MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (CommunicationException)
            {
                MessageBox.Show(Properties.Resources.msgComunnicationEx, Properties.Resources.globalTituloError, MessageBoxButton.OK, MessageBoxImage.Error);
            }

            IniciarSesion ventanaIniciarSesion = new IniciarSesion();
            this.Close();
            ventanaIniciarSesion.ShowDialog();
        }
    }
}