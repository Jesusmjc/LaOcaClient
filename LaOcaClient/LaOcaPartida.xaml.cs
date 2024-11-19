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
    public partial class LaOcaPartida : Window
    {
        private readonly IServicioJugabilidad _servicioJugabilidad;
        private readonly Dictionary<int, Point> _posicionesCasillas;

        public LaOcaPartida()
        {
            InitializeComponent();
            _servicioJugabilidad = new ServicioJugabilidadClient();
            _posicionesCasillas = ObtenerPosicionesCasillas();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            MoverFicha(1);
        }

        private void MoverFicha(int pasos)
        {
            //_servicioJugabilidad.JugarTurno(pasos, sala.Codigo, nombreJugador);
            int nuevaPosicion = _servicioJugabilidad.ObtenerPosicionFicha();
            ActualizarInterfazGrafica(nuevaPosicion);
        }

        private void ActualizarInterfazGrafica(int nuevaPosicion)
        {
            if (_posicionesCasillas.TryGetValue(nuevaPosicion, out Point nuevaPosicionCanvas))
            {
                Canvas.SetLeft(FichaOcaAmarilla, nuevaPosicionCanvas.X);
                Canvas.SetTop(FichaOcaAmarilla, nuevaPosicionCanvas.Y);
                Console.WriteLine($"La ficha está ahora en la casilla {nuevaPosicion}");
            }
            else
            {
                Console.WriteLine($"Posición {nuevaPosicion} no encontrada en el tablero.");
            }
        }

        private Dictionary<int, Point> ObtenerPosicionesCasillas()
        {
            var posiciones = new Dictionary<int, Point>
            {
                { 0, new Point(10, 10) },
                { 1, new Point(-331, 155) },
                { 2, new Point(-255, 155) },
                { 3, new Point(-179, 158) },
                { 4, new Point(-102, 155) },
                { 5, new Point(-26, 155) },
                { 6, new Point(50, 155) },
                { 7, new Point(127, 155) },
                { 8, new Point(203, 155) },
                { 9, new Point(280, 155) },
                { 10, new Point(280, 95) },
                { 11, new Point(280, 35) },
                { 12, new Point(280, -25) },
                { 13, new Point(280, -85) },
                { 14, new Point(280, -145) },
                { 15, new Point(280, -205) },
                { 16, new Point(203, -205) },
                { 17, new Point(127, -205) },
                { 18, new Point(50, -205) },
                { 19, new Point(-26, -205) },
                { 20, new Point(-102, -205) },
                { 21, new Point(-179, -205) },
                { 22, new Point(-255, -205) },
                { 23, new Point(-331, -205) },
                { 24, new Point(-331, -145) },
                { 25, new Point(-331, -85) },
                { 26, new Point(-331, -25) },
                { 27, new Point(-331, 35) },
                { 28, new Point(-331, 95) },
                { 29, new Point(-255, 95) },
                { 30, new Point(-179, 95) },
                { 31, new Point(-102, 95) },
                { 32, new Point(-26, 95) },
                { 33, new Point(50, 95) },
                { 34, new Point(127, 95) },
                { 35, new Point(203, 95) },
                { 36, new Point(203, 35) },
                { 37, new Point(203, -25) },
                { 38, new Point(203, -85) },
                { 39, new Point(203, -145) },
                { 40, new Point(127, -145) },
                { 41, new Point(50, -145) },
                { 42, new Point(-26, -145) },
                { 43, new Point(-102, -145) },
                { 44, new Point(-179, -145) },
                { 45, new Point(-255, -145) },
                { 46, new Point(-255, -85) },
                { 47, new Point(-255, -25) },
                { 48, new Point(-255, 35) },
                { 49, new Point(-179, 35) },
                { 50, new Point(-102, 35) },
                { 51, new Point(-26, 35) },
                { 52, new Point(50, 35) },
                { 53, new Point(127, 35) },
                { 54, new Point(127, -25) },
                { 55, new Point(127, -85) },
                { 56, new Point(50, -85) },
                { 57, new Point(-26, -85) },
                { 58, new Point(-102, -85) },
                { 59, new Point(-179, -85) },
                { 60, new Point(-179, -25) },
                { 61, new Point(-102, -25) },
                { 62, new Point(-26, -25) },
                { 63, new Point(50, -25) },
            };
            return posiciones;
        }

        private void Btn_Regresar(object sender, RoutedEventArgs e)
        {

        }
    }
}