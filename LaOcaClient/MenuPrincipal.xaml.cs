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

        private void CrearSala(object sender, RoutedEventArgs e)
        {

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
            LaOcaService.Sala salaObjetivo = new LaOcaService.Sala();

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
                            MessageBox.Show("Parece que la sala ya está llena.", "Error con la sala", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                    else
                    {
                        MessageBox.Show("No existe una sala con el código ingresado.", "Error con la sala", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    
                }
                catch (FaultException<SalaException> ex)
                {
                    MessageBox.Show(ex.Detail.Mensaje + "\n" + ex.Reason, "Error al buscar la Sala", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                catch (TimeoutException)
                {
                    MessageBox.Show("El servidor ha tardado demasiado en responder.", "Error de conexión", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                catch (CommunicationException)
                {
                    MessageBox.Show("Ha ocurrido un error al intentar conectar con el Servidor. Por favor intente de nuevo más tarde.", "Error de conexión", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void MostrarListaAmigos(object sender, RoutedEventArgs e)
        {
            Social ventanaSocial = new Social();
            this.Close();
            ventanaSocial.ShowDialog();
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
                MessageBox.Show("El servidor ha tardado demasiado en responder.", "Error de conexión", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (CommunicationException)
            {
                MessageBox.Show("Ha ocurrido un error al intentar conectar con el Servidor. Por favor intente de nuevo más tarde.", "Error de conexión", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            this.Close();
        }
    }
}