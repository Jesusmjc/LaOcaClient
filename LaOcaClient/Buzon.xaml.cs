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
using System.Windows.Shapes;
using LaOcaClient.LaOcaService;
using LaOcaClient.UserControls;

namespace LaOcaClient
{
    public partial class Buzon : Window, IServicioBuzonCallback
    {
        private LaOcaService.ServicioSocialClient clienteSocial;

        public Buzon()
        {
            InitializeComponent();

            clienteSocial = new LaOcaService.ServicioSocialClient();

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
            }

        }

        public void MostrarNuevaInvitacionAPartida(InvitacionPartida invitacion)
        {
            Peticion peticion = new Peticion(invitacion);
            peticion.VentanaBuzon = this;

            lbxPeticiones.Items.Insert(0, peticion);
        }

        public void MostrarNuevaSolicitudAmistad(Amistad solicitudAmistad)
        {
            throw new NotImplementedException();
        }

        private void MostrarInvitacionesPendientes()
        {
            try
            {
                InvitacionPartida[] invitaciones = clienteSocial.RecuperarInvitaciones(SingletonJugador.Instance.Jugador.NombreUsuario);

                if (invitaciones != null)
                {
                    foreach (var invitacion in invitaciones)
                    {
                        Peticion peticion = new Peticion(invitacion);
                        peticion.VentanaBuzon = this;

                        lbxPeticiones.Items.Add(peticion);
                    }
                }
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

        private void RegresarASocial(object sender, MouseButtonEventArgs e)
        {
            Social ventanaSocial = new Social();
            this.Close();
            ventanaSocial.ShowDialog();
        }
    }
}
