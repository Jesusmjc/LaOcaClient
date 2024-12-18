using LaOcaClient.LaOcaService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
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

namespace LaOcaClient.UserControls
{
    public partial class JugadorEnSala : UserControl
    {
        public Jugador JugadorEnLaSala { get; set; }

        public Sala _ventanaPadre;

        private ServicioAmistadClient _clienteAmistad;
        private Amistad _amistad;

        public JugadorEnSala(Jugador jugadorEnSala, Sala ventanaSala)
        {
            InitializeComponent();

            lbNombreJugador.Content = jugadorEnSala.NombreUsuario;
            this.JugadorEnLaSala = jugadorEnSala;

            _clienteAmistad = new ServicioAmistadClient();
            _ventanaPadre = ventanaSala;

            string rutaFotoPerfil = FotoPerfilUtils.ObtenerRutaFotoPerfil(jugadorEnSala.IdFotoPerfil);
            imgFotoPerfil.Source = new BitmapImage(new Uri(rutaFotoPerfil, UriKind.RelativeOrAbsolute));

            MostrarImagenMasOpciones();
        }

        public JugadorEnSala(Jugador jugadorEnSala)
        {
            InitializeComponent();

            lbNombreJugador.Content = jugadorEnSala.NombreUsuario;
            this.JugadorEnLaSala = jugadorEnSala;

            _clienteAmistad = new ServicioAmistadClient();

            string rutaFotoPerfil = FotoPerfilUtils.ObtenerRutaFotoPerfil(jugadorEnSala.IdFotoPerfil);
            imgFotoPerfil.Source = new BitmapImage(new Uri(rutaFotoPerfil, UriKind.RelativeOrAbsolute));

            MostrarImagenMasOpciones();
        }

        private void MostrarImagenMasOpciones()
        {
            if (!(_ventanaPadre is Sala))
            {
                imgMasOpciones.Visibility = Visibility.Hidden;
                return;
            }

            bool soyHost = SingletonJugador.Instance.Jugador.NombreUsuario.Equals(_ventanaPadre.SalaActual.NombreHost);
            bool soyYo = SingletonJugador.Instance.Jugador.Equals(JugadorEnLaSala);

            if (!SingletonJugador.Instance.Jugador.EsInvitado)
            {
                if (soyYo)
                {
                    imgMasOpciones.Visibility = Visibility.Hidden;
                }
                else if (!JugadorEnLaSala.EsInvitado)
                {
                    AjustarMenuPopupSegunAmistad();
                }
                else
                {
                    if (soyHost)
                    {
                        CargarOpcionExpulsar();
                    }
                    else
                    {
                        imgMasOpciones.Visibility = Visibility.Hidden;
                    }
                }
            }
            else
            {
                imgMasOpciones.Visibility = Visibility.Hidden;
            }
        }

