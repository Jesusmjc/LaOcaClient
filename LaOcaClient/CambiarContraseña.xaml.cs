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
using System.Windows.Shapes;

namespace LaOcaClient
{
    public partial class CambiarContraseña : Window
    {
        private readonly IServicioCuenta _servicioCuenta;
        private Cuenta cuenta;

        public CambiarContraseña(int idCuenta)
        {
            InitializeComponent();
            _servicioCuenta = new ServicioCuentaClient();
            CargarDatosCuenta(idCuenta);
        }

        private void CargarDatosCuenta(int idCuenta)
        {
            try
            {
                cuenta = _servicioCuenta.ObtenerCuentaPorId(idCuenta);
                if (cuenta == null)
                {
                    MessageBox.Show("Cuenta no encontrada.");
                    Close();
                }
            }
            catch (CommunicationException ex)
            {
                MessageBox.Show($"Error de comunicación al cargar los datos de la cuenta: {ex.Message}");
                Close();
            }
            catch (TimeoutException ex)
            {
                MessageBox.Show($"La carga de los datos de la cuenta ha superado el tiempo de espera: {ex.Message}");
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error inesperado al cargar los datos de la cuenta: {ex.Message}");
                Close();
            }
        }

        private void btnActualizarContraseña_Click(object sender, RoutedEventArgs e)
        {
            string contraseñaActual = tbContraseñaActual.Password;
            string nuevaContraseña = tbNuevaContraseña.Password;
            string confirmarNuevaContraseña = tbConfirmarNuevaContraseña.Password;

            if (string.IsNullOrEmpty(contraseñaActual) || string.IsNullOrEmpty(nuevaContraseña) || string.IsNullOrEmpty(confirmarNuevaContraseña))
            {
                MessageBox.Show("Todos los campos son obligatorios.");
                return;
            }

            if (!Utilidad.ValidarContrasena(nuevaContraseña))
            {
                MessageBox.Show("La nueva contraseña no cumple con los requisitos. Debe tener entre 8 y 16 caracteres, incluir al menos una letra mayúscula, una letra minúscula, un número y un carácter especial.");
                return;
            }

            if (nuevaContraseña != confirmarNuevaContraseña)
            {
                MessageBox.Show("Las nuevas contraseñas no coinciden.");
                return;
            }

            try
            {
                bool esContraseñaCorrecta = _servicioCuenta.VerificarContraseñaActual(cuenta.IdCuenta, Utilidad.HashearConSha256(contraseñaActual));
                if (esContraseñaCorrecta)
                {
                    cuenta.Contrasena = Utilidad.HashearConSha256(nuevaContraseña);
                    _servicioCuenta.ModificarCuenta(cuenta);
                    MessageBox.Show("Contraseña actualizada exitosamente.");
                    CerrarSesion();
                }
                else
                {
                    MessageBox.Show("La contraseña actual es incorrecta.");
                }
            }
            catch (CommunicationException ex)
            {
                MessageBox.Show($"Error de comunicación al actualizar la contraseña: {ex.Message}");
            }
            catch (TimeoutException ex)
            {
                MessageBox.Show($"La actualización de la contraseña ha superado el tiempo de espera: {ex.Message}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error inesperado al actualizar la contraseña: {ex.Message}");
            }
        }

        private void CerrarSesion()
        {
            LaOcaService.ServicioJugadoresEnLineaClient clienteJugadoresEnLinea = new LaOcaService.ServicioJugadoresEnLineaClient();

            try
            {
                clienteJugadoresEnLinea.EliminarJugadorDesconectado(SingletonJugador.Instance.Jugador);
            }
            catch (TimeoutException)
            {
                MessageBox.Show("El servidor ha tardado demasiado en responder.", "Error de conexión", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (CommunicationException)
            {
                MessageBox.Show("Ha ocurrido un error al intentar conectar con el Servidor. Por favor intente de nuevo más tarde.", "Error de conexión", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            IniciarSesion ventanaIniciarSesion = new IniciarSesion();
            this.Close();
            ventanaIniciarSesion.ShowDialog();
        }

        private void btnVolver_Click(object sender, RoutedEventArgs e)
        {
            int idCuenta = SingletonJugador.Instance.Jugador.IdCuenta;
            int idJugador = SingletonJugador.Instance.Jugador.IdJugador;
            CrearCuenta ventanaCrearCuenta = new CrearCuenta(ModoCuenta.Modificar, idCuenta, idJugador);
            ventanaCrearCuenta.ActualizarVentanaModificar(ModoCuenta.Modificar);
            ventanaCrearCuenta.Show();
            this.Close();
        }
    }
}
