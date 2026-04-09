# 🦎 INVASIÓN DE SALAMANQUEJAS
## En casa de la abuela

![Unity](https://img.shields.io/badge/Unity-2022.3.62f2-black?logo=unity)
![C#](https://img.shields.io/badge/C%23-Scripts-blue?logo=csharp)
![Materia](https://img.shields.io/badge/ISW--414-Programación%20de%20Videojuegos-red)

Videojuego 2D de acción con temática dominicana desarrollado en **Unity 2022** para la materia **Programación de Videojuegos (ISW-414)** de la Universidad Central del Este.

---

## 👤 Autor

| Campo | Dato |
|---|---|
| **Nombre** | Diego Alberto Castillo Zorrilla |
| **Matrícula** | 2023-0733 |
| **Universidad** | Universidad Central del Este (UCE) |
| **Profesor** | Ivan Zorrilla |
| **Materia** | ISW-414 Programación de Videojuegos |

---

## 🎮 Descripción

En una típica tarde dominicana, la abuela está descansando en su sala cuando una invasión masiva de salamanquejas toma la casa. Diego, armado con sus lanzas, debe eliminarlas antes de que escapen o toquen a su abuela.

El juego está ambientado en una casa dominicana auténtica con:
- Bandera dominicana en la pared
- Televisor viejo, abanico de techo y cuadros religiosos
- Moto Suzuki en el patio
- Música estilo merengue/caribeño

---

## 🕹️ Controles

| Acción | Control |
|---|---|
| Mover al jugador | `W A S D` o Flechas del teclado |
| Disparar lanza | `Clic izquierdo` (apunta hacia el cursor) |
| Pausar el juego | `ESC` |

---

## 🦎 Tipos de Salamanquejas

| Tipo | Color | Puntos | Especial |
|---|---|---|---|
| Normal | Amarilla | 10 RD$ | — |
| Rápida | Azul | 25 RD$ | Desaparece rápido |
| Resistente | Naranja | 15 RD$ | Requiere 2 golpes |
| Dorada | Dorada | 50 RD$ | Muy escasa |

---

## 🏠 Niveles

| Nivel | Escenario | Objetivo | Lanzas | Dificultad |
|---|---|---|---|---|
| 1 | Sala de la abuela | 15 salamanquejas | 20 | Normal |
| 2 | Patio de la casa | 20 salamanquejas | 30 | Difícil |

---

## ⚙️ Características

- ✅ Menú principal con 4 opciones (Jugar, Opciones, Personajes, Salir)
- ✅ Selección de personaje (Diego / Railyn)
- ✅ Panel de opciones con control de volumen y silencio
- ✅ Sistema de vidas (3 vidas por partida)
- ✅ Sistema de puntuación en RD$ acumulado entre niveles
- ✅ 3 Oleadas de dificultad progresiva por nivel
- ✅ Salamanquejas salen de los hoyos del piso
- ✅ La abuela sigue al jugador por el escenario
- ✅ Puerta se desbloquea al completar el objetivo
- ✅ Transición con fade a negro entre niveles
- ✅ Panel de pausa con controles de audio
- ✅ Pantalla de Game Over con record guardado
- ✅ Música y efectos de sonido dominicanos
- ✅ 2 escenarios: Sala y Patio

---

## 🗂️ Estructura del Repositorio

```
CazadorDeSalamanquejas/
├── Assets/
│   ├── Audio/          # Música y efectos de sonido
│   ├── Prefabs/        # Salamanqueja y Lanza
│   ├── Scenes/         # MainMenu, Level_Sala, Level_Patio, GameOver
│   ├── Scripts/        # 12 scripts C#
│   └── Sprites/        # Personajes, lagartos, fondos, UI
├── Documentacion/
│   └── CazadorSalamanquejas_Castillo_2023-0733.pdf
├── .gitignore
└── README.md
```

---

## 📜 Scripts principales

| Script | Descripción |
|---|---|
| `GameManager.cs` | Controla estado del juego, puntuación, vidas y oleadas |
| `PlayerController.cs` | Movimiento, animaciones y disparo del jugador |
| `Salamanqueja.cs` | Comportamiento y tipos de enemigos |
| `Spawner.cs` | Object Pool y spawn de salamanquejas |
| `UIManager.cs` | HUD, mensajes, pausa y flash de daño |
| `AudioManager.cs` | Música y efectos de sonido persistentes entre escenas |
| `LanzaProyectil.cs` | Movimiento y detección de impacto del proyectil |
| `TransicionNivel.cs` | Fade a negro entre niveles |
| `MainMenuController.cs` | Navegación del menú y selección de personaje |
| `GameOverController.cs` | Pantalla de Game Over y récord |

---

## 🎥 Video Demo

*(Próximamente — agregar link de YouTube aquí)*

---

## 📄 Documentación

La documentación técnica completa se encuentra en:
`Documentacion/CazadorSalamanquejas_Castillo_2023-0733.pdf`

Incluye: descripción, mecánicas, diseño técnico, scripts, assets, screenshots y conclusiones.

---

## 🛠️ Tecnologías

- **Motor:** Unity 2022.3.62f2
- **Lenguaje:** C#
- **UI:** TextMesh Pro
- **Arte:** Generado con IA (sprites propios)
- **Audio:** Generado con IA (Gemini)
- **Control de versiones:** Git / GitHub

---

*ISW-414 Programación de Videojuegos · Universidad Central del Este · 2026*
