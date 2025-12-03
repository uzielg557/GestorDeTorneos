# 🏆 Gestor de Torneos

Aplicación de escritorio desarrollada en **C# + WPF + SQL Server**, diseñada para gestionar torneos en formato **Liga** y **Eliminatoria Directa**.  
Permite registrar equipos, generar jornadas automáticamente, llevar el control de resultados y clasificar a los mejores para luego disputar un formato de eliminación.

Este proyecto fue desarrollado por un **ingeniero mecatrónico con enfoque en software**, combinando lógica deportiva, programación estructurada y conexión a bases de datos.

---

## 📌 Características principales

### ✔ Modo **Liga**
- Registro de equipos (Agregar/Eliminar).
- Generación automática de *todas las jornadas* usando el método de rotación circular.
- Registro de goles por partido.
- Cálculo automático de:
  - Puntos
  - Diferencia de goles
  - Victorias, empates y derrotas
  - Goles a favor y en contra
- Tabla de posiciones ordenada como en ligas profesionales.
- Inserción y actualización de datos en SQL Server.
- Al finalizar, permite seleccionar:
  - Top 4  
  - Top 8  
  - Top 16  
  para continuar en modo eliminatoria.

---

### ✔ Modo **Eliminatoria Directa**
- Generación automática de llaves.
- Enfrenta al mejor clasificado vs el peor (1 vs último, 2 vs penúltimo…).
- Sistema de avance por rondas:
  - Cuartos
  - Semifinal
  - Final
- Manejo dinámico de ganadores y creación automática de la siguiente fase.
- Determina el campeón del torneo.

---

### ✔ Conexión a SQL Server

El sistema utiliza 3 tablas:

#### 🗂 Tabla **Equipos**
| EquipoID | Nombre |
|---------|--------|

#### 🗂 Tabla **Jornadas**
| JornadaID | Numero |

#### 🗂 Tabla **Partidos**
| PartidoID | JornadaID | EquipoLocalID | EquipoVisitanteID | GolesLocal | GolesVisitante | Jugado |

Incluye operaciones:
- INSERT  
- UPDATE  
- SELECT con JOINs  
- InsertAndReturnID (para capturar IDs generados automáticamente)

---

## 📐 Arquitectura del proyecto

```
GestorDeTorneos/
│
├── Clases/
│   └── EquipoLiga.cs
│
├── Data/
│   └── DatabaseHelper.cs
│
├── Vistas/
│   ├── Liga.xaml (+ .cs)
│   ├── EliminatoriaDirecta.xaml (+ .cs)
│   └── SeleccionarClasificados.xaml (+ .cs)
│
├── App.xaml
└── MainWindow.xaml
```

---

## 🧠 Lógica clave del proyecto

### 🎯 Generación de jornadas (Round Robin)
Implementa la rotación clásica:
- Si hay equipos impares → se agrega “DESCANSA”.
- Se rotan elementos para generar todas las combinaciones.
- Cada fecha contiene sus propios partidos.

### 🎯 Registro de resultados
Actualiza automáticamente:
- Puntos (3/1/0)
- GF / GC
- Diferencia
- Posiciones ordenadas en vivo

### 🎯 Cruce de eliminatoria
- Toma a los clasificados ordenados (Top 4/8/16).
- Los empareja así:
  - 1 vs último
  - 2 vs penúltimo  
  - …
- Cada ganador avanza y se genera la siguiente fase automáticamente.

---

## 🛠 Tecnologías utilizadas

- **C# (.NET)**
- **WPF (XAML)**
- **SQL Server**
- LINQ
- Programación orientada a objetos
- Arquitectura por capas

---

## ▶️ ¿Cómo ejecutar el proyecto?

### 1️⃣ Clonar el repositorio
```bash
git clone https://github.com/uzielg557/GestorDeTorneos.git
```

### 2️⃣ Abrir en **Visual Studio**

### 3️⃣ Configurar SQL Server
Crear una base de datos llamada `TorneosDB`.

Luego crear las tablas:

```sql
CREATE TABLE Equipos (
    EquipoID INT PRIMARY KEY IDENTITY(1,1),
    Nombre VARCHAR(50) NOT NULL
);

CREATE TABLE Jornadas (
    JornadaID INT PRIMARY KEY IDENTITY(1,1),
    Numero INT NOT NULL
);

CREATE TABLE Partidos (
    PartidoID INT PRIMARY KEY IDENTITY(1,1),
    JornadaID INT NOT NULL,
    EquipoLocalID INT NOT NULL,
    EquipoVisitanteID INT NOT NULL,
    GolesLocal INT NULL,
    GolesVisitante INT NULL,
    Jugado BIT DEFAULT 0
);
```

### 4️⃣ Ajustar cadena de conexión
Modificar en `DatabaseHelper.cs`:

```csharp
private static readonly string connectionString =
    "Server=TU_SERVIDOR;Database=TorneosDB;Trusted_Connection=True;";
```

### 5️⃣ Ejecutar la aplicación
Compilar y correr desde Visual Studio.

---

## 🏅 Autor

Proyecto desarrollado por un **ingeniero mecatrónico con enfoque en software**, apasionado por:

- Lógica deportiva  
- Programación backend  
- Bases de datos  
- Aplicaciones de escritorio  
- Interfaces WPF  

---
