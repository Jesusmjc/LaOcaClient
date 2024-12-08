using LaOcaClient.LaOcaService;
using LaOcaClient.UserControls;
using System;
using System.CodeDom;
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

namespace LaOcaClient
{
    /// <summary>
    /// Interaction logic for Social.xaml
    /// </summary>
    public partial class Social : Window, IServicioActualizacionJugadoresEnLineaCallback
    {
        private LaOcaService.ServicioActualizacionJugadoresEnLineaClient clienteActualizacionJugadoresEnLinea;
        private Dictionary<string, Amigo> amigos = new Dictionary<string, Amigo>();

        public Sala ventanaSala;

        public Social()
        {
            InitializeComponent();

            InstanceContext contexto = new InstanceContext(this);
            clienteActualizacionJugadoresEnLinea = new ServicioActualizacionJugadoresEnLineaClient(contexto);

            try
            {
                clienteActualizacionJugadoresEnLinea.AgregarCanalCallbackJugadoresEnLinea(SingletonJugador.Instance.Jugador.NombreUsuario);
            }
            catch (TimeoutException)
            {
                MessageBox.Show(Properties.Resources.msgTimeoutEx, Properties.Resources.tituloTimeOut, MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (CommunicationException)
            {
                MessageBox.Show(Properties.Resources.msgComunnicationEx, Properties.Resources.globalTituloError, MessageBoxButton.OK, MessageBoxImage.Error);
            }

            MostrarJugadoresConectados();
        }

        public Social(Sala ventanaSala)
        {
            InitializeComponent();

            InstanceContext contexto = new InstanceContext(this);
            clienteActualizacionJugadoresEnLinea = new ServicioActualizacionJugadoresEnLineaClient(contexto);

            try
            {
                clienteActualizacionJugadoresEnLinea.AgregarCanalCallbackJugadoresEnLinea(SingletonJugador.Instance.Jugador.NombreUsuario);

                this.ventanaSala = ventanaSala;

                MostrarJugadoresConectados();
                imgBuzon.Visibility = Visibility.Hidden;
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

        private void MostrarJugadoresConectados()
        {
            LaOcaService.ServicioJugadoresEnLineaClient clienteJugadoresEnLinea = new LaOcaService.ServicioJugadoresEnLineaClient();
            try
            {
                Jugador[] jugadoresConectados = clienteJugadoresEnLinea.RecuperarJugadoresConectados();

                foreach (Jugador jugador in jugadoresConectados)
                {
                    if (!jugador.NombreUsuario.Equals(SingletonJugador.Instance.Jugador.NombreUsuario))
                    {
                        Amigo entradaAmigo = new Amigo(jugador, Properties.Resources.lbEnLinea, this);
                        lbxListaAmigos.Items.Add(entradaAmigo);
                        amigos.Add(jugador.NombreUsuario, entradaAmigo);
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

        public void MostrarNuevoJugadorConectado(Jugador nuevoJugadorConectado)
        {
            Amigo entradaNuevoAmigoConectado = new Amigo(nuevoJugadorConectado, Properties.Resources.lbEnLinea, this);
            lbxListaAmigos.Items.Insert(0, entradaNuevoAmigoConectado);
            amigos.Add(nuevoJugadorConectado.NombreUsuario, entradaNuevoAmigoConectado);
        }

        public void OcultarJugadorDesconectado(Jugador jugadorDesconectado)
        {
            lbxListaAmigos.Items.Remove(amigos[jugadorDesconectado.NombreUsuario]);
            amigos.Remove(jugadorDesconectado.NombreUsuario);
        }

        private void RegresarAVentanaAnterior(object sender, MouseButtonEventArgs e)
        {
            if (ventanaSala != null)
            {
                this.Close();
            }
            else
            {
                RegresarAMenúPrincipal();
            }
        }

        private void RegresarAMenúPrincipal()
        {
            MenuPrincipal ventanaMenuPrincipal = new MenuPrincipal();
            this.Close();
            ventanaMenuPrincipal.ShowDialog();
        }

        private void MostrarBuzon(object sender, MouseButtonEventArgs e)
        {
            Buzon ventanaBuzon = new Buzon();
            this.Close();
            ventanaBuzon.ShowDialog();
        }
    }
}
