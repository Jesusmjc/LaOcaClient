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
        public Sala ventanaSala;
        public bool EstaAbierta = true;

        private readonly ServicioActualizacionJugadoresEnLineaClient _clienteActualizacionJugadoresEnLinea;

        private Dictionary<string, Amigo> _amigos = new Dictionary<string, Amigo>();

        

        public Social()
        {
            InitializeComponent();

            InstanceContext contexto = new InstanceContext(this);
            _clienteActualizacionJugadoresEnLinea = new ServicioActualizacionJugadoresEnLineaClient(contexto);
   
            try
            {
                MostrarAmigos();
            }
            catch (RegresarAlMenuPrincipalException)
            {
                RedirigirAlMenuPrincipal();
            }
        }

        public Social(Sala ventanaSala)
        {
            InitializeComponent();

            InstanceContext contexto = new InstanceContext(this);
            _clienteActualizacionJugadoresEnLinea = new ServicioActualizacionJugadoresEnLineaClient(contexto);
            
            this.ventanaSala = ventanaSala;
            imgBuzon.Visibility = Visibility.Hidden;

            try
            { 
                MostrarAmigos();
            }
            catch (RegresarAlMenuPrincipalException)
            {
                RedirigirAlMenuPrincipal();
            }
        }

        private void MostrarAmigos()
        {
            try
            {
                _clienteActualizacionJugadoresEnLinea.AgregarCanalCallbackJugadoresEnLinea(SingletonJugador.Instance.Jugador.NombreUsuario);
            }
            catch (TimeoutException)
            {
                MessageBox.Show(Properties.Resources.msgTimeoutEx, Properties.Resources.tituloTimeOut, MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (CommunicationException)
            {
                Utilidad.ManejarCommunicationException(_clienteActualizacionJugadoresEnLinea);
            }

            List<Jugador> amigos = RecuperarAmigos();
            Dictionary<string, Jugador> jugadoresConectados = RecuperarJugadoresConectados();
            List<Jugador> amigosConectados = new List<Jugador>();

            foreach (Jugador amigo in amigos)
            {
                if (ventanaSala != null)
                {
                    if (ventanaSala.SalaActual.Jugadores.ContainsKey(amigo.NombreUsuario))
                    {
                        continue;
                    }
                }

                if (!jugadoresConectados.ContainsKey(amigo.NombreUsuario))
                {
                    if (ventanaSala != null)
                    {
                        Amigo entradaAmigo = new Amigo(amigo, "Desconectado", this);
                        _amigos.Add(amigo.NombreUsuario, entradaAmigo);

                        continue;
                    }

                    MostrarAmigo(amigo, "Desconectado");             
                }
                else
                {
                    amigosConectados.Add(amigo);
                }
            }

            foreach (Jugador amigoConectado in amigosConectados)
            {
                MostrarAmigo(amigoConectado, Properties.Resources.lbEnLinea);
            }
        }

        private void MostrarAmigo(Jugador amigo, string estadoConexion)
        {
            Amigo entradaAmigo = new Amigo(amigo, estadoConexion, this);
            lbxListaAmigos.Items.Insert(0, entradaAmigo);
            _amigos.Add(amigo.NombreUsuario, entradaAmigo);
        }

        private Dictionary<string, Jugador> RecuperarJugadoresConectados()
        {
            Dictionary<string, Jugador> jugadores = new Dictionary<string, Jugador>();

            LaOcaService.ServicioJugadoresEnLineaClient clienteJugadoresEnLinea = new LaOcaService.ServicioJugadoresEnLineaClient();

            try
            {
                
                Jugador[] jugadoresConectados = clienteJugadoresEnLinea.RecuperarJugadoresConectados();

                foreach (Jugador jugador in jugadoresConectados)
                {
                    jugadores.Add(jugador.NombreUsuario, jugador);
                }
            }
            catch (TimeoutException)
            {
                MessageBox.Show(Properties.Resources.msgTimeoutEx, Properties.Resources.tituloTimeOut, MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (CommunicationException)
            {
                Utilidad.ManejarCommunicationException(clienteJugadoresEnLinea);
            }

            return jugadores;
        }

        private List<Jugador> RecuperarAmigos()
        {
            List<Jugador> amigos = new List<Jugador>();
            ServicioAmistadClient clienteAmistad = new ServicioAmistadClient();
            ServicioJugadorClient clienteJugador = new ServicioJugadorClient();
            try
            {
                Amistad[] amistades = clienteAmistad.RecuperarAmistades(SingletonJugador.Instance.Jugador.IdJugador, EstadoAmistad.AMIGOS);

                Jugador jugadorAmigo = new Jugador();

                foreach (Amistad amistad in amistades)
                {
                    if (amistad.IdJugadorSolicitante == SingletonJugador.Instance.Jugador.IdJugador)
                    {
                        jugadorAmigo = clienteJugador.ObtenerJugadorPorId(amistad.IdJugadorReceptor);
                    } 
                    else
                    {
                        jugadorAmigo = clienteJugador.ObtenerJugadorPorId(amistad.IdJugadorSolicitante);
                    }
                    
                    amigos.Add(jugadorAmigo);
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
                Utilidad.ManejarCommunicationException(clienteAmistad);
            }

            return amigos;
        }

        public void MostrarNuevoJugadorConectado(Jugador nuevoJugadorConectado)
        {
            if (_amigos.ContainsKey(nuevoJugadorConectado.NombreUsuario))
            {
                Amigo amigoConectado = _amigos[nuevoJugadorConectado.NombreUsuario];
                lbxListaAmigos.Items.Remove(amigoConectado);

                amigoConectado.estado = Properties.Resources.lbEnLinea;
                amigoConectado.lbEstado.Content = Properties.Resources.lbEnLinea;

                lbxListaAmigos.Items.Insert(0, amigoConectado);
            } 
        }

        public void OcultarJugadorDesconectado(Jugador jugadorDesconectado)
        {
            if (_amigos.ContainsKey(jugadorDesconectado.NombreUsuario))
            {
                Amigo amigoDesconectado = _amigos[jugadorDesconectado.NombreUsuario];
                lbxListaAmigos.Items.Remove(amigoDesconectado);

                if (ventanaSala == null)
                {
                    amigoDesconectado.estado = "Desconectado";
                    amigoDesconectado.lbEstado.Content = "Desconectado";
                    lbxListaAmigos.Items.Add(amigoDesconectado);
                }
            }
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
            if (ventanaBuzon.EstaAbierta)
            {
                ventanaBuzon.ShowDialog();
            }
        }

        public void OcultarJugadorQueTerminoAmistad(int idJugadorQueTerminoAmistad)
        {
            ServicioJugadorClient clienteJugador = new ServicioJugadorClient();

            try
            {
                Jugador exAmigo = clienteJugador.ObtenerJugadorPorId(idJugadorQueTerminoAmistad);

                Amigo entradaExAmigo = _amigos[exAmigo.NombreUsuario];
                lbxListaAmigos.Items.Remove(entradaExAmigo);
                _amigos.Remove(exAmigo.NombreUsuario);
            }
            catch (TimeoutException)
            {
                MessageBox.Show(Properties.Resources.msgTimeoutEx, Properties.Resources.tituloTimeOut, MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (CommunicationException)
            {
                Utilidad.ManejarCommunicationException(clienteJugador);
            }
        }

        public void RedirigirAlMenuPrincipal()
        {
            IniciarSesion ventanaIniciarSesion = new IniciarSesion();
            this.Close();
            EstaAbierta = false;
            ventanaIniciarSesion.Show();
        }
    }
}
