# 🏆 Gestor de Torneos — C# + WPF + SQL Server

Aplicación de escritorio desarrollada en **C# (WPF)** con persistencia en **SQL Server**, diseñada para gestionar torneos en formato **Liga** y **Eliminatoria Directa**.

Permite:

- Registrar equipos  
- Generar jornadas automáticamente  
- Registrar resultados  
- Calcular tabla de posiciones  
- Avanzar a fases eliminatorias (Top 4 / 8 / 16)  
- Determinar un campeón  

Este proyecto combina lógica de competencia deportiva, estructuras de datos, WPF/MVVM básico y operaciones SQL reales.

---

## 📸 Vista principal

![MainWindow](https://raw.githubusercontent.com/uzielg557/GestorDeTorneos/main/%7B1EA29568-69E2-4EC8-80CE-354B54763928%7D.png)

---

# 📌 Funcionalidades principales

---

# ⚽ Modo Liga

- Gestión de equipos (Agregar / Editar / Eliminar)  
- Generación automática de jornadas con algoritmo **Round Robin**  
- Registro de goles por partido  
- Cálculo automático de:
  - Puntos  
  - Victorias, empates y derrotas  
  - Goles a favor y en contra  
  - Diferencia de goles  
- Tabla de posiciones profesional  
- Clasificación a Top 4 / Top 8 / Top 16  

---

## 🖼️ Capturas — Modo Liga

### ✔ Equipos cargados
![Equipos](https://raw.githubusercontent.com/uzielg557/GestorDeTorneos/main/%7B5230773B-F682-40BE-9964-27EB686CA5A8%7D.png)

### ✔ Liga generada
![Liga generada](https://raw.githubusercontent.com/uzielg557/GestorDeTorneos/main/%7BA87D9F98-0BBE-416D-AAA5-1177636A2ADB%7D.png)

### ✔ "Descansa" automático
![Descanso](https://raw.githubusercontent.com/uzielg557/GestorDeTorneos/main/%7B661C21E8-051F-4A80-9178-49F8BDA8AC36%7D.png)

### ✔ Liga finalizada
![Liga finalizada](https://raw.githubusercontent.com/uzielg557/GestorDeTorneos/main/%7BCB32A77D-B501-4504-B87A-27399A3F08B5%7D.png)

### ✔ Selección de clasificados
![Repechaje](https://raw.githubusercontent.com/uzielg557/GestorDeTorneos/main/%7B64BE3037-4C7F-434A-A6BC-9B27DA0A061D%7D.png)

### ✔ Llaves desde la Liga
![Llaves liga](https://raw.githubusercontent.com/uzielg557/GestorDeTorneos/main/%7B978CBBF0-3180-45BC-B378-610944371CF0%7D.png)

### ✔ Campeón
![Campeón liga](https://raw.githubusercontent.com/uzielg557/GestorDeTorneos/main/%7B331C546A-1C49-4BC1-98F9-B597AC7FF984%7D.png)

---

# 🔥 Modo Eliminatoria Directa

- Emparejamiento 1 vs último, 2 vs penúltimo...  
- Registro de resultados por fase  
- Avance automático de ganadores  
- Rondas: Cuartos → Semifinal → Final  
- Muestra al campeón  

---

## 🖼️ Capturas — Modo Eliminatoria

### ✔ Pantalla vacía
![Empty eliminatoria](https://raw.githubusercontent.com/uzielg557/GestorDeTorneos/main/%7B4A165459-D852-4238-8DA7-FF24BF9A43D5%7D.png)

### ✔ Equipos listos
![Equipos eliminatoria](https://raw.githubusercontent.com/uzielg557/GestorDeTorneos/main/%7BD9AED9A0-988D-40DA-A92D-36B0072B126A%7D.png)

### ✔ Llaves generadas
![Llaves](https://raw.githubusercontent.com/uzielg557/GestorDeTorneos/main/%7B1222B026-DEFA-4321-AA42-A678F43944AC%7D.png)

### ✔ Campeón final
![Campeón](https://raw.githubusercontent.com/uzielg557/GestorDeTorneos/main/%7B73810B48-902D-4763-AC68-9DA2B240D424%7D.png)

---

# 🗄️ Conexión a SQL Server

## Tablas utilizadas

### 🧩 Equipos
| EquipoID | Nombre |

### 🧩 Jornadas
| JornadaID | Numero |

### 🧩 Partidos
| PartidoID | JornadaID | EquipoLocalID | EquipoVisitanteID | GolesLocal | GolesVisitante | Jugado |

## Operaciones SQL utilizadas

- INSERT  
- UPDATE  
- SELECT con JOIN  
- InsertAndReturnID  

---

# 🧱 Arquitectura del proyecto

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
│   ├── Liga.xaml / Liga.xaml.cs
│   ├── EliminatoriaDirecta.xaml / EliminatoriaDirecta.xaml.cs
│   └── SeleccionarClasificados.xaml / SeleccionarClasificados.xaml.cs
│
├── App.xaml
└── MainWindow.xaml
```

---

# 🧠 Lógica clave

## 🔄 Round Robin
- Agrega “Descansa” si hay equipos impares  
- Rotación circular  
- Genera todas las jornadas  

## 📝 Registro de resultados
- Tabla recalculada automáticamente  
- Reglas del fútbol  
- Orden por puntos → diferencia → goles  

## 🎯 Eliminatoria
- Cruces según ranking  
- Avance por rondas  
- Campeón final  

---

# 🛠️ Tecnologías utilizadas

- C#  
- WPF  
- SQL Server  
- ADO.NET  
- LINQ  
- Programación orientada a objetos  

---

# ▶️ Cómo ejecutar el proyecto

## 1️⃣ Clonar repositorio
```
git clone https://github.com/uzielg557/GestorDeTorneos.git
```

## 2️⃣ Abrir en Visual Studio

## 3️⃣ Crear base de datos

```
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

## 4️⃣ Ajustar cadena de conexión
Editar en `DatabaseHelper.cs`:

```
"Server=TU_SERVIDOR;Database=TorneosDB;Trusted_Connection=True;"
```

## 5️⃣ Ejecutar  
Presiona **F5** en Visual Studio.

---

# 🏅 Autor

**Víctor Uziel García Jácome**  
Ingeniero Mecatrónico con enfoque en software.

---

# ⭐ ¿Te gustó el proyecto?

**¡Dale una estrella ⭐ en GitHub!**
