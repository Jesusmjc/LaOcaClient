using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Security;
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
using LaOcaClient.LaOcaService;
using LaOcaClient.UserControls;

namespace LaOcaClient
{
    public partial class Buzon : Window, IServicioBuzonCallback
    {
        public bool EstaAbierta = true;

        private LaOcaService.ServicioSocialClient _clienteSocial;
        private IniciarSesion _iniciarSesion = new IniciarSesion();

        public Buzon()
        {
            InitializeComponent();
            btnInvitaciones.IsEnabled = false;
            _clienteSocial = new LaOcaService.ServicioSocialClient();

            try
            {
                MostrarInvitacionesPendientes();
            }
            catch (RegresarAlMenuPrincipalException)
            {
                RedirigirAInicioSesion();
            }
        }

        public void MostrarNuevaInvitacionAPartida(InvitacionPartida invitacion)
        {
            Peticion peticion = new Peticion(invitacion)
            {
                VentanaBuzon = this
            };

            lbxPeticiones.Items.Insert(0, peticion);
        }

        private void MostrarInvitacionesPendientes()
        {
            InstanceContext contexto = new InstanceContext(this);
            LaOcaService.ServicioBuzonClient clienteBuzon = new LaOcaService.ServicioBuzonClient(contexto);

            try
            {
                clienteBuzon.AgregarCanalCallbackBuzon(SingletonJugador.Instance.Jugador.NombreUsuario);

                List<InvitacionPartida> invitaciones = RecuperarInvitacionesDelServidor();

                foreach (var invitacion in invitaciones)
                {
                    Peticion peticion = new Peticion(invitacion);
                    peticion.VentanaBuzon = this;

                    lbxPeticiones.Items.Add(peticion);
                }
            }
            catch (TimeoutException)
            {
                MessageBox.Show(Properties.Resources.msgTimeoutEx, Properties.Resources.tituloTimeOut, MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (CommunicationException)
            {
                Utilidad.ManejarCommunicationException(clienteBuzon);
            }
        }

        private List<InvitacionPartida> RecuperarInvitacionesDelServidor()
        {
            List<InvitacionPartida> invitaciones = new List<InvitacionPartida>();

            try
            {
                InvitacionPartida[] invitacionesDelServidor = _clienteSocial.RecuperarInvitaciones(SingletonJugador.Instance.Jugador.NombreUsuario);

                foreach (InvitacionPartida invitacion in invitacionesDelServidor)
                {
                    invitaciones.Add(invitacion);
                }
            }
            catch (FaultException<AmistadException> ex)
            {
                MessageBox.Show(ex.Detail.Mensaje, ex.Reason.ToString(), MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (TimeoutException)
            {
                MessageBox.Show(Properties.Resources.msgTimeoutEx, Properties.Resources.tituloTimeOut, MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (CommunicationException)
            {
                Utilidad.ManejarCommunicationException(_clienteSocial);
            }

            return invitaciones;
        }

        private void RegresarASocial(object sender, MouseButtonEventArgs e)
        {
            Social ventanaSocial = new Social();
            this.Close();
            if (ventanaSocial.EstaAbierta)
            {
                ventanaSocial.ShowDialog();
            } 
        }

        private void MostrarSolicitudesAmistad(object sender, RoutedEventArgs e)
        {
            btnSolicitudes.IsEnabled = false;
            btnInvitaciones.IsEnabled = true;
            lbxPeticiones.Items.Clear();

            ServicioAmistadClient clienteAmistad = new ServicioAmistadClient();
            try
            {
                Amistad[] solicitudesDeAmistadDeJugador = clienteAmistad.RecuperarAmistades(SingletonJugador.Instance.Jugador.IdJugador, EstadoAmistad.SOLICITUD);

                foreach (Amistad solicitudAmistad in solicitudesDeAmistadDeJugador)
                {
                    Peticion peticion = new Peticion(solicitudAmistad);
                    peticion.VentanaBuzon = this;

                    lbxPeticiones.Items.Add(peticion);
                }
            }
            catch (FaultException<AmistadException> ex)
            {
                MessageBox.Show(ex.Detail.Mensaje, ex.Reason.ToString(), MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (TimeoutException)
            {
                MessageBox.Show(Properties.Resources.msgTimeoutEx, Properties.Resources.tituloTimeOut, MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (CommunicationException)
            {
                try
                {
                    Utilidad.ManejarCommunicationException(clienteAmistad);
                }
                catch (RegresarAlMenuPrincipalException)
                {
                    RedirigirAInicioSesion();
                }
            }
        }

        private void MostrarInvitacionesAPartida(object sender, RoutedEventArgs e)
        {
            btnInvitaciones.IsEnabled = false;
            btnSolicitudes.IsEnabled = true;
            lbxPeticiones.Items.Clear();

            try
            {
                MostrarInvitacionesPendientes();
            }
            catch (RegresarAlMenuPrincipalException)
            {
                RedirigirAInicioSesion();
            }
        }

        public void RedirigirAInicioSesion()
        {
            IniciarSesion ventanaIniciarSesion = new IniciarSesion();
            this.Close();
            EstaAbierta = false;
            ventanaIniciarSesion.Show();
        }
    }
}