        public void AjustarMenuPopupSegunAmistad()
        {
            try
            {
                _amistad = _clienteAmistad.RecuperarAmistad(SingletonJugador.Instance.Jugador.IdJugador, JugadorEnLaSala.IdJugador);
                CargarMenuPopup();
            }
            catch (FaultException<AmistadException> ex)
            {
                MessageBox.Show(ex.Detail.Mensaje + "\n" + ex.Reason, Properties.Resources.tituloInvitacionNoEnviada, MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (TimeoutException)
            {
                MessageBox.Show(Properties.Resources.msgTimeoutEx, Properties.Resources.tituloTimeOut, MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (CommunicationException)
            {
                try
                {
                    Utilidad.ManejarCommunicationException(_clienteAmistad);
                }
                catch (RegresarAInicioSesionException)
                {
                    _ventanaPadre.RedirigirAInicioSesion();
                }
            }
        }

        private void CargarMenuPopup()
        {
            imgMasOpciones_MenuContextual.Items.Clear();

            switch (_amistad.Estado)
            {
                case "Solicitud":
                case "Amigos":
                    CargarOpcionBloquear();
                    break;

                case "Bloqueo":
                    if (_amistad.IdJugadorReceptor != SingletonJugador.Instance.Jugador.IdJugador)
                    {
                        CargarOpcionDesbloquear();
                    }
                    break;

                default:
                    CargarOpcionEnviarSolicitudAmistad();
                    CargarOpcionBloquear();
                    break;
            }

            if (SingletonJugador.Instance.Jugador.NombreUsuario.Equals(_ventanaPadre.SalaActual.NombreHost))
            {
                CargarOpcionExpulsar();
            }
        }

        private void CargarOpcionEnviarSolicitudAmistad()
        {
            MenuItem opcionEnviarSolicitudAmistad = new MenuItem { Header = Properties.Resources.lbEnviarSolicitudAmistad };
            opcionEnviarSolicitudAmistad.Click += (s, args) => EnviarSolicitudAmistad();
            imgMasOpciones_MenuContextual.Items.Add(opcionEnviarSolicitudAmistad);
        }

        private void CargarOpcionBloquear()
        {
            MenuItem opcionBloquearJugador = new MenuItem { Header = Properties.Resources.lbBloquear };
            opcionBloquearJugador.Click += (s, args) => BloquearJugador();
            imgMasOpciones_MenuContextual.Items.Add(opcionBloquearJugador);
        }

        private void CargarOpcionDesbloquear()
        {
            MenuItem opcionDesbloquearJugador = new MenuItem { Header = Properties.Resources.lbDesbloquear };
            opcionDesbloquearJugador.Click += (s, args) => DesbloquearJugador();
            imgMasOpciones_MenuContextual.Items.Add(opcionDesbloquearJugador);
        }

        public void CargarOpcionExpulsar()
        {
            MenuItem opcionExpulsar = new MenuItem { Header = Properties.Resources.lbExpulsar };
            opcionExpulsar.Click += (s, args) => ExpulsarJugador();
            imgMasOpciones_MenuContextual.Items.Add(opcionExpulsar);
        }

        private void MostrarMenuPopup(object sender, MouseButtonEventArgs e)
        {
            bool soyHost = SingletonJugador.Instance.Jugador.NombreUsuario.Equals(_ventanaPadre.SalaActual.NombreHost);
            bool estoyBloqueado = false;
            if (_amistad != null && _amistad.IdAmistad > 0)
            {
                estoyBloqueado = _amistad.Estado.Equals(EstadoAmistad.BLOQUEO) && _amistad.IdJugadorReceptor == SingletonJugador.Instance.Jugador.IdJugador;
            }

            if (!JugadorEnLaSala.EsInvitado)
            {
                if (estoyBloqueado)
                {
                    MessageBox.Show("Parece que el jugador te ha bloqueado.", "Estás bloqueado", MessageBoxButton.OK, MessageBoxImage.Information);

                    if (soyHost)
                    {
                        imgMasOpciones_MenuContextual.IsOpen = true;
                    }
                }
                else
                {
                    imgMasOpciones_MenuContextual.IsOpen = true;
                }
            }
            else if (soyHost)
            {
                imgMasOpciones_MenuContextual.IsOpen = true;
            }
        }

        private void EnviarSolicitudAmistad()
        {
            try
            {
                Amistad amistad = new Amistad
                {
                    Estado = EstadoAmistad.SOLICITUD,
                    IdJugadorSolicitante = SingletonJugador.Instance.Jugador.IdJugador,
                    IdJugadorReceptor = JugadorEnLaSala.IdJugador,
                };

                if (_amistad.IdAmistad == 0)
                {
                    amistad.IdAmistad = _clienteAmistad.RegistrarNuevaAmistad(amistad, JugadorEnLaSala.NombreUsuario);
                }
                else
                {
                    amistad.IdAmistad = _amistad.IdAmistad;
                    _clienteAmistad.ActualizarSolicitudAmistad(amistad, EstadoAmistad.SOLICITUD);
                }

                _amistad = amistad;

                _ventanaPadre.ClienteJugadoresEnSala.NotificarCambioEnAmistad(_ventanaPadre.SalaActual.Codigo, SingletonJugador.Instance.Jugador.NombreUsuario, JugadorEnLaSala.NombreUsuario);
                ActualizarOpcionesDeMenuPopupLocal(EstadoAmistad.SOLICITUD);
            }
            catch (FaultException<AmistadException> ex)
            {
                MessageBox.Show(ex.Detail.Mensaje + "\n" + ex.Reason, Properties.Resources.tituloInvitacionNoEnviada, MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (TimeoutException)
            {
                MessageBox.Show(Properties.Resources.msgTimeoutEx, Properties.Resources.tituloTimeOut, MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (CommunicationException)
            {
                try
                {
                    Utilidad.ManejarCommunicationException(_clienteAmistad);
                }
                catch (RegresarAInicioSesionException)
                {
                    _ventanaPadre.RedirigirAInicioSesion();
                }
            }
        }

        private void BloquearJugador()
        {
            try
            {
                Amistad bloqueo = new Amistad
                {
                    Estado = EstadoAmistad.BLOQUEO,
                    IdJugadorSolicitante = SingletonJugador.Instance.Jugador.IdJugador,
                    IdJugadorReceptor = JugadorEnLaSala.IdJugador
                };

                if (_amistad.IdAmistad == 0)
                {
                    bloqueo.IdAmistad = _clienteAmistad.RegistrarNuevaAmistad(bloqueo, JugadorEnLaSala.NombreUsuario);
                }
                else
                {
                    bloqueo.IdAmistad = _amistad.IdAmistad;
                    _clienteAmistad.ActualizarSolicitudAmistad(bloqueo, EstadoAmistad.BLOQUEO);
                }

                _amistad = bloqueo;

                _ventanaPadre.ClienteJugadoresEnSala.NotificarCambioEnAmistad(_ventanaPadre.SalaActual.Codigo, SingletonJugador.Instance.Jugador.NombreUsuario, JugadorEnLaSala.NombreUsuario);
                ActualizarOpcionesDeMenuPopupLocal(EstadoAmistad.BLOQUEO);
            }
            catch (FaultException<AmistadException> ex)
            {
                MessageBox.Show(ex.Detail.Mensaje + "\n" + ex.Reason, Properties.Resources.tituloErrorBloqueo, MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (TimeoutException)
            {
                MessageBox.Show(Properties.Resources.msgTimeoutEx, Properties.Resources.tituloTimeOut, MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (CommunicationException)
            {
                try
                {
                    Utilidad.ManejarCommunicationException(_clienteAmistad);
                }
                catch (RegresarAInicioSesionException)
                {
                    _ventanaPadre.RedirigirAInicioSesion();
                }
            }
        }

        private void ExpulsarJugador()
        {
            try
            {
                _ventanaPadre.ClienteJugadoresEnSala?.ExpulsarJugador(_ventanaPadre.SalaActual.Codigo, JugadorEnLaSala.NombreUsuario);
                _ventanaPadre.MostrarDesconexionJugador(JugadorEnLaSala.NombreUsuario);
            }
            catch (TimeoutException)
            {
                MessageBox.Show(Properties.Resources.msgTimeoutEx, Properties.Resources.tituloTimeOut, MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (CommunicationException)
            {
                try
                {
                    Utilidad.ManejarCommunicationException(_ventanaPadre.ClienteJugadoresEnSala);
                }
                catch (RegresarAInicioSesionException)
                {
                    _ventanaPadre.RedirigirAInicioSesion();
                }
            }
        }

        private void DesbloquearJugador()
        {
            try
            {
                Amistad desbloqueo = new Amistad
                {
                    IdAmistad = _amistad.IdAmistad,
                    Estado = EstadoAmistad.RECHAZADA,
                    IdJugadorSolicitante = SingletonJugador.Instance.Jugador.IdJugador,
                    IdJugadorReceptor = JugadorEnLaSala.IdJugador
                };

                _clienteAmistad.ActualizarSolicitudAmistad(desbloqueo, EstadoAmistad.RECHAZADA);
                _ventanaPadre.ClienteJugadoresEnSala.NotificarCambioEnAmistad(_ventanaPadre.SalaActual.Codigo, SingletonJugador.Instance.Jugador.NombreUsuario, JugadorEnLaSala.NombreUsuario);
                ActualizarOpcionesDeMenuPopupLocal(EstadoAmistad.RECHAZADA);
            }
            catch (FaultException<AmistadException> ex)
            {
                MessageBox.Show(ex.Detail.Mensaje + "\n" + ex.Reason, Properties.Resources.tituloErrorBloqueo, MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (TimeoutException)
            {
                MessageBox.Show(Properties.Resources.msgTimeoutEx, Properties.Resources.tituloTimeOut, MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (CommunicationException)
            {
                try
                {
                    Utilidad.ManejarCommunicationException(_clienteAmistad);
                }
                catch (RegresarAInicioSesionException)
                {
                    _ventanaPadre.RedirigirAInicioSesion();
                }
            }
        }

        public void ActualizarOpcionesDeMenuPopupLocal(string nuevoEstadoAmistad)
        {
            imgMasOpciones_MenuContextual.Items.Clear();
            _amistad.Estado = nuevoEstadoAmistad;
            _amistad.IdJugadorSolicitante = SingletonJugador.Instance.Jugador.IdJugador;
            _amistad.IdJugadorReceptor = JugadorEnLaSala.IdJugador;

            CargarMenuPopup();
        }

        public void ActualizarOpcionesDeMenuPopupCallback(Amistad amistad)
        {
            imgMasOpciones_MenuContextual.Items.Clear();
            _amistad = amistad;

            CargarMenuPopup();
        }
    }
}