using LaOcaClient.LaOcaService;
using LaOcaClient.UserControls;
using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
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
    /// Interaction logic for Sala.xaml
    /// </summary>
    public partial class Sala : Window, IServicioChatCallback, IServicioSalaCallback
    {
        public LaOcaService.Sala sala;

        private InstanceContext contexto;
        private LaOcaService.ServicioChatClient clienteChat;
        private LaOcaService.ServicioSalaClient clienteSala;

        private Grid[] gridsJugadores;
        private JugadorEnSala[] jugadoresEnSala;

        private Social ventanaSocial;
        private bool _ventanaEstaAbierta = true;

        public Sala()
        {
            InitializeComponent();
            
            PrepararSala();
            MostrarPrimerJugador();
            UnirseAlChat();
        }

        public Sala(string nombreSala, string visibilidad)
        {
            InitializeComponent();

            PrepararSala();
            CrearSala(nombreSala, visibilidad);
            MostrarPrimerJugador();
            UnirseAlChat();
        }

        public Sala(LaOcaService.Sala sala)
        {
            InitializeComponent();

            this.sala = sala;
            lbNombreSala.Content = sala.Nombre;
            lbCodigoSala.Content = sala.Codigo;

            PrepararSala();
            MostrarJugadoresEnSala();
            AgregarJugadorASala();
            UnirseAlChat();
        }

        private void PrepararSala()
        {
            gridsJugadores = new Grid[4];
            gridsJugadores[0] = gridJugadorSala1;
            gridsJugadores[1] = gridJugadorSala2;
            gridsJugadores[2] = gridJugadorSala3;
            gridsJugadores[3] = gridJugadorSala4;

            jugadoresEnSala = new JugadorEnSala[4];

            contexto = new InstanceContext(this);
            clienteChat = new LaOcaService.ServicioChatClient(contexto);
            clienteSala = new LaOcaService.ServicioSalaClient(contexto);
        }

        private void CrearSala(string nombreSala, string visibilidad)
        {
            btnIniciarPartida.Visibility = Visibility.Visible;

            LaOcaService.Sala nuevaSala = new LaOcaService.Sala()
            {
                Nombre = nombreSala,
                Codigo = GenerarCodigoSala(),
                Jugadores = new Dictionary<string, LaOcaService.Jugador>(),
                Visibilidad = visibilidad,
                NombreHost = SingletonJugador.Instance.Jugador.NombreUsuario
            };
            nuevaSala.Jugadores.Add(SingletonJugador.Instance.Jugador.NombreUsuario, SingletonJugador.Instance.Jugador);
            sala = nuevaSala;

            lbNombreSala.Content = nuevaSala.Nombre;
            lbCodigoSala.Content = nuevaSala.Codigo;

            int resultadoAgregarSala = 0;

            try
            {
                resultadoAgregarSala = clienteSala.AgregarNuevaSala(nuevaSala);

                if (resultadoAgregarSala == 0)
                {
                    MessageBox.Show("Ha ocurrido un error al crear la sala.", "Error con la sala", MessageBoxButton.OK, MessageBoxImage.Error);
                }
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

        private void MostrarPrimerJugador()
        {
            JugadorEnSala jugadorSala = new JugadorEnSala(SingletonJugador.Instance.Jugador, sala.Codigo);
            gridJugadorSala1.Children.Add(jugadorSala);
        }

        private void MostrarJugadoresEnSala()
        {
            JugadorEnSala hostEnSala = new JugadorEnSala(sala.Jugadores[sala.NombreHost], sala.Codigo);
            gridsJugadores[0].Children.Add(hostEnSala);
            jugadoresEnSala[0] = hostEnSala;

            JugadorEnSala jugadorSala = new JugadorEnSala(SingletonJugador.Instance.Jugador, sala.Codigo);
            gridsJugadores[sala.Jugadores.Count].Children.Add(jugadorSala);
            jugadoresEnSala[sala.Jugadores.Count] = jugadorSala;

            int posicion = 1;

            foreach (var parJugador in sala.Jugadores)
            {
                if (!parJugador.Key.Equals(SingletonJugador.Instance.Jugador.NombreUsuario) && !parJugador.Key.Equals(sala.NombreHost))
                {
                    JugadorEnSala jugadorEnSala = new JugadorEnSala(parJugador.Value, sala.Codigo);
                   
                    gridsJugadores[posicion].Children.Add(jugadorEnSala);
                    jugadoresEnSala[posicion] = jugadorEnSala;

                    posicion++;
                }
            }
        }

        private string GenerarCodigoSala()
        {
            Random random = new Random();
            string codigoSala;
            bool esCodigoUnico = false;

            do
            {
                codigoSala = "";
                for (int i = 0; i < 4; i++)
                {
                    codigoSala += random.Next(0, 10).ToString();
                }

                try
                {
                    esCodigoUnico = clienteSala.VerificarCodigoSalaEsUnico(codigoSala);
                }
                catch (TimeoutException)
                {
                    MessageBox.Show("El servidor ha tardado demasiado en responder.", "Error de conexión", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                catch (CommunicationException)
                {
                    MessageBox.Show("Ha ocurrido un error al intentar conectar con el Servidor. Por favor intente de nuevo más tarde.", "Error de conexión", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            } while (!esCodigoUnico);

            return codigoSala;
        }

        private void AgregarJugadorASala()
        {
            sala.Jugadores.Add(SingletonJugador.Instance.Jugador.NombreUsuario, SingletonJugador.Instance.Jugador);

            try
            {
                clienteSala.AgregarJugadorASala(SingletonJugador.Instance.Jugador, sala.Codigo);
            }
            catch (TimeoutException)
            {
                MessageBox.Show("El servidor ha tardado demasiado en responder.", "Error de conexión", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (CommunicationException)
            {
                MessageBox.Show("Ha ocurrido un error al intentar conectar con el Servidor. Por favor intente de nuevo más tarde.", "Error de conexión", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al enviar mensaje: {ex.Message}");
            }
        }

        private void UnirseAlChat()
        {
            try
            {
                clienteChat.UnirseAlChat(SingletonJugador.Instance.Jugador.NombreUsuario, sala.Codigo);
            }
            catch (TimeoutException)
            {
                MessageBox.Show("El servidor ha tardado demasiado en responder.", "Error de conexión", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (CommunicationException)
            {
                MessageBox.Show("Ha ocurrido un error al intentar conectar con el Servidor. Por favor intente de nuevo más tarde.", "Error de conexión", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al enviar mensaje: {ex.Message}");
            }
        }

        public void MostrarMensaje(string nombreJugador, string mensaje)
        {
            Dispatcher.Invoke(() =>
            {
                lbChat.Items.Add($"{nombreJugador}: {mensaje}");
            });
        }

        private void EnviarMensaje(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                string mensaje = tbxMensaje.Text.ToString();

                if (!string.IsNullOrWhiteSpace(mensaje))
                {
                    try
                    {
                        clienteChat.EnviarMensaje(SingletonJugador.Instance.Jugador.NombreUsuario, mensaje, sala.Codigo);
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

                tbxMensaje.Text = "";
            }
        }

        public void MostrarNuevoJugadorEnSala(Jugador nuevoJugador)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                JugadorEnSala nuevoJugadorEnSala = new JugadorEnSala(nuevoJugador, sala.Codigo);
                if (SingletonJugador.Instance.Jugador.NombreUsuario.Equals(sala.NombreHost))
                {
                    nuevoJugadorEnSala.CargarOpcionExpulsar();
                }
                gridsJugadores[sala.Jugadores.Count].Children.Add(nuevoJugadorEnSala);
                jugadoresEnSala[sala.Jugadores.Count] = nuevoJugadorEnSala;

                sala.Jugadores.Add(nuevoJugador.NombreUsuario, nuevoJugador);

                if (sala.Jugadores.Count > 1)
                {
                    btnIniciarPartida.IsEnabled = true;
                }
            });
        }

        private void LimpiarTextoEjemplo(object sender, RoutedEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            if (textBox != null && tbxMensaje.Text.ToString().Equals("Escribe un mensaje"))
            {
                textBox.Clear();
            }
        }

        private void IniciarPartida(object sender, RoutedEventArgs e)
        {
            if (sala.Jugadores.Count >= 2)
            {
                LaOcaService.Partida nuevaPartida = clienteSala.IniciarPartida(sala.Codigo);

                sala.Partida = nuevaPartida;

                Partida ventanaPartida = new Partida(sala);
                this.Close();
                ventanaPartida.ShowDialog();
            }
            else
            {
                MessageBox.Show("Se necesitan al menos dos jugadores para iniciar partida.", "Se necesitan más jugadores", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void MostrarVentanaDePartida(LaOcaService.Partida partida)
        {
            sala.Partida = partida;

            Partida ventanaPartida = new Partida(sala);
            this.Close();
            ventanaPartida.ShowDialog();
        }

        private void RegresarAMenuPrincipal(object sender, MouseButtonEventArgs e)
        {
            MessageBoxResult resultado = MessageBox.Show("¿Estás seguro de que quieres salir al Menú Principal?", "Estás a punto de abandonar la partida", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (resultado == MessageBoxResult.Yes)
            {
                if (!SingletonJugador.Instance.Jugador.NombreUsuario.Equals(sala.NombreHost))
                {
                    clienteSala.NotificarDesconexion(SingletonJugador.Instance.Jugador.NombreUsuario, sala.Codigo);
                }
                else
                {
                    clienteSala.EliminarSala(sala.Codigo);
                }

                MenuPrincipal ventanaMenuPrincipal = new MenuPrincipal();
                this.Close();
                ventanaMenuPrincipal.ShowDialog();
            }
        }

        public void MostrarDesconexionJugador(string nombreJugadorDesconectado)
        {
            sala.Jugadores.Remove(nombreJugadorDesconectado);

            int posicionJugadorDesconectado = 3;

            for (int i = sala.Jugadores.Count; i >= 1; i--)
            {
                if (jugadoresEnSala[i].jugadorEnSala.NombreUsuario.Equals(nombreJugadorDesconectado))
                {
                    posicionJugadorDesconectado = i;
                    gridsJugadores[i].Children.Clear();
                    jugadoresEnSala[i] = null;

                    break;
                }
            }

            for (int i = posicionJugadorDesconectado;  i < sala.Jugadores.Count; i++)
            {
                JugadorEnSala jugadorEnSalaTemp = jugadoresEnSala[i + 1];

                gridsJugadores[i + 1].Children.Clear();
                gridsJugadores[i].Children.Add(jugadorEnSalaTemp);
                jugadoresEnSala[i] = jugadoresEnSala[i + 1];
            }

            gridsJugadores[sala.Jugadores.Count].Children.Clear();
            jugadoresEnSala[sala.Jugadores.Count] = null;

            if (sala.Jugadores.Count < 2)
            {
                btnIniciarPartida.IsEnabled = false;
            }
        }

        public void ExpulsarAMenúPrincipal(string motivo)
        {
            MessageBox.Show(motivo, "Has sido expulsado de la sala", MessageBoxButton.OK, MessageBoxImage.Information);

            MenuPrincipal ventanaMenuPrincipal = new MenuPrincipal();
            ventanaSocial?.Close();
            _ventanaEstaAbierta = false;
            this.Close();
            ventanaMenuPrincipal.ShowDialog();
        }

        private void MostrarAmigos(object sender, RoutedEventArgs e)
        {
            Social ventanaAmigos = new Social(this);
            this.ventanaSocial = ventanaAmigos;

            this.Hide();
            ventanaAmigos.ShowDialog();
            if (_ventanaEstaAbierta)
            {
                this.Show();
            }
        }
    }
}
