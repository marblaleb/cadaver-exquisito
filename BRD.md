# Proyecto: CadaverExquisito.App - Especificaciones Técnicas

Este documento detalla los requerimientos y la arquitectura para la implementación de una aplicación de escritura colaborativa asíncrona ("Cadaver Exquisito").

## 1. Stack Tecnológico
- **Backend:** .NET 8 (C#) con Entity Framework Core.
- **Frontend:** Flutter (Dart) con gestión de estado (Provider o Riverpod).
- **Base de Datos:** PostgreSQL.
- **Comunicación:** REST API + Firebase Cloud Messaging (Notificaciones).

## 2. Modelado de Datos (Backend - .NET Core)

### Entidades Principales:
1.  **User**: Identidad, Nombre, Email, FCMToken.
2.  **Cadaver**:
    - `Id` (Guid)
    - `MaxParticipants` (int)
    - `CurrentTurn` (int)
    - `IsCompleted` (bool)
    - `CreatedAt` (DateTime)
3.  **Fragment**:
    - `Id` (Guid)
    - `CadaverId` (FK)
    - `UserId` (FK)
    - `Content` (string - max 300 words)
    - `SequenceOrder` (int)

## 3. Lógica de Negocio (Reglas Críticas)
- **Privacidad de Contenido:** Al escribir un fragmento, el usuario SOLO puede recuperar el `Content` del `Fragment` con `SequenceOrder = CurrentTurn - 1`. No puede ver el resto de la historia.
- **Finalización:** Cuando `CurrentTurn == MaxParticipants`, se marca `IsCompleted = true`.
- **Validación:** El backend debe validar que el texto no supere las 300 palabras mediante un split de espacios en el servidor antes de guardar.

## 4. Endpoints API Necesarios
- `POST /api/cadavers`: Inicia un cadáver (Define participantes y primer fragmento).
- `GET /api/cadavers/pending`: Lista de cadáveres donde es el turno del usuario.
- `GET /api/cadavers/{id}/last-fragment`: Obtiene solo el último fragmento para continuar la historia.
- `POST /api/cadavers/{id}/fragments`: Envía un nuevo fragmento.
- `GET /api/cadavers/completed`: Lista de historias terminadas (Lectura pública).
- `GET /api/cadavers/{id}/full`: Solo disponible si `IsCompleted == true`. Retorna todos los fragmentos ordenados.

## 5. Frontend (Flutter) - Estructura de Pantallas

### A. Pantalla "Participar" (Tab 1)
- **Botón flotante (+)**: Abre un modal/pantalla para crear uno nuevo (Input: Num. Participantes, Input: Texto).
- **Lista de Pendientes**: Cards que muestran "Te toca escribir en esta historia". Al tocar, abre el editor mostrando el fragmento previo.

### B. Pantalla "Archivo / Completados" (Tab 2)
- Feed de historias finalizadas.
- Vista de lectura: Título (generado o fecha) y el cuerpo completo del texto unido.

## 6. Notificaciones Push
- **Evento**: `FragmentAdded`.
- **Destino**: El siguiente participante (si es por invitación) o notificación general a la comunidad.
- **Evento**: `CadaverCompleted`.
- **Destino**: Todos los IDs de usuario presentes en la tabla `Fragments` para ese `CadaverId`.

## 7. Instrucciones para Claude Code
1.  **Backend**: Crear una arquitectura limpia (Clean Architecture) con Controladores, Servicios y Repositorios. Implementar una política de CORS para Flutter.
2.  **Flutter**: Usar una UI limpia, tipografía Serif para la escritura. Implementar un `WordCounter` dinámico en el campo de texto que bloquee el envío si supera las 300 palabras.