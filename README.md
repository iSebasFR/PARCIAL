# Portal Académico - Gestión de Cursos y Matrículas

## 📋 Descripción del Proyecto
Sistema web interno para gestión de cursos, estudiantes y matrículas desarrollado como examen parcial. Permite a estudiantes autenticados inscribirse en cursos disponibles con validaciones estrictas de cupo y horarios, mientras los coordinadores académicos pueden administrar la oferta de cursos.

## 🚀 Stack Tecnológico
- **Backend:** ASP.NET Core MVC (.NET 8) + Identity
- **Base de datos:** Entity Framework Core + SQLite
- **Cache y Sesiones:** Redis
- **Frontend:** Bootstrap 5 + Razor Views
- **Despliegue:** Render.com + Redis Cloud
- **Control de versiones:** GitHub con ramas por funcionalidad

## 📁 Estructura del Proyecto

### Ramas por Pregunta del Examen:
1. **`feature/bootstrap-dominio`**: Modelos de datos y configuración inicial
2. **`feature/catalogo-cursos`**: Catálogo con filtros y vista detalle  
3. **`feature/matriculas`**: Sistema de inscripciones con validaciones
4. **`feature/sesion-redis`**: Sesiones y cache con Redis
5. **`feature/panel-coordinador`**: Panel de administración para coordinadores
6. **`deploy/render`**: Configuración de despliegue en Render

## 🛠️ Configuración Local

### Prerrequisitos
- .NET 8 SDK
- SQLite
- Redis (opcional para desarrollo)

### Instalación y Ejecución
```bash
# 1. Clonar repositorio
git clone https://github.com/iSebasFR/PARCIAL.git
cd PARCIAL

# 2. Restaurar dependencias
dotnet restore

# 3. Aplicar migraciones de base de datos
dotnet ef database update

# 4. Ejecutar la aplicación
dotnet run
```

### Configuración de Desarrollo
El archivo `appsettings.json` debe contener:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=parcialbase.db",
    "Redis": "localhost:6379"
  }
}
```

## 👥 Credenciales de Prueba

### Usuario Coordinador
- **Email:** coordinador@universidad.edu
- **Contraseña:** Coordinador123!
- **Acceso:** Panel de administración completo

### Usuario Estudiante
- Registrar nuevos usuarios desde la interfaz de registro
- Acceso a catálogo de cursos y sistema de matrículas

## 📊 Funcionalidades Implementadas

### Para Estudiantes
- ✅ Catálogo de cursos activos con filtros (nombre, créditos, horario)
- ✅ Vista detalle de cursos con botón de inscripción
- ✅ Validaciones de inscripción (cupo máximo, solapamiento de horarios)
- ✅ Gestión de matrículas propias
- ✅ Sesión con último curso visitado

### Para Coordinadores
- ✅ CRUD completo de cursos (crear, editar, desactivar)
- ✅ Gestión de matrículas (confirmar, cancelar)
- ✅ Panel administrativo con autorización por roles
- ✅ Invalidación automática de cache

### Técnicas
- ✅ Cache Redis de 60 segundos para listado de cursos
- ✅ Sesiones Redis-backed
- ✅ Validaciones server-side
- ✅ Autorización por roles
- ✅ Migraciones Entity Framework

## 🌐 Despliegue en Render

### Variables de Entorno Requeridas
```
ASPNETCORE_ENVIRONMENT=Production
ASPNETCORE_URLS=http://0.0.0.0:10000
ConnectionStrings__DefaultConnection=Data Source=parcialbase.db
ConnectionStrings__Redis=rediss://red-cu6vgg2j1k6c739kv0rg:6379
```

### URL de Producción
- **Aplicación:** https://portal-academico.onrender.com
- **Catálogo:** https://portal-academico.onrender.com/Cursos/Catalogo
- **Login:** https://portal-academico.onrender.com/Identity/Account/Login

## 🔧 Estructura de Archivos
```
PARCIAL/
├── Controllers/          # Controladores MVC
├── Models/              # Modelos de datos y ViewModels
├── Views/               # Vistas Razor
├── Data/                # DbContext y SeedData
├── Services/            # Servicios (RedisService)
├── Migrations/          # Migraciones de EF Core
├── wwwroot/            # Archivos estáticos
└── Properties/         # Configuración de launch
```

## 📝 Validaciones Implementadas

### Cursos
- Créditos > 0
- HorarioFin > HorarioInicio
- Código único
- Cupo máximo entre 1-100

### Matrículas
- Usuario autenticado
- No superar CupoMaximo
- No solapamiento de horarios
- No duplicar matrículas en mismo curso

## 🔄 Flujo de Trabajo Git
```bash
# Para cada funcionalidad:
git checkout -b feature/nombre-funcionalidad
# Desarrollar funcionalidad
git add .
git commit -m "feat: Descripción de la funcionalidad"
git push origin feature/nombre-funcionalidad
# Crear Pull Request hacia main
```

## 🐛 Solución de Problemas Comunes

### Base de datos no se crea
```bash
dotnet ef database update
```

### Redis no disponible en desarrollo
- Usar `DistributedMemoryCache` como fallback
- Configurar en `Program.cs` para entorno de desarrollo

### Error de migraciones en producción
- Verificar que `Migrations/` está en el repositorio
- Las migraciones se aplican automáticamente al iniciar

## 📞 Soporte

Para issues y preguntas:
1. Revisar logs de aplicación en Render
2. Verificar configuración de variables de entorno
3. Validar que todas las migraciones estén aplicadas

---
