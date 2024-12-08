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
        private InvitacionPartida invitacion;

        public Peticion(InvitacionPartida invitacion)
        {
            InitializeComponent();

            this.invitacion = invitacion;
            this.codigoSala = invitacion.CodigoSalaObjetivo;

            lbMensaje.Content = invitacion.JugadorEmisor.NombreUsuario;

            string rutaFotoPerfil = FotoPerfilUtils.ObtenerRutaFotoPerfil(invitacion.JugadorEmisor.IdFotoPerfil);
            imgFotoPerfil.Source = new BitmapImage(new Uri(rutaFotoPerfil, UriKind.RelativeOrAbsolute));
        }

        private void UnirseASala(object sender, RoutedEventArgs e)
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
                        clienteSocial.EliminarInvitacionAPartida(SingletonJugador.Instance.Jugador.NombreUsuario, invitacion);

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

        private void EliminarPeticion(object sender, RoutedEventArgs e)
        {
            try
            {
                ServicioSocialClient clienteSocial = new ServicioSocialClient();
                clienteSocial.EliminarInvitacionAPartida(SingletonJugador.Instance.Jugador.NombreUsuario, invitacion);
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
    }
}
