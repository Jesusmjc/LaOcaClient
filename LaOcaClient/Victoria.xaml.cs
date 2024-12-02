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
                PrimerLugar.Text = jugadoresOrdenados[0].Key;
                CasillasPrimerLugar.Text = jugadoresOrdenados[0].Value.ToString();
            }

            if (jugadoresOrdenados.Count > 1)
            {
                SegundoLugar.Text = jugadoresOrdenados[1].Key;
                CasillasSegundoLugar.Text = jugadoresOrdenados[1].Value.ToString();
            }

            if (jugadoresOrdenados.Count > 2)
            {
                TercerLugar.Text = jugadoresOrdenados[2].Key;
                CasillasTercerLugar.Text = jugadoresOrdenados[2].Value.ToString();
            }

            if (jugadoresOrdenados.Count > 3)
            {
                CuartoLugar.Text = jugadoresOrdenados[3].Key;
                CasillasCuartoLugar.Text = jugadoresOrdenados[3].Value.ToString();
            }
        }


        private void RegresarAlInicio_Click(object sender, RoutedEventArgs e)
        {
            MenuPrincipal ventanaMenuPrincipal = new MenuPrincipal();
            ventanaMenuPrincipal.Show();
            this.Close();
        }
    }
}
