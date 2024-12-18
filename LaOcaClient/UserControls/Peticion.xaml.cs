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

            ServicioJugadorClient clienteJugador = new ServicioJugadorClient();

            try
            {
                Jugador jugadorEmisor = clienteJugador.ObtenerJugadorPorId(amistad.IdJugadorSolicitante);

                lbMensaje.Content = jugadorEmisor.NombreUsuario + Properties.Resources.lbQuiereAmistad;
                _amistad = amistad;
                _esInvitacion = false;

                string rutaFotoPerfil = FotoPerfilUtils.ObtenerRutaFotoPerfil(jugadorEmisor.IdFotoPerfil);
                imgFotoPerfil.Source = new BitmapImage(new Uri(rutaFotoPerfil, UriKind.RelativeOrAbsolute));
            }
            catch (TimeoutException)
            {
                MessageBox.Show(Properties.Resources.msgTimeoutEx, Properties.Resources.tituloTimeOut, MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (CommunicationException)
            {
                try
                {
                    ManejadorExcepciones.ManejarCommunicationException(clienteJugador);
                    MessageBox.Show("No hay internet", Properties.Resources.tituloExcepcionGeneral, MessageBoxButton.OK, MessageBoxImage.Error);
                    clienteJugador = new ServicioJugadorClient();
                }
                catch (RegresarAInicioSesionException)
                {
                    VentanaBuzon.RedirigirAInicioSesion();
                }
            }
        }

        private void UnirseASala()
        {
            LaOcaService.ServicioRecuperarSalaClient clienteRecuperarSala = new LaOcaService.ServicioRecuperarSalaClient();

            try
            {
                LaOcaService.Sala salaObjetivo;

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
                try
                {
                    ManejadorExcepciones.ManejarCommunicationException(clienteRecuperarSala);
                    MessageBox.Show("No hay internet", Properties.Resources.tituloExcepcionGeneral, MessageBoxButton.OK, MessageBoxImage.Error);
                    clienteRecuperarSala = new ServicioRecuperarSalaClient();
                }
                catch (RegresarAInicioSesionException)
                {
                    VentanaBuzon.RedirigirAInicioSesion();
                }
            }
        }

        private void ProcesarSolicitudAmistad(string estadoAmistad)
        {
            ServicioAmistadClient clienteAmistad = new ServicioAmistadClient();

            try
            {    
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
                try
                {
                    ManejadorExcepciones.ManejarCommunicationException(clienteAmistad);
                    MessageBox.Show("No hay internet", Properties.Resources.tituloExcepcionGeneral, MessageBoxButton.OK, MessageBoxImage.Error);
                    clienteAmistad = new ServicioAmistadClient();
                }
                catch (RegresarAInicioSesionException)
                {
                    VentanaBuzon.RedirigirAInicioSesion();
                }
            }
        }

        private void EliminarInvitacion()
        {
            ServicioSocialClient clienteSocial = new ServicioSocialClient();

            try
            {
                clienteSocial.EliminarInvitacionAPartida(SingletonJugador.Instance.Jugador.NombreUsuario, _invitacion);
                VentanaBuzon.lbxPeticiones.Items.Remove(this);
            }
            catch (TimeoutException)
            {
                MessageBox.Show(Properties.Resources.msgTimeoutEx, Properties.Resources.tituloTimeOut, MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (CommunicationException)
            {
                try
                {
                    ManejadorExcepciones.ManejarCommunicationException(clienteSocial);
                    MessageBox.Show("No hay internet", Properties.Resources.tituloExcepcionGeneral, MessageBoxButton.OK, MessageBoxImage.Error);
                    clienteSocial = new ServicioSocialClient();
                }
                catch (RegresarAInicioSesionException)
                {
                    VentanaBuzon.RedirigirAInicioSesion();
                }
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