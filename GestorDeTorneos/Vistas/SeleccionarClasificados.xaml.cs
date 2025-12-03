using GestorDeTorneos.Data;
using System;
using System.Windows;

namespace GestorDeTorneos.Vistas
{
    /// <summary>
    /// Ventana que permite elegir cuántos equipos clasificados
    /// avanzarán desde el modo Liga hacia la eliminatoria directa.
    /// </summary>
    public partial class SeleccionarClasificados : Window
    {
        public SeleccionarClasificados()
        {
            InitializeComponent();
        }

        // --------------------------------------------------------------------
        // EVENTOS DE BOTONES
        // Cada botón solo manda llamar el método principal con un número fijo.
        // --------------------------------------------------------------------

        private void Btn4_Click(object sender, RoutedEventArgs e)
        {
            SeleccionarClasificadosSQL(4);   // Clasificar TOP 4
        }

        private void Btn8_Click(object sender, RoutedEventArgs e)
        {
            SeleccionarClasificadosSQL(8);   // Clasificar TOP 8
        }

        private void Btn16_Click(object sender, RoutedEventArgs e)
        {
            SeleccionarClasificadosSQL(16);  // Clasificar TOP 16
        }

        /// <summary>
        /// Propiedad opcional: guarda cuántos equipos se seleccionaron.
        /// Útil si después quieres comunicarlo a otra clase o usarlo en UI.
        /// </summary>
        

        // --------------------------------------------------------------------
        // MÉTODO PRINCIPAL: Seleccionar los mejores equipos desde SQL
        // --------------------------------------------------------------------
        private void SeleccionarClasificadosSQL(int cantidad)
        {
            // 1) Obtener equipos correctamente ordenados desde la BD.
            //    (ordenados por puntos, diferencia, GF y nombre)
            var equipos = DatabaseHelper.ObtenerEquiposOrdenados();

            // 2) Validar que haya suficientes equipos para la selección.
            if (equipos.Count < cantidad)
            {
                MessageBox.Show(
                    $"Solo hay {equipos.Count} equipos disponibles. " +
                    $"No se pueden seleccionar {cantidad}.",
                    "No hay suficientes equipos",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return;
            }

            // 3) Tomar los primeros 'cantidad' equipos de la tabla ordenada.
            //    Esto produce los clasificados reales.
            var clasificados = equipos.Take(cantidad).ToList();

            // 4) Crear ventana de EliminatoriaDirecta usando el constructor
            //    especial que activa el "modo automático" y recibe clasificados.
            EliminatoriaDirecta ventana = new EliminatoriaDirecta(clasificados);
            ventana.Show();

            // 5) Cerramos esta ventana porque ya no es necesaria.
            this.Close();
        }
    }
}