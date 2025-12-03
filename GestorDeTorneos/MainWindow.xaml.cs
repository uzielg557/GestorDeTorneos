using System.Windows;

namespace GestorDeTorneos
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        // ⚔ Eliminatoria Directa
        private void AbrirEliminatoria_Click(object sender, RoutedEventArgs e)
        {
            var ventana = new Vistas.EliminatoriaDirecta();

            // Cuando se cierre la ventana secundaria, reabrimos MainWindow
            ventana.Closed += (s, args) =>
            {
                this.Show();
            };

            ventana.Show();
            this.Hide(); // Ocultar en vez de cerrar
        }

        // ⚽ Liga
        private void AbrirLiga(object sender, RoutedEventArgs e)
        {
            var ventana = new Vistas.Liga();

            ventana.Closed += (s, args) =>
            {
                this.Show();
            };

            ventana.Show();
            this.Hide(); // Ocultar en vez de cerrar
        }
    }
}