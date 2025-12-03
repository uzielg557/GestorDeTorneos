using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace GestorDeTorneos.Vistas
{
    /// <summary>
    /// Ventana para gestionar un torneo de Eliminatoria Directa.
    /// Puede funcionar en dos modos:
    /// 1) Modo manual: El usuario registra equipos.
    /// 2) Modo automático: Recibe equipos desde el módulo Liga.
    /// </summary>
    public partial class EliminatoriaDirecta : Window
    {
        // =============================================================
        //  SECCIÓN 1 — VARIABLES PRINCIPALES DEL TORNEO
        // =============================================================

        /// <summary>
        /// Indica si la ventana está en modo automático (Liga → Eliminatoria Final).
        /// </summary>
        private bool modoAutomatico = false;

        /// <summary>
        /// Lista de clasificados provenientes de la Liga (Nombre, EquipoID).
        /// </summary>
        public List<(string Nombre, int EquipoID)> Clasificados { get; set; }

        // 🔹 Variables internas para manejar el flujo del torneo
        private List<string> ganadoresRonda = new List<string>();
        private List<string> equiposActuales = new List<string>();

        private int totalPartidos = 0;
        private int partidosJugados = 0;
        private int rondaActual = 1;



        // =============================================================
        //  SECCIÓN 2 — CONSTRUCTORES
        // =============================================================

        /// <summary>
        /// Constructor para el modo automático (cuando viene de la Liga).
        /// Recibe los equipos ya ordenados y oculta el panel de registro.
        /// </summary>
        public EliminatoriaDirecta(List<(string Nombre, int EquipoID)> clasificados)
        {
            InitializeComponent();

            modoAutomatico = true;
            Clasificados = clasificados;

            ConfigurarModoAutomatico();
        }

        /// <summary>
        /// Constructor para el modo manual.
        /// </summary>
        public EliminatoriaDirecta()
        {
            InitializeComponent();
            modoAutomatico = false;
        }



        // =============================================================
        //  SECCIÓN 3 — REGISTRO MANUAL DE EQUIPOS
        // =============================================================

        /// <summary>
        /// Agrega un equipo a la lista validando nombre vacío y duplicados.
        /// </summary>
        private void BtnAgregar_Click(object sender, RoutedEventArgs e)
        {
            string nombre = txtEquipo.Text.Trim();

            if (string.IsNullOrEmpty(nombre))
            {
                MessageBox.Show("Por favor escribe un nombre antes de agregar.");
                return;
            }

            // Validar duplicado
            bool repetido = listEquipos.Items
                .Cast<string>()
                .Any(equipo => equipo.Equals(nombre, StringComparison.OrdinalIgnoreCase));

            if (repetido)
            {
                MessageBox.Show("Equipo repetido, escribe un nombre diferente.");
                return;
            }

            listEquipos.Items.Add(nombre.Normalize());
            txtEquipo.Clear();
        }

        /// <summary>
        /// Elimina el equipo seleccionado en la lista.
        /// </summary>
        private void BtnEliminar_Click(object sender, RoutedEventArgs e)
        {
            if (listEquipos.SelectedItem == null)
            {
                MessageBox.Show("Selecciona un equipo antes de eliminarlo.");
                return;
            }

            listEquipos.Items.Remove(listEquipos.SelectedItem);
        }



        // =============================================================
        //  SECCIÓN 4 — INICIAR TORNEO (MODO MANUAL)
        // =============================================================

        /// <summary>
        /// Prepara el torneo:
        /// - Obtiene equipos
        /// - Ajusta a potencia de 2 agregando 'Bye'
        /// - Mezcla equipos
        /// - Configura variables internas
        /// - Muestra la primera ronda
        /// </summary>
        private void BtnEmpezar_Click(object sender, RoutedEventArgs e)
        {
            int cantidad = listEquipos.Items.Count;

            if (cantidad < 2)
            {
                MessageBox.Show("Debes agregar al menos 2 equipos para comenzar.");
                return;
            }

            // Convertir ListBox → lista de strings
            List<string> equipos = listEquipos.Items.Cast<string>().ToList();

            // Calcular siguiente potencia de 2
            int siguientePotencia = 1;
            while (siguientePotencia < equipos.Count)
                siguientePotencia *= 2;

            // Agregar BYE si faltan equipos
            int faltan = siguientePotencia - equipos.Count;
            for (int i = 0; i < faltan; i++)
                equipos.Add("Bye");

            MessageBox.Show($"Se generarán {faltan} BYE para completar la llave.",
                "Información", MessageBoxButton.OK, MessageBoxImage.Information);

            // Mezclar equipos
            Random rnd = new Random();
            equiposActuales = equipos.OrderBy(x => rnd.Next()).ToList();

            // Reiniciar variables internas
            rondaActual = 1;
            partidosJugados = 0;
            ganadoresRonda.Clear();
            totalPartidos = equiposActuales.Count / 2;

            // Limpiar zona visual
            ZonaLlaves.Children.Clear();

            // Mostrar primera ronda
            MostrarRonda();
        }



        // =============================================================
        //  SECCIÓN 5 — LÓGICA PRINCIPAL DEL TORNEO
        // =============================================================

        /// <summary>
        /// Genera visualmente los partidos de la ronda actual.
        /// </summary>
        private void MostrarRonda()
        {
            ZonaLlaves.Children.Clear();

            // ▬▬▬ TÍTULO ▬▬▬
            ZonaLlaves.Children.Add(new TextBlock
            {
                Text = $"Ronda {rondaActual}",
                FontSize = 20,
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(0, 20, 0, 10),
                HorizontalAlignment = HorizontalAlignment.Center
            });

            // ▬▬▬ GENERAR DUELOS ▬▬▬
            for (int i = 0; i < equiposActuales.Count; i += 2)
            {
                string A = equiposActuales[i];
                string B = equiposActuales[i + 1];

                Border caja = CrearCajaDePartido();

                // Caso BYE
                if (A == "Bye" || B == "Bye")
                {
                    string pasa = (A == "Bye") ? B : A;

                    caja.Child = new TextBlock
                    {
                        Text = $"{pasa} pasa directamente",
                        FontSize = 16,
                        HorizontalAlignment = HorizontalAlignment.Center
                    };

                    ganadoresRonda.Add(pasa);
                    partidosJugados++;
                    ZonaLlaves.Children.Add(caja);
                    continue;
                }

                // Duelo normal
                StackPanel panel = CrearPanelDeDuelos(A, B, caja);
                ZonaLlaves.Children.Add(caja);
            }
        }


        /// <summary>
        /// Lógica para elegir al ganador de un partido.
        /// </summary>
        private void SeleccionarGanador(string ganador, Button botonGanador, Button botonPerdedor)
        {
            // Colores visuales
            botonGanador.Background = Brushes.LightGreen;
            botonPerdedor.Background = Brushes.LightCoral;

            // Desactivar botones
            botonGanador.IsEnabled = false;
            botonPerdedor.IsEnabled = false;

            // Registrar ganador
            ganadoresRonda.Add(ganador);
            partidosJugados++;

            // Si la ronda terminó
            if (partidosJugados == totalPartidos)
            {
                // Si solo queda uno → campeón
                if (ganadoresRonda.Count == 1)
                {
                    MostrarCampeon(ganadoresRonda[0]);
                }
                else
                {
                    MessageBox.Show($"Ronda {rondaActual} finalizada.");
                    PrepararSiguienteRonda();
                }
            }
        }

        /// <summary>
        /// Configura los equipos para la siguiente ronda.
        /// </summary>
        private void PrepararSiguienteRonda()
        {
            equiposActuales = new List<string>(ganadoresRonda);
            ganadoresRonda.Clear();

            partidosJugados = 0;
            totalPartidos = equiposActuales.Count / 2;
            rondaActual++;

            MostrarRonda();
        }


        /// <summary>
        /// Muestra visualmente al campeón del torneo.
        /// </summary>
        private void MostrarCampeon(string campeon)
        {
            ZonaLlaves.Children.Clear();

            ZonaLlaves.Children.Add(new TextBlock
            {
                Text = $"🏆 Campeón: {campeon}",
                FontSize = 26,
                FontWeight = FontWeights.Bold,
                FontFamily = new FontFamily("Orbitron"),
                Foreground = new SolidColorBrush(Color.FromRgb(47, 255, 136)),
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 40, 0, 0)
            });
        }



        // =============================================================
        //  SECCIÓN 6 — MODO AUTOMÁTICO (CLASIFICADOS DESDE LIGA)
        // =============================================================

        /// <summary>
        /// Oculta la UI de registro manual y genera llaves automáticamente.
        /// </summary>
        private void ConfigurarModoAutomatico()
        {
            panelRegistro.Visibility = Visibility.Collapsed;
            listEquipos.Visibility = Visibility.Collapsed;
            BtnEmpezar.Visibility = Visibility.Collapsed;

            CargarLlavesAutomaticas();
        }

        /// <summary>
        /// Genera los duelos en base al orden de clasificación:
        /// 1 vs último, 2 vs penúltimo, etc.
        /// </summary>
        private void CargarLlavesAutomaticas()
        {
            ZonaLlaves.Children.Clear();

            var ordenados = Clasificados.ToList();
            List<(string Local, string Visitante)> cruces = new();

            int izquierda = 0;
            int derecha = ordenados.Count - 1;

            // Asignación estándar: 1 vs N, 2 vs N-1, ...
            while (izquierda < derecha)
            {
                cruces.Add((ordenados[izquierda].Nombre,
                            ordenados[derecha].Nombre));
                izquierda++;
                derecha--;
            }

            // Dibujar visualmente
            foreach (var duelo in cruces)
            {
                Border caja = CrearCajaDePartido();

                StackPanel panel = new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    HorizontalAlignment = HorizontalAlignment.Center
                };

                Button btnA = CrearBotonEquipo(duelo.Local);
                Button btnB = CrearBotonEquipo(duelo.Visitante);

                panel.Children.Add(btnA);
                panel.Children.Add(new TextBlock { Text = "VS", Margin = new Thickness(6) });
                panel.Children.Add(btnB);

                caja.Child = panel;
                ZonaLlaves.Children.Add(caja);

                btnA.Click += (s, e) => SeleccionarGanador(duelo.Local, btnA, btnB);
                btnB.Click += (s, e) => SeleccionarGanador(duelo.Visitante, btnB, btnA);
            }

            // Preparar variables internas
            equiposActuales = ordenados.Select(e => e.Nombre).ToList();
            totalPartidos = cruces.Count;
            partidosJugados = 0;
            rondaActual = 1;
            ganadoresRonda.Clear();
        }



        // =============================================================
        //  SECCIÓN 7 — MÉTODOS AUXILIARES PARA CREAR UI
        // =============================================================

        private Border CrearCajaDePartido()
        {
            return new Border
            {
                BorderBrush = new SolidColorBrush(Color.FromRgb(47, 255, 136)),
                BorderThickness = new Thickness(1.5),
                Margin = new Thickness(5),
                Padding = new Thickness(10),
                CornerRadius = new CornerRadius(10),
                Background = Brushes.Transparent,
                HorizontalAlignment = HorizontalAlignment.Center,
                Width = 280
            };
        }

        private StackPanel CrearPanelDeDuelos(string equipoA, string equipoB, Border caja)
        {
            StackPanel panel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(15, 5, 15, 5)
            };

            Button btnA = CrearBotonEquipo(equipoA);
            Button btnB = CrearBotonEquipo(equipoB);

            panel.Children.Add(btnA);
            panel.Children.Add(new TextBlock { Text = "VS", Margin = new Thickness(5), VerticalAlignment = VerticalAlignment.Center });
            panel.Children.Add(btnB);

            caja.Child = panel;

            // Eventos
            btnA.Click += (s, e) => SeleccionarGanador(equipoA, btnA, btnB);
            btnB.Click += (s, e) => SeleccionarGanador(equipoB, btnB, btnA);

            return panel;
        }

        private Button CrearBotonEquipo(string nombre)
        {
            return new Button
            {
                Content = nombre,
                Tag = nombre,
                Style = (Style)FindResource("BotonTorneo")
            };
        }
    }
}