using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
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
using System.IO;
using LaOcaClient.LaOcaService;
using System.ServiceModel;
using System.Windows.Threading;

namespace LaOcaClient
{
    public enum ModoCuenta
    {
        Crear,
        Modificar
    }

    public partial class CrearCuenta : Window
    {
        private readonly IServicioCuenta _servicioCuenta;
        private readonly IServicioCuenta _servicioJugador;
        private readonly IServicioCuenta _servicioAspecto; 
        private string _imagenPerfilSeleccionada;
        private DispatcherTimer _timer;
        private int _tiempoRestante;
        private readonly ModoCuenta _modo;

        public CrearCuenta(ModoCuenta modo)
        {
            InitializeComponent();
            _servicioCuenta = new ServicioCuentaClient();
            _servicioJugador = new ServicioCuentaClient();
            _servicioAspecto = new ServicioCuentaClient();
            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromSeconds(1);
            _timer.Tick += Timer_Tick;
            _modo = modo;
            AjustarInterfazSegunModo();
        }

        public CrearCuenta(ModoCuenta modo, int idCuenta, int idJugador) : this(modo)
        {
            if (modo == ModoCuenta.Modificar)
            {
                CargarDatosJugador(idCuenta, idJugador);
            }
        }

        private void AjustarInterfazSegunModo()
        {
            if (_modo == ModoCuenta.Crear)
            {
                btnSiguienteCrearCuenta.Visibility = Visibility.Visible;
                btnSiguienteModificarCuenta.Visibility = Visibility.Collapsed;
            }
            else if (_modo == ModoCuenta.Modificar)
            {
                btnSiguienteCrearCuenta.Visibility = Visibility.Collapsed;
                btnSiguienteModificarCuenta.Visibility = Visibility.Visible;

                tbContraseña.Visibility = Visibility.Collapsed;
                tbConfirmarContraseña.Visibility = Visibility.Collapsed;
                btnCambiarContraseña.Visibility = Visibility.Visible;
                globalCorreo.Margin = new Thickness(78, 440, 0, 0);
                tbCorreo.Margin = new Thickness(78, 468, 0, 0);
                btnCambiarContraseña.Margin = new Thickness(78, 353, 0, 0);
            }
        }

        private void btnSiguienteCrear_Click(object sender, RoutedEventArgs e)
        {
            string nombreUsuario = tbNombreUsuario.Text;
            string contrasena = tbContraseña.Password;
            string confirmarContrasena = tbConfirmarContraseña.Password;
            string correo = tbCorreo.Text;

            if (contrasena != confirmarContrasena)
            {
                MessageBox.Show("Las contraseñas no coinciden.");
                return;
            }

            var cuenta = new Cuenta
            {
                correoElectronico = correo,
                contrasena = contrasena
            };

            var jugador = new Jugador
            {
                nombreUsuario = nombreUsuario,
                idFotoPerfil = ObtenerIdAspectoPorReferencia(_imagenPerfilSeleccionada)
            };

            try
            {
                _servicioCuenta.EnviarCodigoVerificacion(correo);
                MessageBox.Show("Se han guardado los datos de tu cuenta. Por favor revisa el código de verificación que se envió a tu correo electrónico.");
                ActualizarVentanaCrearCuenta();
                IniciarTemporizador();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al enviar el código de verificación: {ex.Message}");
            }
        }

        private void btnSiguienteModificar_Click(object sender, RoutedEventArgs e)
        {
            string nombreUsuario = tbNombreUsuario.Text;
            string contrasena = tbContraseña.Password;
            string confirmarContrasena = tbConfirmarContraseña.Password;
            string correo = tbCorreo.Text;

            var cuenta = new Cuenta
            {
                idCuenta = 2,
                correoElectronico = correo,
                contrasena = contrasena
            };

            int idFotoPerfil;
            try
            {
                idFotoPerfil = ObtenerIdAspectoPorReferencia(_imagenPerfilSeleccionada);
            }
            catch (ArgumentException)
            {
                MessageBox.Show("La referencia de la imagen seleccionada no es válida.");
                return;
            }

            MessageBox.Show($"ID de la imagen de perfil seleccionada: {idFotoPerfil}");

            var jugador = new Jugador
            {
                idJugador = 2,
                nombreUsuario = nombreUsuario,
                idFotoPerfil = idFotoPerfil
            };

            try
            {
                _servicioCuenta.ModificarCuenta(cuenta);
                _servicioJugador.ModificarJugador(jugador);
                MessageBox.Show("Cuenta modificada exitosamente.");
                CargarDatosJugador(2, 2);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al modificar la cuenta: {ex.Message}");
            }
        }

