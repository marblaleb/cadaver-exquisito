# CadaverExquisito.App — Diseño Técnico

**Fecha:** 2026-05-18  
**Estado:** Aprobado

---

## 1. Resumen

App móvil (iOS + Android) de escritura colaborativa asíncrona tipo "Cadáver Exquisito". Los usuarios crean historias en cola comunitaria: cada participante escribe un fragmento viendo solo el anterior, sin acceso al resto. Cuando se alcanza el número máximo de participantes, la historia se completa y se publica en el archivo.

---

## 2. Stack Tecnológico

| Capa | Tecnología |
|------|-----------|
| Backend | .NET 8 (C#), Clean Architecture |
| ORM | Entity Framework Core |
| Base de datos | PostgreSQL |
| Frontend | Flutter (Dart), Riverpod |
| Autenticación | Firebase Authentication (email/contraseña + Google) |
| Notificaciones | Firebase Cloud Messaging (FCM) |
| Despliegue | Railway / Render / Fly.io |
| Testing | xUnit — unit tests de servicios backend |

---

## 3. Modelo de Datos

### User
```
Id          string     Firebase UID (PK)
Name        string
Email       string
FCMToken    string?
CreatedAt   DateTime
```

### Cadaver
```
Id                Guid       PK
Title             string     "Historia del {CreatedAt:dd MMM yyyy}"
MaxParticipants   int        2–10
CurrentTurn       int        default: 1
IsCompleted       bool       default: false
CreatedAt         DateTime
CreatedByUserId   string     FK → User
```

### Fragment
```
Id              Guid      PK
CadaverId       Guid      FK → Cadaver
UserId          string    FK → User
Content         string    validado por conteo de palabras (split por espacios, máx 300)
SequenceOrder   int
CreatedAt       DateTime
```

---

## 4. Arquitectura Backend

```
CadaverExquisito.API
├── Controllers/         → HTTP endpoints, sin lógica de negocio
├── Application/
│   ├── Services/        → CadaverService, FragmentService, NotificationService
│   └── DTOs/            → Request/Response objects
├── Domain/
│   └── Entities/        → User, Cadaver, Fragment (POCOs)
├── Infrastructure/
│   ├── Repositories/    → EF Core, acceso a PostgreSQL
│   └── Notifications/   → FirebaseAdmin SDK wrapper
└── Tests/               → xUnit, unit tests de Services con mocks
```

**Autenticación:** el cliente envía el Firebase ID Token en `Authorization: Bearer <token>`. El middleware valida el token con `FirebaseAdmin` y extrae el UID para identificar al usuario en PostgreSQL.

**CORS:** política configurada para permitir requests desde el cliente Flutter (en desarrollo: `*`; en producción: dominio específico si aplica).

---

## 5. Reglas de Negocio

1. **Disponibilidad**: Un Cadáver está disponible para el usuario si `IsCompleted = false` Y no existe ningún `Fragment` con ese `CadaverId` y ese `UserId`.
2. **Privacidad**: `GET /cadavers/{id}/last-fragment` retorna solo el Fragment con `SequenceOrder = CurrentTurn - 1`. Si `CurrentTurn = 1`, no hay fragmento previo.
3. **Avance de turno**: Al guardar un Fragment, `CurrentTurn++`. Si `CurrentTurn > MaxParticipants`, se setea `IsCompleted = true`.
4. **Validación de palabras**: El backend valida `content.Split(' ').Length <= 300` antes de persistir. El frontend bloquea el botón como UX, no como seguridad.
5. **Concurrencia**: Si dos usuarios intentan escribir el mismo Cadáver simultáneamente, el segundo recibe `409 Conflict`.
6. **Sin repetición**: Cada usuario puede aparecer máximo una vez por Cadáver.

---

## 6. API Endpoints

| Método | Ruta | Descripción |
|--------|------|-------------|
| `POST` | `/api/auth/register` | Crea/actualiza User en PostgreSQL tras Firebase Auth |
| `PUT` | `/api/auth/fcm-token` | Actualiza FCMToken del usuario |
| `GET` | `/api/cadavers/available` | Cadáveres disponibles para el usuario autenticado |
| `GET` | `/api/cadavers/pending` | Cadáveres donde el usuario ya participó, no completados |
| `POST` | `/api/cadavers` | Crea Cadáver + guarda primer fragmento (transacción única) |
| `GET` | `/api/cadavers/{id}/last-fragment` | Solo el fragmento previo (para mostrar al escribir) |
| `POST` | `/api/cadavers/{id}/fragments` | Envía un nuevo fragmento |
| `GET` | `/api/cadavers/completed` | Feed público de historias completadas |
| `GET` | `/api/cadavers/{id}/full` | Historia completa (solo si `IsCompleted = true`) |

---

## 7. Notificaciones FCM

| Evento | Destinatarios | Mensaje |
|--------|--------------|---------|
| `FragmentAdded` | Todos los usuarios sin Fragment en ese Cadáver | "Hay una historia esperando tu continuación." |
| `CadaverCompleted` | Todos los autores de Fragments en ese Cadáver | "La historia en la que participaste está completa." |

Las notificaciones se envían de forma asíncrona tras persistir el fragmento, sin bloquear la respuesta HTTP.

---

## 8. Frontend Flutter

### Estructura de archivos
```
lib/
├── features/
│   ├── auth/
│   │   ├── screens/login_screen.dart
│   │   └── providers/auth_provider.dart
│   ├── cadavers/
│   │   ├── screens/
│   │   │   ├── participate_tab.dart       → Tab 1
│   │   │   └── archive_tab.dart           → Tab 2
│   │   ├── widgets/
│   │   │   ├── cadaver_card.dart
│   │   │   └── create_cadaver_sheet.dart  → Bottom sheet FAB
│   │   └── providers/
│   │       ├── available_cadavers_provider.dart
│   │       └── completed_cadavers_provider.dart
│   └── editor/
│       ├── screens/editor_screen.dart
│       └── providers/word_count_provider.dart
└── core/
    ├── api/
    │   ├── api_client.dart               → Dio + interceptor Firebase token
    │   └── endpoints.dart
    └── theme/
        └── app_theme.dart               → Tipografía serif, Soft UI 90s pastel
```

### Pantallas

**Login**: Firebase Auth UI — email/contraseña + Google Sign-In. Al autenticar llama a `POST /api/auth/register` (upsert).

**Tab 1 — Participar**:
- Sección *"Te toca escribir"*: cadáveres disponibles
- Sección *"En progreso"*: cadáveres donde ya participó el usuario
- FAB (+): bottom sheet con selector de participantes (2–10) + textarea primer fragmento con `WordCounter`

**Editor**:
- Bloque superior (solo lectura, serif): fragmento previo o mensaje de inicio
- Textarea (serif): campo de escritura con `WordCounter` (`X / 300 palabras`)
- Botón "Enviar" deshabilitado si `X > 300`
- Al enviar: vuelve a Tab 1 con snackbar de confirmación

**Tab 2 — Archivo**:
- Feed de cards con título (fecha) y preview del texto
- Vista de lectura: fragmentos concatenados con autor al margen de cada uno

### Riverpod Providers principales
- `authProvider` — estado del usuario Firebase
- `availableCadaversProvider` — lista con auto-refresh
- `completedCadaversProvider` — feed paginado
- `wordCountProvider` — contador reactivo del campo de texto

---

## 9. Identidad Visual (Flutter)

**Estilo**: Soft UI (neumorfismo suave) + paleta retro 90s pastel muted.

**Paleta de colores:**
```
Background:   #F0EBF4   (lavanda muy pálido — base Soft UI)
Surface:      #E8E2EE   (para cards con sombra neumórfica)
Primary:      #A89BB5   (lavanda muted — acciones principales)
Accent:       #C4A882   (beige rosado — detalles, bordes activos)
Text dark:    #3D3347   (casi negro violáceo)
Text muted:   #7A7085   (gris lavanda — subtítulos, metadata)
Success:      #8FB8A0   (sage green — confirmaciones)
```

**Soft UI — reglas de implementación:**
- Sombras duales en cards: sombra clara (`#FFFFFF` con opacidad 0.8) arriba-izquierda + sombra oscura (`#C8BDD4` con opacidad 0.5) abajo-derecha.
- Sin bordes duros — `BorderRadius` de 16–24px en todos los contenedores.
- Botones con efecto "pressed" (sombra invertida al hacer tap).
- Fondos sin gradientes agresivos — color sólido `Background`.

**Tipografía:**
- **Serif** (escritura y lectura): `Playfair Display` — para fragmentos, editor, historias completas.
- **Sans-serif** (UI): `DM Sans` — para etiquetas, botones, navegación.

**Tono visual general**: íntimo, literario, levemente nostálgico. Sin íconos estándar de Material Design — usar íconos de línea fina (Phosphor Icons o similar).

---

## 10. Testing

**Scope**: unit tests en xUnit cubriendo los servicios críticos del backend.

**Casos obligatorios**:
- `CadaverService.GetAvailable` — excluye correctamente cadáveres donde el usuario ya participó
- `FragmentService.AddFragment` — valida el límite de 300 palabras
- `FragmentService.AddFragment` — incrementa `CurrentTurn` y marca `IsCompleted` cuando corresponde
- `FragmentService.GetLastFragment` — retorna solo `SequenceOrder = CurrentTurn - 1`
- `FragmentService.AddFragment` — retorna 409 si el usuario ya tiene un Fragment en ese Cadáver
