using LaOcaClient.LaOcaService;
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

namespace LaOcaClient.UserControls
{
    /// <summary>
    /// Interaction logic for JugadorEnSala.xaml
    /// </summary>
    public partial class JugadorEnSala : UserControl
    {
        public Jugador jugadorEnSala;

        private ServicioAmistadClient _clienteAmistad;
        private Amistad _amistad;
        private string _codigoSala;
        private bool _esJugadorActualHost = false;

        public JugadorEnSala(Jugador jugadorEnSala, string codigoSala)
        {
            InitializeComponent();

            this.jugadorEnSala = jugadorEnSala;

            lbNombreJugador.Content = jugadorEnSala.NombreUsuario;

            _clienteAmistad = new ServicioAmistadClient();
            _codigoSala = codigoSala;

            if (SingletonJugador.Instance.Jugador.Equals(jugadorEnSala))
            {
                imgMasOpciones.Visibility = Visibility.Hidden;
            }
            else
            {
                RecuperarAmistadConJugador();
                CargarMenuPopup();
            }
        }

        private void RecuperarAmistadConJugador()
        {
            try
            {
                _amistad = _clienteAmistad.RecuperarAmistad(SingletonJugador.Instance.Jugador.IdJugador, jugadorEnSala.IdJugador);
            }
            catch (FaultException<AmistadException> ex)
            {
                MessageBox.Show(ex.Detail.Mensaje + "\n" + ex.Reason, "Error al enviar la solicitud.", MessageBoxButton.OK, MessageBoxImage.Error);
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

        private void CargarMenuPopup()
        {
            switch (_amistad.Estado)
            {
                case "Solicitud":
                    CargarOpcionBloquear();
                    break;

                case "Amigos":
                    CargarOpcionBloquear();
                    break;

                case "Bloqueo":
                    CargarOpcionDesbloquear();
                    break;

                default:
                    CargarOpcionEnviarSolicitudAmistad();
                    CargarOpcionBloquear();
                    break;
            }
        }

        private void CargarOpcionEnviarSolicitudAmistad()
        {
            MenuItem opcionEnviarSolicitudAmistad = new MenuItem { Header = "Enviar Solicitud de Amistad" };
            opcionEnviarSolicitudAmistad.Click += (s, args) => EnviarSolicitudAmistad();
            imgMasOpciones_MenuContextual.Items.Add(opcionEnviarSolicitudAmistad);
        }

        private void CargarOpcionBloquear()
        {
            MenuItem opcionBloquearJugador = new MenuItem { Header = "Bloquear" };
            opcionBloquearJugador.Click += (s, args) => BloquearJugador();
            imgMasOpciones_MenuContextual.Items.Add(opcionBloquearJugador);

            _esHost = true;
        }

        private void CargarOpcionDesbloquear()
        {
            MenuItem opcionDesbloquearJugador = new MenuItem { Header = "Desbloquear" };
            opcionDesbloquearJugador.Click += (s, args) => DesbloquearJugador();
            imgMasOpciones_MenuContextual.Items.Add(opcionDesbloquearJugador);
        }

        public void CargarOpcionExpulsar()
        {
            MenuItem opcionExpulsar = new MenuItem { Header = "Expulsar" };
            opcionExpulsar.Click += (s, args) => ExpulsarJugador();
            imgMasOpciones_MenuContextual.Items.Add(opcionExpulsar);
        }

        private void MostrarMenuPopup(object sender, MouseButtonEventArgs e)
        {
            imgMasOpciones_MenuContextual.IsOpen = true;
        }

        private void EnviarSolicitudAmistad()
        {
            try
            {
                _clienteAmistad.EnviarSolicitudAmistad(SingletonJugador.Instance.Jugador, jugadorEnSala);
                ActualizarOpcionesDeMenúPopup(EstadoAmistad.SOLICITUD);
            }
            catch (FaultException<AmistadException> ex)
            {
                MessageBox.Show(ex.Detail.Mensaje + "\n" + ex.Reason, "Error al enviar la solicitud.", MessageBoxButton.OK, MessageBoxImage.Error);
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

        private void BloquearJugador()
        {
            try
            {
                _clienteAmistad.ActualizarSolicitudAmistad(_amistad, EstadoAmistad.BLOQUEO);
                ActualizarOpcionesDeMenúPopup(EstadoAmistad.BLOQUEO);
            }
            catch (FaultException<AmistadException> ex)
            {
                MessageBox.Show(ex.Detail.Mensaje + "\n" + ex.Reason, "Error al procesar el bloqueo.", MessageBoxButton.OK, MessageBoxImage.Error);
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

        private void ExpulsarJugador()
        {
            ServicioExpulsionSalaClient clienteExpulsionSala = new ServicioExpulsionSalaClient();
            clienteExpulsionSala.ExpulsarJugador(_codigoSala, jugadorEnSala.NombreUsuario);
        }

        private void DesbloquearJugador()
        {
            try
            {
                _clienteAmistad.ActualizarSolicitudAmistad(_amistad, EstadoAmistad.RECHAZADA);
                ActualizarOpcionesDeMenúPopup(EstadoAmistad.RECHAZADA);
            }
            catch (FaultException<AmistadException> ex)
            {
                MessageBox.Show(ex.Detail.Mensaje + "\n" + ex.Reason, "Error al procesar el bloqueo.", MessageBoxButton.OK, MessageBoxImage.Error);
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

        private void ActualizarOpcionesDeMenúPopup(string nuevoEstadoAmistad)
        {
            imgMasOpciones_MenuContextual.Items.Clear();
            _amistad.Estado = nuevoEstadoAmistad;
            CargarMenuPopup();

            if (_esJugadorActualHost)
            {
                CargarOpcionExpulsar();
            }
        }
    }
}
