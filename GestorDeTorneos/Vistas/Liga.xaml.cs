using GestorDeTorneos.Clases;
using GestorDeTorneos.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace GestorDeTorneos.Vistas
{
    public partial class Liga : Window
    {
        public Liga()
        {
            InitializeComponent();

            // Cargar equipos almacenados en SQL al iniciar
            CargarEquiposDesdeSQL();

            // Desactivar inputs de goles hasta que la liga se genere
            DeshabilitarControlesPartido();
        }

        // ============================================================
        //     AGREGAR / ELIMINAR EQUIPOS
        // ============================================================

        private void BtnAgregar_Click(object sender, RoutedEventArgs e)
        {
            string nombre = txtEquipo.Text.Trim();

            if (string.IsNullOrWhiteSpace(nombre))
            {
                MessageBox.Show("Debes ingresar un nombre de equipo.");
                return;
            }

            try
            {
                // INSERT INTO Equipos
                DatabaseHelper.ExecuteNonQuery(
                    "INSERT INTO Equipos (Nombre) VALUES (@nombre)",
                    cmd => cmd.Parameters.AddWithValue("@nombre", nombre)
                );

                listEquipos.Items.Add(nombre);
                txtEquipo.Clear();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al agregar equipo: " + ex.Message);
            }
        }

        private void BtnEliminar_Click(object sender, RoutedEventArgs e)
        {
            if (listEquipos.SelectedItem == null)
            {
                MessageBox.Show("Selecciona un equipo antes de eliminarlo.");
                return;
            }

            string nombre = listEquipos.SelectedItem.ToString();

            // Confirmación
            if (MessageBox.Show(
                $"¿Seguro que deseas eliminar el equipo '{nombre}'?",
                "Confirmación",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning) != MessageBoxResult.Yes)
                return;

            try
            {
                int equipoID = DatabaseHelper.GetEquipoID(nombre);

                if (equipoID == -1)
                {
                    MessageBox.Show("El equipo no existe en la base de datos.");
                    return;
                }

                // BORRAR todos los partidos donde participó
                DatabaseHelper.ExecuteNonQuery(
                    "DELETE FROM Partidos WHERE EquipoLocalID = @id OR EquipoVisitanteID = @id",
                    cmd => cmd.Parameters.AddWithValue("@id", equipoID)
                );

                // BORRAR equipo
                DatabaseHelper.ExecuteNonQuery(
                    "DELETE FROM Equipos WHERE EquipoID = @id",
                    cmd => cmd.Parameters.AddWithValue("@id", equipoID)
                );

                listEquipos.Items.Remove(nombre);
                MessageBox.Show("Equipo eliminado correctamente.");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al eliminar equipo: " + ex.Message);
            }
        }

        // ============================================================
        //     VARIABLES GENERALES DE LA LIGA
        // ============================================================

        private List<EquipoLiga> equiposLiga = new List<EquipoLiga>();
        private List<List<(string Local, string Visitante)>> jornadas = new();

        private List<int> jornadasSQL_IDs = new();
        private List<int> partidosSQL_IDs = new();

        private int jornadaActual = 0;
        private int partidoActual = 0;
        private int indiceSQL = 0;

        // ============================================================
        //                         INICIAR LIGA
        // ============================================================

        private void BtnIniciarLiga_Click(object sender, RoutedEventArgs e)
        {
            if (listEquipos.Items.Count < 2)
            {
                MessageBox.Show("Debes agregar al menos 2 equipos para iniciar la liga.");
                return;
            }

            // Limpiar estructuras en memoria
            equiposLiga.Clear();
            jornadas.Clear();
            partidosSQL_IDs.Clear();
            jornadasSQL_IDs.Clear();
            indiceSQL = 0;

            // Limpiar SQL para nueva liga
            DatabaseHelper.ExecuteNonQuery("DELETE FROM Partidos");
            DatabaseHelper.ExecuteNonQuery("DELETE FROM Jornadas");

            // Crear objetos EquipoLiga desde listbox
            foreach (var item in listEquipos.Items)
            {
                equiposLiga.Add(new EquipoLiga { Nombre = item.ToString() });
            }

            // Mostrar tabla vacía
            tablaPosiciones.ItemsSource = equiposLiga;

            // Generar calendario estilo Round Robin
            GenerarJornadas();

            // Guardar en SQL
            GuardarJornadasEnSQL();
            GuardarPartidosEnSQL();

            // Reiniciar índices
            jornadaActual = 0;
            partidoActual = 0;

            // Activar inputs de goles
            HabilitarControlesPartido();

            // Mostrar primer partido en pantalla
            MostrarPartidoActual();

            
        }

        // ------------------------------------------------------------
        //    Guardar jornadas en SQL (INSERT con OUTPUT)
        // ------------------------------------------------------------

        private void GuardarJornadasEnSQL()
        {
            for (int i = 0; i < jornadas.Count; i++)
            {
                int jornadaID = DatabaseHelper.InsertAndReturnID(
                    "INSERT INTO Jornadas (Numero) OUTPUT INSERTED.JornadaID VALUES (@num)",
                    cmd => cmd.Parameters.AddWithValue("@num", i + 1)
                );

                jornadasSQL_IDs.Add(jornadaID);
            }
        }

        // ------------------------------------------------------------
        //     Guardar partidos generados en SQL
        // ------------------------------------------------------------

        private void GuardarPartidosEnSQL()
        {
            for (int j = 0; j < jornadas.Count; j++)
            {
                int jornadaID = jornadasSQL_IDs[j];

                foreach (var p in jornadas[j])
                {
                    // No guardar "DESCANSA"
                    if (p.Local == "DESCANSA" || p.Visitante == "DESCANSA")
                        continue;

                    int localID = DatabaseHelper.GetEquipoID(p.Local);
                    int visitanteID = DatabaseHelper.GetEquipoID(p.Visitante);

                    int partidoID = DatabaseHelper.InsertAndReturnID(
                        "INSERT INTO Partidos (JornadaID, EquipoLocalID, EquipoVisitanteID) " +
                        "OUTPUT INSERTED.PartidoID VALUES (@j, @l, @v)",
                        cmd =>
                        {
                            cmd.Parameters.AddWithValue("@j", jornadaID);
                            cmd.Parameters.AddWithValue("@l", localID);
                            cmd.Parameters.AddWithValue("@v", visitanteID);
                        });

                    partidosSQL_IDs.Add(partidoID);
                }
            }
        }

        // ============================================================
        //        GENERAR JORNADAS (Round Robin profesional)
        // ============================================================

        private void GenerarJornadas()
        {
            jornadas.Clear();

            // Copiar nombres
            List<string> equipos = equiposLiga.Select(e => e.Nombre).ToList();

            // Si es impar, agregar DESCANSA
            if (equipos.Count % 2 != 0)
                equipos.Add("DESCANSA");

            int n = equipos.Count;
            int totalJornadas = n - 1;
            int partidosPorJornada = n / 2;

            List<string> rotacion = new(equipos);

            for (int j = 0; j < totalJornadas; j++)
            {
                List<(string, string)> jornada = new();

                for (int i = 0; i < partidosPorJornada; i++)
                {
                    jornada.Add((rotacion[i], rotacion[n - 1 - i]));
                }

                jornadas.Add(jornada);

                // Rotación estilo Round Robin
                string fijo = rotacion[0];
                rotacion.RemoveAt(0);

                string ultimo = rotacion[^1];
                rotacion.RemoveAt(rotacion.Count - 1);

                rotacion.Insert(0, ultimo);
                rotacion.Insert(0, fijo);
            }
        }

        // ============================================================
        //        MOSTRAR PARTIDO ACTUAL EN LA UI
        // ============================================================

        private void MostrarPartidoActual()
        {
            var partido = jornadas[jornadaActual][partidoActual];

            txtJornadaActual.Text = $"Jornada {jornadaActual + 1}";
            txtPartidoActual.Text = $"{partido.Local} vs {partido.Visitante}";

            // Caso de DESCANSO
            if (partido.Local == "DESCANSA" || partido.Visitante == "DESCANSA")
            {
                txtGolesLocal.Visibility = Visibility.Collapsed;
                txtGolesVisitante.Visibility = Visibility.Collapsed;
                lblGuion.Visibility = Visibility.Collapsed;
                btnRegistrarResultado.Visibility = Visibility.Collapsed;

                string equipoDescansa = partido.Local == "DESCANSA" ? partido.Visitante : partido.Local;

                MessageBox.Show($"⏸ {equipoDescansa} descansa esta jornada.");

                AvanzarSiguientePartido();
                return;
            }

            // Mostrar inputs de goles
            txtGolesLocal.Visibility = Visibility.Visible;
            txtGolesVisitante.Visibility = Visibility.Visible;
            lblGuion.Visibility = Visibility.Visible;
            btnRegistrarResultado.Visibility = Visibility.Visible;

            txtGolesLocal.Text = "";
            txtGolesVisitante.Text = "";
        }

        // ============================================================
        //                 REGISTRAR RESULTADO
        // ============================================================

        private void BtnRegistrarResultado_Click(object sender, RoutedEventArgs e)
        {
            RegistrarResultado();
        }

        private void RegistrarResultado()
        {
            // Validar números
            if (!int.TryParse(txtGolesLocal.Text, out int gl) ||
                !int.TryParse(txtGolesVisitante.Text, out int gv))
            {
                MessageBox.Show("Debes ingresar números válidos.");
                return;
            }

            var partido = jornadas[jornadaActual][partidoActual];

            // DESCANSO evita registrar goles
            if (partido.Local == "DESCANSA" || partido.Visitante == "DESCANSA")
            {
                AvanzarSiguientePartido();
                return;
            }

            // Buscar objetos equipo
            EquipoLiga local = equiposLiga.First(e => e.Nombre == partido.Local);
            EquipoLiga visitante = equiposLiga.First(e => e.Nombre == partido.Visitante);

            // Actualizar estadísticas
            local.PartidosJugados++;
            visitante.PartidosJugados++;

            local.GolesFavor += gl;
            local.GolesContra += gv;

            visitante.GolesFavor += gv;
            visitante.GolesContra += gl;

            if (gl > gv)
            {
                local.Ganados++;
                visitante.Perdidos++;
            }
            else if (gv > gl)
            {
                visitante.Ganados++;
                local.Perdidos++;
            }
            else
            {
                local.Empatados++;
                visitante.Empatados++;
            }

            // Guardar en SQL
            if (indiceSQL < partidosSQL_IDs.Count)
            {
                int partidoID = partidosSQL_IDs[indiceSQL];

                DatabaseHelper.ExecuteNonQuery(
                    "UPDATE Partidos SET GolesLocal=@gl, GolesVisitante=@gv, Jugado=1 WHERE PartidoID=@id",
                    cmd =>
                    {
                        cmd.Parameters.AddWithValue("@gl", gl);
                        cmd.Parameters.AddWithValue("@gv", gv);
                        cmd.Parameters.AddWithValue("@id", partidoID);
                    });

                indiceSQL++;
            }
            else
            {
                MessageBox.Show("⚠ Error interno: índice SQL fuera de rango.");
            }

            // Orden de tabla estilo PRO
            equiposLiga = equiposLiga
                .OrderByDescending(e => e.Puntos)
                .ThenByDescending(e => e.Diferencia)
                .ThenByDescending(e => e.GolesFavor)
                .ThenBy(e => e.Nombre)
                .ToList();

            // Refrescar DataGrid
            tablaPosiciones.ItemsSource = null;
            tablaPosiciones.ItemsSource = equiposLiga;

            // Limpiar inputs
            txtGolesLocal.Clear();
            txtGolesVisitante.Clear();

            AvanzarSiguientePartido();
        }

        // ============================================================
        //                 AVANZAR AL SIGUIENTE PARTIDO
        // ============================================================

        private void AvanzarSiguientePartido()
        {
            partidoActual++;

            // Si termina jornada, pasar a la siguiente
            if (partidoActual >= jornadas[jornadaActual].Count)
            {
                jornadaActual++;
                partidoActual = 0;
            }

            // Si ya no hay más jornadas → liga terminada
            if (jornadaActual >= jornadas.Count)
            {
                MessageBox.Show("🏆 ¡La liga ha terminado!");

                // Abrir selección de clasificados
                SeleccionarClasificados ventana = new SeleccionarClasificados();
                ventana.Show();

                this.Close();
                return;
            }

            MostrarPartidoActual();
        }

        // ============================================================
        //                         UTILIDADES
        // ============================================================

        private void CargarEquiposDesdeSQL()
        {
            listEquipos.Items.Clear();

            DatabaseHelper.ExecuteQuery(
                "SELECT Nombre FROM Equipos ORDER BY Nombre",
                cmd => { }, // sin parámetros
                reader =>
                {
                    while (reader.Read())
                        listEquipos.Items.Add(reader.GetString(0));
                });
        }

        private void DeshabilitarControlesPartido()
        {
            txtGolesLocal.IsEnabled = false;
            txtGolesVisitante.IsEnabled = false;
            lblGuion.IsEnabled = false;
            btnRegistrarResultado.IsEnabled = false;
        }

        private void HabilitarControlesPartido()
        {
            txtGolesLocal.IsEnabled = true;
            txtGolesVisitante.IsEnabled = true;
            lblGuion.IsEnabled = true;
            btnRegistrarResultado.IsEnabled = true;
        }
    }
}