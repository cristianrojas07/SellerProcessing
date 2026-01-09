# Seller Management System

Sistema de Gestión de Vendedores que combina carga masiva asíncrona (Excel) con un módulo completo de administración (CRUD).
Desarrollado con **.NET 10**, siguiendo estrictamente **Clean Architecture**, **DDD** y el patrón **CQRS**.

## 📂 Estructura y Arquitectura

La solución sigue una arquitectura limpia dentro de la carpeta `src`:

* **Core**:
    * **Domain**: Contiene Entidades y Lógica de Negocio pura. **No tiene ninguna dependencia externa** (Persistence Ignorance).
    * **Application**: Orquesta los casos de uso implementando el patrón **CQRS** (Command Query Responsibility Segregation). Aquí residen los *Handlers*, *Commands* y *Queries*.
* **Infrastructure**: Implementación de interfaces (EF Core para SQL Server y RabbitMQ.Client para mensajería).
* **Api**: Web API RESTful. Actúa como punto de entrada para el Frontend y disparador de comandos.
* **Worker**: Servicio en background encargado del procesamiento pesado de archivos.
* **BlazorApp**: Interfaz de usuario interactiva.

---

## ✨ Funcionalidades Principales

1.  **Gestión de Vendedores (CRUD):**
    * Creación, Edición y Eliminación (Soft Delete) de vendedores.
    * **Listados Avanzados:** Implementación de paginación en servidor y filtros de búsqueda dinámicos.
2.  **Carga Masiva:**
    * Subida de archivos Excel para procesar múltiples registros.
    * Procesamiento asíncrono mediante colas (RabbitMQ) para no bloquear la UI.

---

## 🚀 Despliegue Rápido con Docker (Recomendado)

**Requisitos:** Docker Desktop instalado.

1.  **Clonar el repositorio:**
    ```bash
    git clone <URL_DEL_REPO>
    cd SellerProcessing
    ```

2.  **Levantar el ecosistema:**
    ```bash
    docker-compose up --build -d
    ```

3.  **Verificar estado:**
    El sistema utiliza **HealthChecks**. SQL Server puede tardar unos 30-40 segundos en iniciar. La API y el Worker esperarán automáticamente a que la base de datos esté lista ("Healthy").
    
    Verifica con:
    ```bash
    docker-compose ps
    ```

---

## 🛠️ Ejecución Manual (Desarrollo)

Si prefieres ejecutar sin Docker Compose, necesitas una instancia de SQL Server y RabbitMQ corriendo.

### 1. Base de Datos
El sistema ejecuta `EnsureCreated` al inicio. Para migraciones manuales:
```bash
dotnet tool install --global dotnet-ef
dotnet ef database update --project src/Infrastructure --startup-project src/Api
```

### 2. Comandos de Ejecución
Ejecuta cada comando en una terminal separada:

**Backend (API):**
```bash
dotnet run --project src/Api
```

**Worker (Procesamiento Background):**
```bash
dotnet run --project src/Worker
```

**Frontend (Blazor):**
```bash
dotnet run --project src/BlazorApp
```

## 🌐 Accesos

**Una vez ejecutados los comandos de docker y que el sistema este up/healthy, se puede acceder a cada componente por medio de estas URL:**

| Componente | URL | Credenciales / Info |
| :--- | :--- | :--- |
| **Frontend (Blazor)** | http://localhost:5002 | Acceso a CRUD y Carga |
| **API Swagger** | http://localhost:5000/swagger | Documentación REST |
| **RabbitMQ** | http://localhost:15672 | `guest` / `guest` |
| **SQL Server** | `localhost,1433` | `sa` / `Password123!` |

---

## 🧪 Datos de Prueba

Para facilitar la validación de la carga masiva y pruebas de estrés, el repositorio incluye archivos de ejemplo listos para usar:

* **Ubicación:** Carpeta `SellerProcessing/data`
* **Archivos disponibles:**
    * 📄 **3,000 registros:** Ideal para una prueba rápida del flujo completo.
    * 📄 **50,000 registros:** Diseñado para probar el rendimiento, la paginación y la estabilidad del Worker bajo carga.
 
---

## 🏗️ Decisiones de Diseño

* **Domain-Driven Design (DDD):** La lógica de negocio reside exclusivamente en el Dominio. Las entidades son ricas y validan sus propios invariantes.
* **CQRS:** Se separaron las operaciones de lectura (Queries) de las de escritura (Commands) en la capa de Aplicación para mayor claridad y escalabilidad.
* **Soft Delete**: Implementado a nivel de `DbContext` mediante intercepción de `SaveChangesAsync` y Global Query Filters. No se borra físicamente nada.
* **RabbitMQ Nativo:** Se utiliza el driver oficial para tener control granular sobre la infraestructura de mensajería (Exchanges/Queues).
* **Docker Orchestration:** Uso de `HealthChecks` y `depends_on` para garantizar un inicio ordenado de los servicios dependientes.
* **Manejo de Errores**: Uso del patrón **Result** (Railway Oriented Programming) para evitar el uso de Excepciones como control de flujo.
* **Nombres en Inglés**: Todo el código (clases, variables, métodos) está en inglés.

## 🤖 Uso de Inteligencia Artificial

Para este desarrollo se utilizaron herramientas de IA (**Gemini Agent y Web**) como soporte:

* **Generación de Código Boilerplate:** Creación rápida de código repetitivo y estructuras base.
* **RabbitMQ:** Asistencia técnica para comprender la sintaxis de la librería cliente y configuración correcta.
* **Apoyo General:** Consultas sobre implementación de código y resolución de errores puntuales.
* **Documentación:** Ayuda en la documentación del proyecto.
