using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.ServiceModel.Security;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using LaOcaClient.LaOcaService;

namespace LaOcaClient.UserControls
{
    public partial class Amigo : UserControl
    {
        public Jugador JugadorAmigo { get; set; }
        public string Estado { get; set; }
        public Social VentanaSocial { get; set; }

        public Amigo(Jugador amigo, string estado, Social ventanaSocial)
        {
            InitializeComponent();

            JugadorAmigo = amigo;
            Estado = estado;
            VentanaSocial = ventanaSocial;

            lbNombreAmigo.Content = JugadorAmigo.NombreUsuario;
            lbEstado.Content = Estado;

            string rutaFotoPerfil = FotoPerfilUtils.ObtenerRutaFotoPerfil(JugadorAmigo.IdFotoPerfil);
            imgFotoPerfil.Source = new BitmapImage(new Uri(rutaFotoPerfil, UriKind.RelativeOrAbsolute));

            MenuItem opcionEliminarAmigo = new MenuItem { Header = Properties.Resources.lbEliminarAmigo };
            opcionEliminarAmigo.Click += (s, args) => EliminarAmigo();
            imgMasOpciones_MenuContextual.Items.Add(opcionEliminarAmigo);

            if (VentanaSocial.VentanaSala != null)
            {
                MenuItem opcionInvitarAPartida = new MenuItem { Header = Properties.Resources.lbInvitarAPartida };
                opcionInvitarAPartida.Click += (s, args) => EnviarInvitacionAPartida();
                imgMasOpciones_MenuContextual.Items.Add(opcionInvitarAPartida);
            }
        }

        private void MostrarMenúPopup(object sender, MouseButtonEventArgs e)
        {
            imgMasOpciones_MenuContextual.IsOpen = true;
        }

        private void EnviarInvitacionAPartida()
        {
            LaOcaService.ServicioRecuperarSalaClient clienteSala = new LaOcaService.ServicioRecuperarSalaClient();

            try
            {
                LaOcaService.Sala salaActual = clienteSala.RecuperarSala(VentanaSocial.VentanaSala.SalaActual.Codigo);

                if (salaActual != null)
                {
                    LaOcaService.ServicioSocialClient clienteSocial = new LaOcaService.ServicioSocialClient();
                    bool resultado = clienteSocial.EnviarInvitacionAPartida(JugadorAmigo.NombreUsuario, SingletonJugador.Instance.Jugador, VentanaSocial.VentanaSala.SalaActual.Codigo);

                    if (resultado)
                    {
                        MessageBox.Show(Properties.Resources.msgInvitacionEnviada, Properties.Resources.tituloInvitacionEnviada, MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        MessageBox.Show(Properties.Resources.msgInvitacionYaEnviada, Properties.Resources.tituloInvitacionNoEnviada, MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            catch (FaultException<SalaException>)
            {
                MessageBox.Show(Properties.Resources.msgAbandonoHost, Properties.Resources.tituloRegresarAlMenu, MessageBoxButton.OK, MessageBoxImage.Error);
                MenuPrincipal ventanaMenuPrincipal = new MenuPrincipal();
                VentanaSocial.Close();
                ventanaMenuPrincipal.ShowDialog();
            }
            catch (TimeoutException)
            {
                MessageBox.Show(Properties.Resources.msgTimeoutEx, Properties.Resources.tituloTimeOut, MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (CommunicationException)
            {
                try
                {
                    ManejadorExcepciones.ManejarCommunicationException(clienteSala);
                    MessageBox.Show("No hay internet", Properties.Resources.tituloExcepcionGeneral, MessageBoxButton.OK, MessageBoxImage.Error);
                    clienteSala = new ServicioRecuperarSalaClient();
                }
                catch (RegresarAInicioSesionException)
                {
                    VentanaSocial.RedirigirAInicioSesion();
                }
            }
        }

        private void EliminarAmigo()
        {
            ServicioAmistadClient clienteAmistad = new ServicioAmistadClient();
            
            try
            {    
                Amistad amistad = clienteAmistad.RecuperarAmistad(SingletonJugador.Instance.Jugador.IdJugador, JugadorAmigo.IdJugador);

                amistad.IdJugadorSolicitante = SingletonJugador.Instance.Jugador.IdJugador;
                amistad.IdJugadorReceptor = JugadorAmigo.IdJugador;

                clienteAmistad.ActualizarSolicitudAmistad(amistad, EstadoAmistad.RECHAZADA);

                VentanaSocial.lbxListaAmigos.Items.Remove(this);
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
                    VentanaSocial.RedirigirAInicioSesion();
                }
            }
        }
    }
}
