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
        private LaOcaService.Sala sala;

        private InstanceContext contexto;
        private LaOcaService.ServicioChatClient clienteChat;
        private LaOcaService.ServicioSalaClient clienteSala;

        private Grid[] gridsJugadores;

        public Sala()
        {
            InitializeComponent();
            
            PrepararSala();
            MostrarPrimerJugador();
            //CrearSala();
            UnirseAlChat();
        }

        public Sala(string nombreSala, string visibilidad)
        {
            InitializeComponent();

            PrepararSala();
            MostrarPrimerJugador();
            CrearSala(nombreSala, visibilidad);
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

            contexto = new InstanceContext(this);
            clienteChat = new LaOcaService.ServicioChatClient(contexto);
            clienteSala = new LaOcaService.ServicioSalaClient(contexto);
        }

        private void CrearSala(string nombreSala, string visibilidad)
        {
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
            catch (TimeoutException ex)
            {
                MessageBox.Show("El servidor ha tardado demasiado en responder.", "Error de conexión", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (CommunicationException ex)
            {
                MessageBox.Show("Ha ocurrido un error al intentar conectar con el Servidor. Por favor intente de nuevo más tarde.", "Error de conexión", MessageBoxButton.OK, MessageBoxImage.Error);
            }

        }

        private void MostrarPrimerJugador()
        {
            JugadorEnSala jugadorSala = new JugadorEnSala(SingletonJugador.Instance.Jugador.NombreUsuario, SingletonJugador.Instance.Jugador.IdJugador);
            gridJugadorSala1.Children.Add(jugadorSala);
        }

        private void MostrarJugadoresEnSala()
        {
            JugadorEnSala hostEnSala = new JugadorEnSala(sala.NombreHost, sala.Jugadores[sala.NombreHost].IdJugador);
            gridsJugadores[0].Children.Add(hostEnSala);

            JugadorEnSala jugadorSala = new JugadorEnSala(SingletonJugador.Instance.Jugador.NombreUsuario, SingletonJugador.Instance.Jugador.IdJugador);
            gridsJugadores[sala.Jugadores.Count].Children.Add(jugadorSala);

            int posicion = 1;

            foreach (var parJugador in sala.Jugadores)
            {
                if (!parJugador.Key.Equals(SingletonJugador.Instance.Jugador.NombreUsuario) && !parJugador.Key.Equals(sala.NombreHost))
                {
                    gridsJugadores[posicion].Children.Add(new JugadorEnSala(parJugador.Key, parJugador.Value.IdJugador));

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
            if (!sala.Jugadores.ContainsKey(nuevoJugador.NombreUsuario)) // Verificar si el jugador ya existe
            {
                // Crear una nueva visualización del jugador en la interfaz
                JugadorEnSala nuevoJugadorEnSala = new JugadorEnSala(nuevoJugador.NombreUsuario, nuevoJugador.IdJugador);
                gridsJugadores[sala.Jugadores.Count].Children.Add(nuevoJugadorEnSala);

                // Agregar el jugador a la sala
                sala.Jugadores.Add(nuevoJugador.NombreUsuario, nuevoJugador);

                // Habilitar el botón para iniciar la partida si hay más de un jugador
                if (sala.Jugadores.Count > 1)
                {
                    btnIniciarPartida.IsEnabled = true;
                }
            }
            else
            {
                Console.WriteLine($"El jugador {nuevoJugador.NombreUsuario} ya está en la sala.");
            }
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
    }
}
