using LaOcaClient.LaOcaService;
using System;
using System.Collections.Generic;
using System.Linq;
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
using System.ServiceModel;

namespace LaOcaClient
{
    public partial class RecuperarContraseña : Window
    {
        private readonly IServicioCuenta _servicioCuenta;

        public RecuperarContraseña()
        {
            InitializeComponent();
            _servicioCuenta = new ServicioCuentaClient();
        }

        private void btnEnviarCodigoRestablecimiento_Click(object sender, RoutedEventArgs e)
        {
            string correo = tbCorreoElectronico.Text;

            if (string.IsNullOrEmpty(correo))
            {
                MessageBox.Show("Por favor, ingrese su correo electrónico.");
                return;
            }

            if (!Utilidad.ValidarCorreoElectronico(correo))
            {
                MessageBox.Show("El correo electrónico no es válido. Debe ser un correo de gmail, outlook o hotmail.");
                return;
            }

            try
            {
                _servicioCuenta.EnviarCodigoVerificacion(correo);
                MessageBox.Show("Se ha enviado un código de restablecimiento a su correo.");
                ActualizarVentanaCodigoVerificacion();
            }
            catch (CommunicationException ex)
            {
                MessageBox.Show($"Error de comunicación al enviar el código de restablecimiento: {ex.Message}");
            }
            catch (TimeoutException ex)
            {
                MessageBox.Show($"El envío del código de restablecimiento ha superado el tiempo de espera: {ex.Message}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error inesperado al enviar el código de restablecimiento: {ex.Message}");
            }
        }

        private int idCuenta;

        private void btnVerificarCodigo_Click(object sender, RoutedEventArgs e)
        {
            string correo = tbCorreoElectronico.Text;
            string codigoIngresado = tbCodigoRestablecimiento.Text;

            if (string.IsNullOrEmpty(correo))
            {
                MessageBox.Show("Por favor, ingrese su correo electrónico.");
                return;
            }

            if (string.IsNullOrEmpty(codigoIngresado))
            {
                MessageBox.Show("Por favor, ingrese el código de verificación.");
                return;
            }

            try
            {
                int cuentaId = _servicioCuenta.VerificarCodigoRecuperarContraseña(correo, codigoIngresado);

                if (cuentaId > 0)
                {
                    idCuenta = cuentaId;
                    MessageBox.Show("Código de verificación correcto.");
                    ActualizarVentanaRestablecerContrasena();
                }
                else
                {
                    MessageBox.Show("Código de verificación incorrecto.");
                }
            }
            catch (CommunicationException ex)
            {
                MessageBox.Show($"Error de comunicación al verificar el código: {ex.Message}");
            }
            catch (TimeoutException ex)
            {
                MessageBox.Show($"La verificación del código ha superado el tiempo de espera: {ex.Message}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error inesperado al verificar el código: {ex.Message}");
            }
        }

        private void ActualizarVentanaCodigoVerificacion()
        {
            spCorreo.Visibility = Visibility.Collapsed;
            spCodigo.Visibility = Visibility.Visible;
        }

        private void ActualizarVentanaRestablecerContrasena()
        {
            spCodigo.Visibility = Visibility.Collapsed;
            spNuevaContraseña.Visibility = Visibility.Visible;
        }

        private void btnRestablecerContrasena_Click(object sender, RoutedEventArgs e)
        {
            string nuevaContrasena = pbNuevaContrasena.Password;
            string confirmarContrasena = pbConfirmarContrasena.Password;

            if (string.IsNullOrEmpty(nuevaContrasena) || string.IsNullOrEmpty(confirmarContrasena))
            {
                MessageBox.Show("Todos los campos son obligatorios.");
                return;
            }

            if (!Utilidad.ValidarContrasena(nuevaContrasena))
            {
                MessageBox.Show("La nueva contraseña no cumple con los requisitos. Debe tener entre 8 y 16 caracteres, incluir al menos una letra mayúscula, una letra minúscula, un número y un carácter especial.");
                return;
            }

            if (nuevaContrasena != confirmarContrasena)
            {
                MessageBox.Show("Las contraseñas no coinciden.");
                return;
            }

            if (idCuenta <= 0)
            {
                MessageBox.Show("Primero verifique el código de recuperación.");
                return;
            }

            try
            {
                _servicioCuenta.ModificarContraseña(idCuenta, Utilidad.HashearConSha256(nuevaContrasena));
                MessageBox.Show("Contraseña restablecida exitosamente.");
                IniciarSesion ventanaIniciarSesion = new IniciarSesion();
                ventanaIniciarSesion.Show();
                this.Close();
            }
            catch (CommunicationException ex)
            {
                MessageBox.Show($"Error de comunicación al restablecer la contraseña: {ex.Message}");
            }
            catch (TimeoutException ex)
            {
                MessageBox.Show($"El restablecimiento de la contraseña ha superado el tiempo de espera: {ex.Message}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error inesperado al restablecer la contraseña: {ex.Message}");
            }
        }

        private void btnVolver_Click(object sender, RoutedEventArgs e)
        {
            IniciarSesion ventanaIniciarSesion = new IniciarSesion();
            ventanaIniciarSesion.Show();
            this.Close();
        }
    }
}
