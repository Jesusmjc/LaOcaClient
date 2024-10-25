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
            cuenta = _servicioCuenta.ObtenerCuentaPorId(idCuenta);
            if (cuenta == null)
            {
                MessageBox.Show("Cuenta no encontrada.");
                Close();
            }
        }

        private void btnActualizarContraseña_Click(object sender, RoutedEventArgs e)
        {
            string contraseñaActual = tbContraseñaActual.Password;
            string nuevaContraseña = tbNuevaContraseña.Password;
            string confirmarNuevaContraseña = tbConfirmarNuevaContraseña.Password;

            if (nuevaContraseña != confirmarNuevaContraseña)
            {
                MessageBox.Show("Las nuevas contraseñas no coinciden.");
                return;
            }

            try
            {
                bool esContraseñaCorrecta = _servicioCuenta.VerificarContraseñaActual(cuenta.idCuenta, contraseñaActual);
                if (esContraseñaCorrecta)
                {
                    cuenta.contrasena = nuevaContraseña;
                    _servicioCuenta.ModificarCuenta(cuenta);
                    MessageBox.Show("Contraseña actualizada exitosamente.");
                    this.Close();
                }
                else
                {
                    MessageBox.Show("La contraseña actual es incorrecta.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al actualizar la contraseña: {ex.Message}");
            }
        }
    }
}