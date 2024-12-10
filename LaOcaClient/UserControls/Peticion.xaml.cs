using LaOcaClient.LaOcaService;
using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace LaOcaClient.UserControls
{
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

            string rutaFotoPerfil = FotoPerfilUtils.ObtenerRutaFotoPerfil(invitacion.JugadorEmisor.IdFotoPerfil);
            imgFotoPerfil.Source = new BitmapImage(new Uri(rutaFotoPerfil, UriKind.RelativeOrAbsolute));
        }

        public Peticion(Amistad amistad)
        {
            InitializeComponent();

            ServicioJugadorClient clienteJugador = new ServicioJugadorClient();

            Jugador jugadorEmisor = clienteJugador.ObtenerJugadorPorId(amistad.IdJugadorSolicitante);

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
                        MessageBox.Show(Properties.Resources.msgSalaLlena, Properties.Resources.tituloErrorSala, MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            catch (FaultException<SalaException> ex)
            {
                MessageBox.Show(ex.Detail.Mensaje + "\n" + ex.Reason, Properties.Resources.tituloErrorBuscarSala, MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (TimeoutException)
            {
                MessageBox.Show(Properties.Resources.msgTimeoutEx, Properties.Resources.tituloTimeOut, MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (CommunicationException)
            {
                MessageBox.Show(Properties.Resources.msgComunnicationEx, Properties.Resources.globalTituloError, MessageBoxButton.OK, MessageBoxImage.Error);
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
                MessageBox.Show(Properties.Resources.msgTimeoutEx, Properties.Resources.tituloTimeOut, MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (CommunicationException)
            {
                MessageBox.Show(Properties.Resources.msgComunnicationEx, Properties.Resources.globalTituloError, MessageBoxButton.OK, MessageBoxImage.Error);
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
                MessageBox.Show(Properties.Resources.msgTimeoutEx, Properties.Resources.tituloTimeOut, MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (CommunicationException)
            {
                MessageBox.Show(Properties.Resources.msgComunnicationEx, Properties.Resources.globalTituloError, MessageBoxButton.OK, MessageBoxImage.Error);
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
