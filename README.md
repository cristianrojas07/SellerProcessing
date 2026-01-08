# Seller Management System

Sistema de Gestión de Vendedores desarrollado con .NET 10, siguiendo principios de Clean Architecture y DDD.

## Estructura del Monorepo

El proyecto está organizado en una estructura profesional dentro de la carpeta `src/`:

- **Core**: Contiene `Domain` (Entidades, Eventos) y `Application` (Lógica de negocio agnóstica).
- **Infrastructure**: Implementación de persistencia (EF Core) y mensajería (RabbitMQ).
- **Api**: Web API con endpoints REST.
- **Worker**: Servicio en segundo plano para procesar eventos de creación de vendedores.
- **BlazorApp**: Interfaz de usuario WebAssembly/Server para gestión.

## Instrucciones de Despliegue (Paso a Paso)

Para levantar todo el ecosistema en menos de 10 minutos:

1.  **Requisitos**: Tener instalado **Docker** y **Docker Compose**.
2.  **Clonar el repositorio**.
3.  **Ejecutar**:
    ```bash
    docker-compose up --build
    ```
    Este comando compilará todas las imágenes (API, Worker, Blazor), levantará SQL Server y RabbitMQ, y configurará la red.

4.  **Migraciones**:
    El sistema está configurado para inicializar la base de datos automáticamente al inicio (`EnsureCreated`), por lo que no es necesario correr migraciones manuales para la primera ejecución.

## URLs de Acceso

Una vez levantado:

- **Web API (Swagger)**: http://localhost:5000/swagger
- **Blazor UI (Frontend)**: http://localhost:5002
- **RabbitMQ Management**: http://localhost:15672 (User: `guest`, Pass: `guest`)

## Calidad de Código y Arquitectura

### Decisiones Arquitectónicas
- **Clean Architecture & DDD**: Separación estricta de responsabilidades. El Dominio no tiene dependencias.
- **RabbitMQ Nativo**: Se utiliza `RabbitMQ.Client` puro sin abstracciones de alto nivel como MassTransit, para demostrar control sobre la infraestructura.
- **Soft Delete**: Implementado a nivel de `DbContext` mediante intercepción de `SaveChangesAsync` y Global Query Filters. No se borra físicamente nada.
- **Manejo de Errores**: Uso del patrón **Result** (Railway Oriented Programming) para evitar el uso de Excepciones como control de flujo.
- **Nombres en Inglés**: Todo el código (clases, variables, métodos) está en inglés.

## Uso de Inteligencia Artificial

Este proyecto fue desarrollado con la asistencia de Inteligencia Artificial.

- **Herramienta utilizada**: Modelo de Lenguaje Avanzado (Gemini/Antigravity) en modo Agente.
- **Partes del challenge donde se usó**:
    - Generación del esqueleto inicial (`docker-compose`, estructura de carpetas).
    - Implementación del cliente nativo de RabbitMQ y configuración de DLQ (Dead Letter Queues).
    - Refactorización de la estructura de carpetas hacia el estándar `src/`.
- **Cómo la uso normalmente en mi trabajo**:
    - Como un "Pair Programmer" instántaneo para generar boilerplate y configuraciones de infraestructura repetitivas.
    - Para validación cruzada de principios SOLID y detección temprana de code smells.
    - Para acelerar la escritura de documentación y pruebas unitarias.