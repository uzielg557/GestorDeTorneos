using System;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace GestorDeTorneos.Data
{
    /// <summary>
    /// Clase estática que maneja toda la comunicación con SQL Server.
    /// Ofrece métodos para ejecutar consultas, comandos y obtener IDs generados.
    /// </summary>
    public static class DatabaseHelper
    {
        /// <summary>
        /// Cadena de conexión hacia SQL Server.
        /// NOTA: En producción debe moverse a un archivo de configuración.
        /// </summary>
        private static readonly string connectionString =
            "Server=.;Database=TorneosDB;Trusted_Connection=True;";

        // ---------------------------------------------------------------------
        //  MÉTODOS GENERALES
        // ---------------------------------------------------------------------

        /// <summary>
        /// Ejecuta comandos tipo INSERT, UPDATE o DELETE.
        /// No devuelve resultados.
        /// </summary>
        /// <param name="query">Consulta SQL a ejecutar.</param>
        /// <param name="parameterSetter">Acción opcional para agregar parámetros al SqlCommand.</param>
        public static void ExecuteNonQuery(string query, Action<SqlCommand> parameterSetter = null)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    // Agregar parámetros si fueron enviados
                    parameterSetter?.Invoke(cmd);

                    // Ejecutar comando sin retorno
                    cmd.ExecuteNonQuery();
                }
            }
        }

        /// <summary>
        /// Ejecuta consultas SELECT y permite procesar cada fila con un SqlDataReader.
        /// </summary>
        /// <param name="query">Consulta SELECT a ejecutar.</param>
        /// <param name="parameterSetter">Método que agrega parámetros opcionales.</param>
        /// <param name="readerHandler">Método que procesa los datos devueltos por el lector.</param>
        public static void ExecuteQuery(string query, Action<SqlCommand> parameterSetter, Action<SqlDataReader> readerHandler)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    // Establecer parámetros si existen
                    parameterSetter?.Invoke(cmd);

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        // El programador define cómo manejar cada fila
                        readerHandler(reader);
                    }
                }
            }
        }

        // ---------------------------------------------------------------------
        //  OBTENER INFORMACIÓN ESPECÍFICA
        // ---------------------------------------------------------------------

        /// <summary>
        /// Obtiene el ID (EquipoID) de un equipo por su nombre.
        /// Devuelve -1 si no se encuentra.
        /// </summary>
        public static int GetEquipoID(string nombre)
        {
            int id = -1;

            ExecuteQuery(
                "SELECT EquipoID FROM Equipos WHERE Nombre = @nombre",
                cmd =>
                {
                    cmd.Parameters.AddWithValue("@nombre", nombre);
                },
                reader =>
                {
                    // Si encuentra la fila → obtiene el ID
                    if (reader.Read())
                    {
                        id = reader.GetInt32(0);
                    }
                }
            );

            return id;
        }

        /// <summary>
        /// Ejecuta un INSERT con OUTPUT INSERTED.X y devuelve el valor generado,
        /// normalmente el ID autoincremental.
        /// </summary>
        public static int InsertAndReturnID(string query, Action<SqlCommand> parameterSetter)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    parameterSetter?.Invoke(cmd);

                    // Ejecuta y devuelve el valor retornado por OUTPUT INSERTED
                    return (int)cmd.ExecuteScalar();
                }
            }
        }

        // ---------------------------------------------------------------------
        //  OBTENER EQUIPOS ORDENADOS SEGÚN TABLA REAL DE FÚTBOL
        // ---------------------------------------------------------------------

        /// <summary>
        /// Obtiene una lista de equipos ordenada por:
        /// 1) Puntos
        /// 2) Diferencia de goles
        /// 3) Goles a favor
        /// 4) Nombre (alfabético)
        ///
        /// Útil para seleccionar clasificados para eliminatoria.
        /// </summary>
        /// <returns>Lista de equipos con (Nombre, EquipoID).</returns>
        public static List<(string Nombre, int EquipoID)> ObtenerEquiposOrdenados()
        {
            var lista = new List<(string Nombre, int EquipoID)>();

            string query = @"
        SELECT
            e.EquipoID,
            e.Nombre,

            -- Puntos (3 victoria, 1 empate)
            COALESCE(SUM(
                CASE
                    WHEN p.EquipoLocalID = e.EquipoID AND p.GolesLocal > p.GolesVisitante THEN 3
                    WHEN p.EquipoVisitanteID = e.EquipoID AND p.GolesVisitante > p.GolesLocal THEN 3
                    WHEN p.GolesLocal = p.GolesVisitante 
                         AND (p.EquipoLocalID = e.EquipoID OR p.EquipoVisitanteID = e.EquipoID) THEN 1
                    ELSE 0
                END
            ), 0) AS Puntos,

            -- Diferencia de goles (GF – GC)
            COALESCE(SUM(
                CASE
                    WHEN p.EquipoLocalID = e.EquipoID THEN p.GolesLocal - p.GolesVisitante
                    WHEN p.EquipoVisitanteID = e.EquipoID THEN p.GolesVisitante - p.GolesLocal
                    ELSE 0
                END
            ), 0) AS Diferencia,

            -- Goles a favor
            COALESCE(SUM(
                CASE
                    WHEN p.EquipoLocalID = e.EquipoID THEN p.GolesLocal
                    WHEN p.EquipoVisitanteID = e.EquipoID THEN p.GolesVisitante
                    ELSE 0
                END
            ), 0) AS GolesFavor

        FROM Equipos e
        LEFT JOIN Partidos p
            ON p.Jugado = 1
           AND (p.EquipoLocalID = e.EquipoID OR p.EquipoVisitanteID = e.EquipoID)

        GROUP BY e.EquipoID, e.Nombre

        ORDER BY 
            Puntos DESC,
            Diferencia DESC,
            GolesFavor DESC,
            e.Nombre ASC;
    ";

            // Ejecutar consulta y armar la lista
            ExecuteQuery(
                query,
                cmd => { }, // No hay parámetros
                reader =>
                {
                    while (reader.Read())
                    {
                        int equipoID = reader.GetInt32(0);
                        string nombre = reader.GetString(1);

                        lista.Add((nombre, equipoID));
                    }
                }
            );

            return lista;
        }
    }
}