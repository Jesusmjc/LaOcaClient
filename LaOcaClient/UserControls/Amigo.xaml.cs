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
using System.Windows.Navigation;
using System.Windows.Shapes;
using LaOcaClient.LaOcaService;

namespace LaOcaClient.UserControls
{
    /// <summary>
    /// Interaction logic for Amigo.xaml
    /// </summary>
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

            MenuItem opcionEliminarAmigo = new MenuItem { Header = "Eliminar Amigo" };
            opcionEliminarAmigo.Click += (s, args) => EliminarAmigo();
            imgMasOpciones_MenuContextual.Items.Add(opcionEliminarAmigo);

            if (VentanaSocial.ventanaSala != null)
            {
                MenuItem opcionInvitarAPartida = new MenuItem { Header = "Invitar a Partida" };
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
                        MessageBox.Show("Se ha enviado la solicitud.", "Invitación enviada", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        MessageBox.Show("Parece que ya has enviado una invitación a este jugador.", "No se pudo enviar la invitación", MessageBoxButton.OK, MessageBoxImage.Error);
                    }

                }
            }
            catch (FaultException<SalaException>)
            {
                MessageBox.Show("Parece que el host ha abandonado la sala.", "Regresarás al Menú Principal", MessageBoxButton.OK, MessageBoxImage.Error);

                MenuPrincipal ventanaMenuPrincipal = new MenuPrincipal();
                VentanaSocial.Close();
                ventanaMenuPrincipal.ShowDialog();
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

        private void EliminarAmigo()
        {

        }
    }
}
