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
        public Jugador jugadorEnSala;

        private IVentanaSala _ventanaPadre;

        private ServicioAmistadClient _clienteAmistad;
        private Amistad _amistad;

        public JugadorEnSala(Jugador jugadorEnSala, IVentanaSala ventanaSala)
        {
            InitializeComponent();

            lbNombreJugador.Content = jugadorEnSala.NombreUsuario;
            this.jugadorEnSala = jugadorEnSala;

            _clienteAmistad = new ServicioAmistadClient();
            _ventanaPadre = ventanaSala;

            string rutaFotoPerfil = FotoPerfilUtils.ObtenerRutaFotoPerfil(jugadorEnSala.IdFotoPerfil);
            imgFotoPerfil.Source = new BitmapImage(new Uri(rutaFotoPerfil, UriKind.RelativeOrAbsolute));

            MostrarImagenMasOpciones();
        }

        private void MostrarImagenMasOpciones()
        {
            bool soyHost = SingletonJugador.Instance.Jugador.NombreUsuario.Equals(_ventanaPadre.SalaActual.NombreHost);
            bool soyYo = SingletonJugador.Instance.Jugador.Equals(jugadorEnSala);

            if (soyYo)
            {
                imgMasOpciones.Visibility = Visibility.Hidden;
            }
            else if (!jugadorEnSala.EsInvitado)
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

        public void AjustarMenuPopupSegunAmistad()
        {
            try
            {
                _amistad = _clienteAmistad.RecuperarAmistad(SingletonJugador.Instance.Jugador.IdJugador, jugadorEnSala.IdJugador);
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
                MessageBox.Show(Properties.Resources.msgComunnicationEx, Properties.Resources.globalTituloError, MessageBoxButton.OK, MessageBoxImage.Error);
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

                default: // "Rechazada" o No existe amistad
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

            if (!jugadorEnSala.EsInvitado)
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
                    IdJugadorReceptor = jugadorEnSala.IdJugador,
                };

                if (_amistad.IdAmistad == 0)
                {
                    amistad.IdAmistad = _clienteAmistad.RegistrarNuevaAmistad(amistad, jugadorEnSala.NombreUsuario);
                }
                else
                {
                    amistad.IdAmistad = _amistad.IdAmistad;
                    _clienteAmistad.ActualizarSolicitudAmistad(amistad, EstadoAmistad.SOLICITUD);
                }

                _amistad = amistad;
                
                _ventanaPadre.ClienteJugadoresEnSala.NotificarCambioEnAmistad(_ventanaPadre.SalaActual.Codigo, SingletonJugador.Instance.Jugador.NombreUsuario, jugadorEnSala.NombreUsuario);
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
            catch (CommunicationException ex)
            {
                MessageBox.Show(Properties.Resources.msgComunnicationEx, Properties.Resources.globalTituloError, MessageBoxButton.OK, MessageBoxImage.Error);
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
                    IdJugadorReceptor = jugadorEnSala.IdJugador
                };

                if (_amistad.IdAmistad == 0)
                {
                    bloqueo.IdAmistad = _clienteAmistad.RegistrarNuevaAmistad(bloqueo, jugadorEnSala.NombreUsuario);
                }
                else
                {
                    bloqueo.IdAmistad = _amistad.IdAmistad;
                    _clienteAmistad.ActualizarSolicitudAmistad(bloqueo, EstadoAmistad.BLOQUEO);
                }

                _amistad = bloqueo;
                
                _ventanaPadre.ClienteJugadoresEnSala.NotificarCambioEnAmistad(_ventanaPadre.SalaActual.Codigo, SingletonJugador.Instance.Jugador.NombreUsuario, jugadorEnSala.NombreUsuario);
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
                MessageBox.Show(Properties.Resources.msgComunnicationEx, Properties.Resources.globalTituloError, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ExpulsarJugador()
        {
            _ventanaPadre.ClienteJugadoresEnSala?.ExpulsarJugador(_ventanaPadre.SalaActual.Codigo, jugadorEnSala.NombreUsuario);
            _ventanaPadre.MostrarDesconexionJugador(jugadorEnSala.NombreUsuario);
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
                    IdJugadorReceptor = jugadorEnSala.IdJugador
                };

                _clienteAmistad.ActualizarSolicitudAmistad(desbloqueo, EstadoAmistad.RECHAZADA);
                _ventanaPadre.ClienteJugadoresEnSala.NotificarCambioEnAmistad(_ventanaPadre.SalaActual.Codigo, SingletonJugador.Instance.Jugador.NombreUsuario, jugadorEnSala.NombreUsuario);
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
                MessageBox.Show(Properties.Resources.msgComunnicationEx, Properties.Resources.globalTituloError, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void ActualizarOpcionesDeMenuPopupLocal(string nuevoEstadoAmistad)
        {
            imgMasOpciones_MenuContextual.Items.Clear();
            _amistad.Estado = nuevoEstadoAmistad;
            _amistad.IdJugadorSolicitante = SingletonJugador.Instance.Jugador.IdJugador;
            _amistad.IdJugadorReceptor = jugadorEnSala.IdJugador;

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
