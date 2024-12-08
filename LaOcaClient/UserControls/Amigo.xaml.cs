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
        public Jugador amigo;
        public string estado;

        public Social VentanaSocial { get; set; }

        public Amigo(Jugador amigo, string estado, Social ventanaSocial)
        {
            InitializeComponent();

            this.amigo = amigo;
            this.estado = estado;
            this.VentanaSocial = ventanaSocial;

            lbNombreAmigo.Content = amigo.NombreUsuario;
            lbEstado.Content = estado;

            string rutaFotoPerfil = FotoPerfilUtils.ObtenerRutaFotoPerfil(amigo.IdFotoPerfil);
            imgFotoPerfil.Source = new BitmapImage(new Uri(rutaFotoPerfil, UriKind.RelativeOrAbsolute));

            MenuItem opcionEliminarAmigo = new MenuItem { Header = Properties.Resources.lbEliminarAmigo };
            opcionEliminarAmigo.Click += (s, args) => EliminarAmigo();
            imgMasOpciones_MenuContextual.Items.Add(opcionEliminarAmigo);

            if (VentanaSocial.ventanaSala != null)
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
            try
            {
                LaOcaService.ServicioRecuperarSalaClient clienteSala = new LaOcaService.ServicioRecuperarSalaClient();
                LaOcaService.Sala salaActual = clienteSala.RecuperarSala(VentanaSocial.ventanaSala.sala.Codigo);


                if (salaActual != null)
                {
                    LaOcaService.ServicioSocialClient clienteSocial = new LaOcaService.ServicioSocialClient();
                    bool resultado = clienteSocial.EnviarInvitacionAPartida(amigo.NombreUsuario, SingletonJugador.Instance.Jugador, VentanaSocial.ventanaSala.sala.Codigo);

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
                MessageBox.Show(Properties.Resources.msgComunnicationEx, Properties.Resources.globalTituloError, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void EliminarAmigo()
        {
            // Lógica para eliminar amigo
        }
    }
}
