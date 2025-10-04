using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using PARCIAL.Data;
using PARCIAL.Models;
using System.Diagnostics;

namespace PARCIAL.Controllers
{
    [Authorize]
    public class MatriculasController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<MatriculasController> _logger;

        public MatriculasController(ApplicationDbContext context, 
                                  ILogger<MatriculasController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: Matriculas/Inscribirse/5
        [HttpGet]
        public async Task<IActionResult> Inscribirse(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            
            var curso = await _context.Cursos
                .FirstOrDefaultAsync(c => c.Id == id && c.Activo);

            if (curso == null)
            {
                TempData["Error"] = "Curso no encontrado o no activo.";
                return RedirectToAction("Catalogo", "Cursos");
            }

            // Verificar si ya está matriculado en este curso
            var matriculaExistente = await _context.Matriculas
                .FirstOrDefaultAsync(m => m.CursoId == id && m.UsuarioId == userId);

            if (matriculaExistente != null)
            {
                TempData["Error"] = $"Ya estás matriculado en el curso {curso.Nombre}.";
                return RedirectToAction("Detalle", "Cursos", new { id = id });
            }

            return View(curso);
        }

        // POST: Matriculas/ConfirmarInscripcion
        [HttpPost]
        public async Task<IActionResult> ConfirmarInscripcion(int cursoId)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                
                if (string.IsNullOrEmpty(userId))
                {
                    TempData["Error"] = "Usuario no encontrado.";
                    return RedirectToAction("Detalle", "Cursos", new { id = cursoId });
                }

                // Obtener el curso
                var curso = await _context.Cursos
                    .FirstOrDefaultAsync(c => c.Id == cursoId && c.Activo);

                if (curso == null)
                {
                    TempData["Error"] = "Curso no encontrado o no activo.";
                    return RedirectToAction("Catalogo", "Cursos");
                }

                // Validación 1: Verificar si ya está matriculado en este curso
                var matriculaExistente = await _context.Matriculas
                    .FirstOrDefaultAsync(m => m.CursoId == cursoId && m.UsuarioId == userId);

                if (matriculaExistente != null)
                {
                    TempData["Error"] = $"Ya estás matriculado en el curso {curso.Nombre}.";
                    return RedirectToAction("Detalle", "Cursos", new { id = cursoId });
                }

                // Validación 2: Verificar cupo máximo
                var matriculasConfirmadasCount = await _context.Matriculas
                    .CountAsync(m => m.CursoId == cursoId && m.Estado == EstadoMatricula.Confirmada);

                if (matriculasConfirmadasCount >= curso.CupoMaximo)
                {
                    TempData["Error"] = $"El curso {curso.Nombre} ha alcanzado su cupo máximo de {curso.CupoMaximo} estudiantes.";
                    return RedirectToAction("Detalle", "Cursos", new { id = cursoId });
                }

                // Validación 3: Verificar solapamiento de horarios
                var matriculasUsuario = await _context.Matriculas
                    .Include(m => m.Curso)
                    .Where(m => m.UsuarioId == userId && 
                               m.Estado != EstadoMatricula.Cancelada &&
                               m.Curso.Activo)
                    .ToListAsync();

                var tieneSolapamiento = matriculasUsuario.Any(m => 
                    HorariosSeSolapan(m.Curso.HorarioInicio, m.Curso.HorarioFin, 
                                    curso.HorarioInicio, curso.HorarioFin));

                if (tieneSolapamiento)
                {
                    var cursoSolapado = matriculasUsuario.First(m => 
                        HorariosSeSolapan(m.Curso.HorarioInicio, m.Curso.HorarioFin, 
                                        curso.HorarioInicio, curso.HorarioFin));
                    
                    TempData["Error"] = $"El horario de este curso se solapa con el curso {cursoSolapado.Curso.Nombre} en el que ya estás matriculado.";
                    return RedirectToAction("Detalle", "Cursos", new { id = cursoId });
                }

                // Crear la matrícula
                var matricula = new Matricula
                {
                    CursoId = cursoId,
                    UsuarioId = userId,
                    FechaRegistro = DateTime.Now,
                    Estado = EstadoMatricula.Pendiente
                };

                _context.Matriculas.Add(matricula);
                await _context.SaveChangesAsync();

                TempData["Success"] = $"¡Inscripción exitosa! Te has matriculado en {curso.Nombre}. Estado: Pendiente de confirmación.";
                return RedirectToAction("Detalle", "Cursos", new { id = cursoId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al procesar la inscripción para el curso {CursoId}", cursoId);
                TempData["Error"] = "Ocurrió un error al procesar tu inscripción. Por favor, intenta nuevamente.";
                return RedirectToAction("Detalle", "Cursos", new { id = cursoId });
            }
        }

        // GET: Matriculas/MisMatriculas
        public async Task<IActionResult> MisMatriculas()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            
            var matriculas = await _context.Matriculas
                .Include(m => m.Curso)
                .Where(m => m.UsuarioId == userId)
                .OrderByDescending(m => m.FechaRegistro)
                .ToListAsync();

            return View(matriculas);
        }

        // Helper method para verificar solapamiento de horarios
        private bool HorariosSeSolapan(TimeSpan inicio1, TimeSpan fin1, TimeSpan inicio2, TimeSpan fin2)
        {
            return inicio1 < fin2 && inicio2 < fin1;
        }
    }
}