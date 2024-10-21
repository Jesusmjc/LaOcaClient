using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Channels;
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

namespace LaOcaClient
{
    /// <summary>
    /// Interaction logic for Chat.xaml
    /// </summary>
    public partial class Chat : Window, IServicioChatCallback
    {
        private InstanceContext contexto;
        private LaOcaService.ServicioChatClient cliente;

        public Chat()
        {
            InitializeComponent();

            contexto = new InstanceContext(this);
            cliente = new LaOcaService.ServicioChatClient(contexto);
        }

        private void UnirseAlChat(object sender, RoutedEventArgs e)
        {
            string nombreJugador = tbxNombreJugador.Text.ToString();

            if (!string.IsNullOrWhiteSpace(nombreJugador))
            {
                try
                {
                    cliente.UnirseAlChat(nombreJugador);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error al enviar mensaje: {ex.Message}");
                }
            }

            
        }

        private void EnviarMensaje(object sender, RoutedEventArgs e)
        {
            string nombreJugador = tbxNombreJugador.Text.ToString();
            string mensaje = tbxMensaje.Text.ToString();

            if (!string.IsNullOrWhiteSpace(nombreJugador) && !string.IsNullOrWhiteSpace(mensaje))
            try
            {
                cliente.EnviarMensaje(nombreJugador, mensaje);
            } 
            catch (Exception ex) when (ex is CommunicationException | ex is TimeoutException)
            {
                    MessageBox.Show("Ha ocurrido un error al enviar el mensaje.", "Error de conexión.", MessageBoxButton.OK, MessageBoxImage.Error);
                }
        }

        public void MostrarMensaje(string nombreJugador, string mensaje)
        {
            Dispatcher.Invoke(() =>
            {
                lbChat.Items.Add($"{nombreJugador}: {mensaje}");
            });
        }
    }
}
