using LaOcaClient.LaOcaService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace LaOcaClient.UserControls
{
    /// <summary>
    /// Interaction logic for Peticion.xaml
    /// </summary>
    public partial class Peticion : UserControl
    {
        public string codigoSala;
        public Buzon VentanaBuzon;
        private InvitacionPartida _invitacion;
        private Amistad _amistad;

        private bool _esInvitacion;

        public Peticion(InvitacionPartida invitacion)
        {
            InitializeComponent();

            lbMensaje.Content = invitacion.JugadorEmisor.NombreUsuario + " te ha invitado a su partida.";
            this.codigoSala = invitacion.CodigoSalaObjetivo;
            this._invitacion = invitacion;
            _esInvitacion = true;
        }

        public Peticion(Amistad amistad)
        {
            InitializeComponent();

            ServicioCuentaClient clienteCuenta = new ServicioCuentaClient();

            Jugador jugadorEmisor = clienteCuenta.ObtenerJugadorPorId(amistad.IdJugadorSolicitante);

            lbMensaje.Content = jugadorEmisor.NombreUsuario + " quiere ser tu amigo.";
            _amistad = amistad;
            _esInvitacion = false;
        }

        private void UnirseASala()
        {
            try
            {
                LaOcaService.Sala salaObjetivo = new LaOcaService.Sala();

                LaOcaService.ServicioRecuperarSalaClient clienteRecuperarSala = new LaOcaService.ServicioRecuperarSalaClient();
                salaObjetivo = clienteRecuperarSala.RecuperarSala(codigoSala);

                if (salaObjetivo.Jugadores.Count >= 1)
                {
                    if (salaObjetivo.Jugadores.Count <= 3)
                    {
                        ServicioSocialClient clienteSocial = new ServicioSocialClient();
                        clienteSocial.EliminarInvitacionAPartida(SingletonJugador.Instance.Jugador.NombreUsuario, _invitacion);

                        Sala ventanaNuevaSala = new Sala(salaObjetivo);
                        VentanaBuzon.Close();
                        ventanaNuevaSala.ShowDialog();
                    }
                    else
                    {
                        MessageBox.Show("Parece que la sala ya está llena.", "Error con la sala", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
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

        private void ProcesarSolicitudAmistad(string estadoAmistad)
        {
            try
            {
                ServicioAmistadClient clienteAmistad = new ServicioAmistadClient();
                clienteAmistad.ActualizarSolicitudAmistad(_amistad, estadoAmistad);
                VentanaBuzon.lbxPeticiones.Items.Remove(this);

                if (estadoAmistad.Equals(EstadoAmistad.AMIGOS))
                {
                    MessageBox.Show("¡Ahora son amigos!", "Solicitud de amistad aceptada", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show("Has rechazado la solicitud de amistad.", "Solicitud de amistad rechazada", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (FaultException<AmistadException> ex)
            {
                MessageBox.Show(ex.Detail.Mensaje, ex.Reason.ToString(), MessageBoxButton.OK, MessageBoxImage.Error);
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

        private void EliminarInvitacion()
        {
            try
            {
                ServicioSocialClient clienteSocial = new ServicioSocialClient();
                clienteSocial.EliminarInvitacionAPartida(SingletonJugador.Instance.Jugador.NombreUsuario, _invitacion);
                VentanaBuzon.lbxPeticiones.Items.Remove(this);
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

        private void AceptarPeticion(object sender, RoutedEventArgs e)
        {
            if (_esInvitacion)
            {
                UnirseASala();
            }
            else
            {
                ProcesarSolicitudAmistad(EstadoAmistad.AMIGOS);
            }
        }

        private void RechazarPeticion(object sender, RoutedEventArgs e)
        {
            if (_esInvitacion)
            {
                EliminarInvitacion();
            }
            else
            {
                ProcesarSolicitudAmistad(EstadoAmistad.RECHAZADA);
            }
        }
    }
}
