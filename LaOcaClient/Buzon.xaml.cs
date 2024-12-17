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
        private LaOcaService.ServicioSocialClient _clienteSocial;
        private IniciarSesion _iniciarSesion = new IniciarSesion();

        public Buzon()
        {
            InitializeComponent();
            btnInvitaciones.IsEnabled = false;
            _clienteSocial = new LaOcaService.ServicioSocialClient();
            InstanceContext contexto = new InstanceContext(this);
            LaOcaService.ServicioBuzonClient clienteBuzon = new LaOcaService.ServicioBuzonClient(contexto);

            try
            {
                clienteBuzon.AgregarCanalCallbackBuzon(SingletonJugador.Instance.Jugador.NombreUsuario);
                MostrarInvitacionesPendientes();
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
            List<InvitacionPartida> invitaciones = RecuperarInvitacionesDelServidor();

            foreach (var invitacion in invitaciones)
            {
                Peticion peticion = new Peticion(invitacion);
                peticion.VentanaBuzon = this;

                lbxPeticiones.Items.Add(peticion);
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
                MessageBox.Show(Properties.Resources.msgComunnicationEx, Properties.Resources.globalTituloError, MessageBoxButton.OK, MessageBoxImage.Error);
                _iniciarSesion.Show();
                this.Close();
            }
            catch (Exception)
            {
                MessageBox.Show(Properties.Resources.msgExcepcionGeneral, Properties.Resources.tituloExcepcionGeneral, MessageBoxButton.OK, MessageBoxImage.Error);
            }

            return invitaciones;
        }

        private void RegresarASocial(object sender, MouseButtonEventArgs e)
        {
            Social ventanaSocial = new Social();
            this.Close();
            ventanaSocial.ShowDialog();
        }

        private void MostrarSolicitudesAmistad(object sender, RoutedEventArgs e)
        {
            btnSolicitudes.IsEnabled = false;
            btnInvitaciones.IsEnabled = true;
            lbxPeticiones.Items.Clear();

            try
            {
                ServicioAmistadClient clienteAmistad = new ServicioAmistadClient();
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
                MessageBox.Show(Properties.Resources.msgComunnicationEx, Properties.Resources.globalTituloError, MessageBoxButton.OK, MessageBoxImage.Error);
                _iniciarSesion.Show();
                this.Close();
            }
            catch (Exception)
            {
                MessageBox.Show(Properties.Resources.msgExcepcionGeneral, Properties.Resources.tituloExcepcionGeneral, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void MostrarInvitacionesAPartida(object sender, RoutedEventArgs e)
        {
            btnInvitaciones.IsEnabled = false;
            btnSolicitudes.IsEnabled = true;
            lbxPeticiones.Items.Clear();
            MostrarInvitacionesPendientes();
        }
    }
}