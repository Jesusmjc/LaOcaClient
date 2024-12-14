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
        public string CodigoSala { get; set; }

        public Buzon VentanaBuzon { get; set; }

        private InvitacionPartida _invitacion;
        private Amistad _amistad;
        private bool _esInvitacion;

        public Peticion(InvitacionPartida invitacion)
        {
            InitializeComponent();

            lbMensaje.Content = invitacion.JugadorEmisor.NombreUsuario + Properties.Resources.lbTeInvitoAPartida;
            this.CodigoSala = invitacion.CodigoSalaObjetivo;
            this._invitacion = invitacion;
            _esInvitacion = true;

            string rutaFotoPerfil = FotoPerfilUtils.ObtenerRutaFotoPerfil(invitacion.JugadorEmisor.IdFotoPerfil);
            imgFotoPerfil.Source = new BitmapImage(new Uri(rutaFotoPerfil, UriKind.RelativeOrAbsolute));
        }

        public Peticion(Amistad amistad)
        {
            InitializeComponent();

            ServicioJugadorClient clienteCuenta = new ServicioJugadorClient();

            Jugador jugadorEmisor = clienteCuenta.ObtenerJugadorPorId(amistad.IdJugadorSolicitante);

            lbMensaje.Content = jugadorEmisor.NombreUsuario + Properties.Resources.lbQuiereAmistad;
            _amistad = amistad;
            _esInvitacion = false;

            string rutaFotoPerfil = FotoPerfilUtils.ObtenerRutaFotoPerfil(jugadorEmisor.IdFotoPerfil);
            imgFotoPerfil.Source = new BitmapImage(new Uri(rutaFotoPerfil, UriKind.RelativeOrAbsolute));
        }

        private void UnirseASala()
        {
            try
            {
                LaOcaService.Sala salaObjetivo;

                LaOcaService.ServicioRecuperarSalaClient clienteRecuperarSala = new LaOcaService.ServicioRecuperarSalaClient();
                salaObjetivo = clienteRecuperarSala.RecuperarSala(CodigoSala);

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
                    MessageBox.Show(Properties.Resources.msgAhoraSonAmigos, Properties.Resources.tituloSolicitudAceptada, MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show(Properties.Resources.msgSolicitudRechazada, Properties.Resources.tituloSolicitudRechazada, MessageBoxButton.OK, MessageBoxImage.Information);
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