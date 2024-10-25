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

            try
            {
                _servicioCuenta.EnviarCodigoVerificacion(correo);
                MessageBox.Show("Se ha enviado un código de restablecimiento a su correo.");
                ActualizarVentanaCodigoVerificacion();


            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al enviar el código de restablecimiento: {ex.Message}");
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
            catch (Exception ex)
            {
                MessageBox.Show($"Error al verificar el código: {ex.Message}");
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

            if (nuevaContrasena != confirmarContrasena)
            {
                MessageBox.Show("Las contraseñas no coinciden.");
                return;
            }

            // Asegurarse de que el idCuenta fue obtenido correctamente después de la verificación del código
            if (idCuenta <= 0)
            {
                MessageBox.Show("Primero verifique el código de recuperación.");
                return;
            }

            try
            {
                _servicioCuenta.ModificarContraseña(idCuenta, nuevaContrasena);
                MessageBox.Show("Contraseña restablecida exitosamente.");
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al restablecer la contraseña: {ex.Message}");
            }
        }



    }
}

