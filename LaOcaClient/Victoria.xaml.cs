using System.Collections.Generic;
using System.Windows;

namespace LaOcaClient
{
    public partial class Victoria : Window
    {
        public Victoria(List<KeyValuePair<string, int>> jugadoresOrdenados)
        {
            InitializeComponent();

            if (jugadoresOrdenados.Count > 0)
            {
                tbckPrimerLugar.Text = jugadoresOrdenados[0].Key;
                tbckCasillasPrimerLugar.Text = jugadoresOrdenados[0].Value.ToString();
            }

            if (jugadoresOrdenados.Count > 1)
            {
                tbckSegundoLugar.Text = jugadoresOrdenados[1].Key;
                tbckCasillasSegundoLugar.Text = jugadoresOrdenados[1].Value.ToString();
            }

            if (jugadoresOrdenados.Count > 2)
            {
                tbckTercerLugar.Text = jugadoresOrdenados[2].Key;
                tbckCasillasTercerLugar.Text = jugadoresOrdenados[2].Value.ToString();
            }

            if (jugadoresOrdenados.Count > 3)
            {
                tbckCuartoLugar.Text = jugadoresOrdenados[3].Key;
                tbckCasillasCuartoLugar.Text = jugadoresOrdenados[3].Value.ToString();
            }
        }

        private void RegresarAlInicio(object sender, RoutedEventArgs e)
        {
            MenuPrincipal ventanaMenuPrincipal = new MenuPrincipal();
            ventanaMenuPrincipal.Show();
            this.Close();
        }
    }
}