        private void btnVerificarCodigo_Click(object sender, RoutedEventArgs e)
        {
            string correo = tbCorreo.Text;
            string codigoIngresado = tbCodigoVerificacion.Text;

            try
            {
                bool esCodigoCorrecto = _servicioCuenta.VerificarCodigoCrearCuenta(correo, codigoIngresado);
                if (esCodigoCorrecto)
                {
                    MessageBox.Show("Código de verificación correcto. Creando cuenta...");

                    var cuenta = new Cuenta
                    {
                        correoElectronico = tbCorreo.Text,
                        contrasena = tbContraseña.Password
                    };

                    var jugador = new Jugador
                    {
                        nombreUsuario = tbNombreUsuario.Text,
                        idFotoPerfil = ObtenerIdAspectoPorReferencia(_imagenPerfilSeleccionada)
                    };

                    try
                    {
                        _servicioCuenta.CrearCuenta(cuenta, jugador, _imagenPerfilSeleccionada);
                        MessageBox.Show("Cuenta creada exitosamente.");
                        this.Close();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error al crear la cuenta: {ex.Message}");
                    }
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

        private void btnReenviarCodigo_Click(object sender, RoutedEventArgs e)
        {
            string correo = tbCorreo.Text;

            try
            {
                _servicioCuenta.EnviarCodigoVerificacion(correo);
                MessageBox.Show("Se ha reenviado el código de verificación a tu correo electrónico.");
                IniciarTemporizador();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al reenviar el código de verificación: {ex.Message}");
            }
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            _tiempoRestante--;
            if (_tiempoRestante <= 0)
            {
                _timer.Stop();
                btnReenviarCodigo.IsEnabled = true;
                lbReenviarCodigo.Content = "Reenviar código";
            }
            else
            {
                lbReenviarCodigo.Content = $"Reenviar código ({_tiempoRestante}s)";
            }
        }

        private void IniciarTemporizador()
        {
            _tiempoRestante = 60;
            btnReenviarCodigo.IsEnabled = false;
            _timer.Start();
        }

        private static readonly Dictionary<string, int> referenciaToIdMap = new Dictionary<string, int>
        {
            { "pack://application:,,,/LaOcaClient;component/Recursos/OcaDeportista.jpg", 1 },
            { "pack://application:,,,/LaOcaClient;component/Recursos/OcaDesastrosa.jpg", 2 },
            { "pack://application:,,,/LaOcaClient;component/Recursos/OcaIngeniera.jpg", 3 },
            { "pack://application:,,,/LaOcaClient;component/Recursos/OcaProgramadora.jpg", 4 },
            { "pack://application:,,,/LaOcaClient;component/Recursos/OcaRockstar.jpg", 5 },
            { "pack://application:,,,/LaOcaClient;component/Recursos/OcaUniversitaria.jpg", 6 }
        };

        private int ObtenerIdAspectoPorReferencia(string referencia)
        {
            if (referenciaToIdMap.TryGetValue(referencia, out int id))
            {
                return id;
            }
            else
            {
                throw new ArgumentException("Referencia no válida", nameof(referencia));
            }
        }

        private void btnCancelar_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void ImagenPerfil_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Image imagenSeleccionada = sender as Image;
            _imagenPerfilSeleccionada = imagenSeleccionada.Source.ToString();

            MessageBox.Show($"Referencia de la imagen seleccionada: {_imagenPerfilSeleccionada}");

            foreach (var child in wpImagenesPerfil.Children)
            {
                if (child is Image img)
                {
                    img.Opacity = img == imagenSeleccionada ? 1.0 : 0.5;
                }
            }
        }

        private void ActualizarVentanaCrearCuenta()
        {
            this.Height = 400;
            this.Width = 600;
            tbCodigoVerificacion.Visibility = Visibility.Visible;
            btnVerificarCodigo.Visibility = Visibility.Visible;
            lbIngresarCodigo.Visibility = Visibility.Visible;
            btnReenviarCodigo.Visibility = Visibility.Visible;
            lbReenviarCodigo.Visibility = Visibility.Visible;
            btnVolverAtras.Visibility = Visibility.Visible;

            globalCrearCuenta.Visibility = Visibility.Collapsed;
            globalNombreUsuario.Visibility = Visibility.Collapsed;
            tbNombreUsuario.Visibility = Visibility.Collapsed;
            globalContraseña.Visibility = Visibility.Collapsed;
            tbContraseña.Visibility = Visibility.Collapsed;
            globalConfirmarContraseña.Visibility = Visibility.Collapsed;
            tbConfirmarContraseña.Visibility = Visibility.Collapsed;
            globalCorreo.Visibility = Visibility.Collapsed;
            tbCorreo.Visibility = Visibility.Collapsed;
            btnSiguienteCrearCuenta.Visibility = Visibility.Collapsed;
            btnCancelar.Visibility = Visibility.Collapsed;
            lbSeleccionarImagenPerfil.Visibility = Visibility.Collapsed;
            wpImagenesPerfil.Visibility = Visibility.Collapsed;
        }

        private void btnVolverAtras_Click(object sender, RoutedEventArgs e)
        {
            this.Height = 600;
            this.Width = 900;
            tbCodigoVerificacion.Visibility = Visibility.Collapsed;
            btnVerificarCodigo.Visibility = Visibility.Collapsed;
            lbIngresarCodigo.Visibility = Visibility.Collapsed;
            btnReenviarCodigo.Visibility = Visibility.Collapsed;
            lbReenviarCodigo.Visibility = Visibility.Collapsed;
            btnVolverAtras.Visibility = Visibility.Collapsed;

            globalCrearCuenta.Visibility = Visibility.Visible;
            globalNombreUsuario.Visibility = Visibility.Visible;
            tbNombreUsuario.Visibility = Visibility.Visible;
            globalContraseña.Visibility = Visibility.Visible;
            tbContraseña.Visibility = Visibility.Visible;
            globalConfirmarContraseña.Visibility = Visibility.Visible;
            tbConfirmarContraseña.Visibility = Visibility.Visible;
            globalCorreo.Visibility = Visibility.Visible;
            tbCorreo.Visibility = Visibility.Visible;
            btnSiguienteCrearCuenta.Visibility = Visibility.Visible;
            btnCancelar.Visibility = Visibility.Visible;
            lbSeleccionarImagenPerfil.Visibility = Visibility.Visible;
            wpImagenesPerfil.Visibility = Visibility.Visible;
        }

        private void CargarDatosJugador(int idCuenta, int idJugador)
        {
            var cuenta = _servicioCuenta.ObtenerCuentaPorId(idCuenta);
            if (cuenta == null)
            {
                MessageBox.Show("Cuenta no encontrada.");
                return;
            }

            var jugador = _servicioJugador.ObtenerJugadorPorId(idJugador);
            if (jugador == null)
            {
                MessageBox.Show("Jugador no encontrado.");
                return;
            }

            tbNombreUsuario.Text = jugador.nombreUsuario;
            tbCorreo.Text = cuenta.correoElectronico;
            tbContraseña.Password = cuenta.contrasena;
            tbConfirmarContraseña.Password = cuenta.contrasena;

            var aspecto = _servicioAspecto.ObtenerAspectoPorId(jugador.idFotoPerfil);
            if (aspecto != null)
            {
                _imagenPerfilSeleccionada = aspecto.referencia;
                foreach (var child in wpImagenesPerfil.Children)
                {
                    if (child is Image img && img.Source.ToString() == _imagenPerfilSeleccionada)
                    {
                        img.Opacity = 1.0;
                    }
                    else if (child is Image img2)
                    {
                        img2.Opacity = 0.5;
                    }
                }
            }
            else
            {
                MessageBox.Show("Imagen de perfil no encontrada.");
            }
        }

        public void ActualizarVentanaModificar(ModoCuenta modo)
        {
            if (modo == ModoCuenta.Modificar)
            {
                globalContraseña.Visibility = Visibility.Collapsed;
                tbContraseña.Visibility = Visibility.Collapsed;
                globalConfirmarContraseña.Visibility = Visibility.Collapsed;
                tbConfirmarContraseña.Visibility = Visibility.Collapsed;
                btnCambiarContraseña.Visibility = Visibility.Visible;
                globalCorreo.Margin = new Thickness(78, 440, 0, 0);
                tbCorreo.Margin = new Thickness(78, 468, 0, 0);
                btnCambiarContraseña.Margin = new Thickness(78, 353, 0, 0);
            }
        }

        private void btnCambiarContraseña_Click(object sender, RoutedEventArgs e)
        {
            CambiarContraseña ventanaCambiarContraseña = new CambiarContraseña(2);
            ventanaCambiarContraseña.ShowDialog();
            this.Close();
        }
    }
}