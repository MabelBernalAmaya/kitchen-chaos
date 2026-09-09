# Kitchen Chaos

Juego cooperativo de cocina 2D en tiempo real — proyecto de ARSW enfocado en concurrencia y sincronización multijugador.

## Stack

.NET 8 (C#) + ASP.NET Core SignalR.

## Estructura

```
src/
  KitchenChaos.Server/   -> servidor con el GameHub (SignalR)
    Hubs/GameHub.cs
    wwwroot/index.html   -> página de prueba de conexión (no es el frontend del juego)
```

## Cómo correrlo localmente

```bash
cd src/KitchenChaos.Server
dotnet run
```

Abre en el navegador la URL que muestra la consola (por ejemplo `http://localhost:5080`). Deberías poder escribir un nombre y un mensaje, darle a Enviar, y verlo aparecer en la lista — eso confirma que el GameHub está funcionando.

## Ramas

- `main` — solo se actualiza en releases cerrados.
- `develop` — rama de integración, donde se juntan las features antes de pasar a `main`.
- `feature/*` — una por persona/historia, se crean a partir de `develop`.

## Convención de commits

Incluir el ID de la historia de Azure Boards en el mensaje, ej:

```
git commit -m "Crear sala de juego AB#4"
```
