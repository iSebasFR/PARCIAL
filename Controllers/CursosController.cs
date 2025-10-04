using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PARCIAL.Data;
using PARCIAL.Models;
using PARCIAL.Services;
using System.Diagnostics;

namespace PARCIAL.Controllers
{
    public class CursosController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<CursosController> _logger;
        private readonly IRedisService _redisService;

        // Clave para sesión
        private const string ULTIMO_CURSO_VISITADO_SESSION_KEY = "UltimoCursoVisitado";

        public CursosController(ApplicationDbContext context, 
                              ILogger<CursosController> logger,
                              IRedisService redisService)
        {
            _context = context;
            _logger = logger;
            _redisService = redisService;
        }

        // GET: Cursos/Catalogo
        public async Task<IActionResult> Catalogo(CatalogoFiltroViewModel filtros)
        {
            List<Curso> cursos;

            // Usar cache solo si no hay filtros aplicados
            if (SinFiltrosAplicados(filtros))
            {
                cursos = await _redisService.GetCursosActivosCachedAsync();
            }
            else
            {
                // Si hay filtros, obtener directamente de la base de datos
                cursos = await ObtenerCursosConFiltros(filtros);
            }

            var viewModel = new CatalogoViewModel
            {
                Cursos = cursos,
                Filtros = filtros
            };

            return View(viewModel);
        }

        // GET: Cursos/Detalle/5
        public async Task<IActionResult> Detalle(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var curso = await _context.Cursos
                .FirstOrDefaultAsync(m => m.Id == id && m.Activo);

            if (curso == null)
            {
                return NotFound();
            }

            // Guardar en sesión el último curso visitado
            HttpContext.Session.SetString(ULTIMO_CURSO_VISITADO_SESSION_KEY, 
                $"{curso.Id}|{curso.Nombre}");

            return View(curso);
        }

        // Método para invalidar cache (será usado por el CoordinadorController)
        public async Task InvalidarCacheCursos()
        {
            await _redisService.RemoveFromCache("cursos_activos");
            _logger.LogInformation("Cache de cursos invalidado");
        }

        // Métodos auxiliares privados
        private bool SinFiltrosAplicados(CatalogoFiltroViewModel filtros)
        {
            return string.IsNullOrEmpty(filtros.Nombre) && 
                   !filtros.CreditosMin.HasValue && 
                   !filtros.CreditosMax.HasValue && 
                   !filtros.HorarioDesde.HasValue && 
                   !filtros.HorarioHasta.HasValue;
        }

        private async Task<List<Curso>> ObtenerCursosConFiltros(CatalogoFiltroViewModel filtros)
        {
            var query = _context.Cursos
                .Where(c => c.Activo)
                .AsNoTracking()
                .AsQueryable();

            // Aplicar filtros que SÍ se pueden traducir a SQL
            if (!string.IsNullOrEmpty(filtros.Nombre))
            {
                var nombre = filtros.Nombre.ToLower();
                query = query.Where(c => 
                    c.Nombre.ToLower().Contains(nombre) || 
                    c.Codigo.ToLower().Contains(nombre));
            }

            if (filtros.CreditosMin.HasValue)
            {
                query = query.Where(c => c.Creditos >= filtros.CreditosMin.Value);
            }

            if (filtros.CreditosMax.HasValue)
            {
                query = query.Where(c => c.Creditos <= filtros.CreditosMax.Value);
            }

            // Ejecutar consulta inicial
            var cursos = await query.OrderBy(c => c.Nombre).ToListAsync();

            // Aplicar filtros de horario en memoria
            if (filtros.HorarioDesde.HasValue)
            {
                cursos = cursos.Where(c => c.HorarioInicio >= filtros.HorarioDesde.Value).ToList();
            }

            if (filtros.HorarioHasta.HasValue)
            {
                cursos = cursos.Where(c => c.HorarioFin <= filtros.HorarioHasta.Value).ToList();
            }

            return cursos;
        }
    }
}