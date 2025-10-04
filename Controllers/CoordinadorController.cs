using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PARCIAL.Data;
using PARCIAL.Models;
using PARCIAL.Services;
using System.Security.Claims;

namespace PARCIAL.Controllers
{
    [Authorize(Roles = "Coordinador")]
    public class CoordinadorController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IRedisService _redisService;
        private readonly ILogger<CoordinadorController> _logger;

        public CoordinadorController(ApplicationDbContext context,
                                   UserManager<IdentityUser> userManager,
                                   IRedisService redisService,
                                   ILogger<CoordinadorController> logger)
        {
            _context = context;
            _userManager = userManager;
            _redisService = redisService;
            _logger = logger;
        }

        // GET: Coordinador
        public IActionResult Index()
        {
            return View();
        }

        // GET: Coordinador/Cursos
        public async Task<IActionResult> Cursos()
        {
            var cursos = await _context.Cursos
                .OrderBy(c => c.Nombre)
                .ToListAsync();

            return View(cursos);
        }

        // GET: Coordinador/CrearCurso
        public IActionResult CrearCurso()
        {
            return View();
        }

        // POST: Coordinador/CrearCurso
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CrearCurso(Curso curso)
        {
            if (ModelState.IsValid)
            {
                // Validar horario
                if (curso.HorarioFin <= curso.HorarioInicio)
                {
                    ModelState.AddModelError("HorarioFin", "El horario de fin debe ser posterior al horario de inicio.");
                    return View(curso);
                }

                // Validar código único
                var codigoExistente = await _context.Cursos
                    .AnyAsync(c => c.Codigo == curso.Codigo);
                
                if (codigoExistente)
                {
                    ModelState.AddModelError("Codigo", "Ya existe un curso con este código.");
                    return View(curso);
                }

                curso.Activo = true;
                _context.Add(curso);
                await _context.SaveChangesAsync();

                // Invalidar cache de cursos
                await _redisService.RemoveFromCache("cursos_activos");

                TempData["Success"] = $"Curso {curso.Nombre} creado exitosamente.";
                return RedirectToAction(nameof(Cursos));
            }
            return View(curso);
        }

        // GET: Coordinador/EditarCurso/5
        public async Task<IActionResult> EditarCurso(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var curso = await _context.Cursos.FindAsync(id);
            if (curso == null)
            {
                return NotFound();
            }
            return View(curso);
        }

        // POST: Coordinador/EditarCurso/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditarCurso(int id, Curso curso)
        {
            if (id != curso.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                // Validar horario
                if (curso.HorarioFin <= curso.HorarioInicio)
                {
                    ModelState.AddModelError("HorarioFin", "El horario de fin debe ser posterior al horario de inicio.");
                    return View(curso);
                }

                // Validar código único (excluyendo el curso actual)
                var codigoExistente = await _context.Cursos
                    .AnyAsync(c => c.Codigo == curso.Codigo && c.Id != id);
                
                if (codigoExistente)
                {
                    ModelState.AddModelError("Codigo", "Ya existe un curso con este código.");
                    return View(curso);
                }

                try
                {
                    _context.Update(curso);
                    await _context.SaveChangesAsync();

                    // Invalidar cache de cursos
                    await _redisService.RemoveFromCache("cursos_activos");

                    TempData["Success"] = $"Curso {curso.Nombre} actualizado exitosamente.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await CursoExists(curso.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Cursos));
            }
            return View(curso);
        }

        // POST: Coordinador/DesactivarCurso/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DesactivarCurso(int id)
        {
            var curso = await _context.Cursos.FindAsync(id);
            if (curso == null)
            {
                return NotFound();
            }

            curso.Activo = false;
            _context.Update(curso);
            await _context.SaveChangesAsync();

            // Invalidar cache de cursos
            await _redisService.RemoveFromCache("cursos_activos");

            TempData["Success"] = $"Curso {curso.Nombre} desactivado exitosamente.";
            return RedirectToAction(nameof(Cursos));
        }

        // POST: Coordinador/ActivarCurso/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ActivarCurso(int id)
        {
            var curso = await _context.Cursos.FindAsync(id);
            if (curso == null)
            {
                return NotFound();
            }

            curso.Activo = true;
            _context.Update(curso);
            await _context.SaveChangesAsync();

            // Invalidar cache de cursos
            await _redisService.RemoveFromCache("cursos_activos");

            TempData["Success"] = $"Curso {curso.Nombre} activado exitosamente.";
            return RedirectToAction(nameof(Cursos));
        }

        // GET: Coordinador/Matriculas
        public async Task<IActionResult> Matriculas(int? cursoId)
        {
            var matriculasQuery = _context.Matriculas
                .Include(m => m.Curso)
                .AsQueryable();

            if (cursoId.HasValue)
            {
                matriculasQuery = matriculasQuery.Where(m => m.CursoId == cursoId.Value);
            }

            var matriculas = await matriculasQuery
                .OrderByDescending(m => m.FechaRegistro)
                .ToListAsync();

            // Obtener información de usuarios por separado
            var userIds = matriculas.Select(m => m.UsuarioId).Distinct().ToList();
            var users = await _context.Users
                .Where(u => userIds.Contains(u.Id))
                .ToDictionaryAsync(u => u.Id, u => u.UserName);

            ViewBag.Users = users;
            ViewBag.Cursos = await _context.Cursos
                .Where(c => c.Activo)
                .OrderBy(c => c.Nombre)
                .ToListAsync();
            ViewBag.CursoSeleccionado = cursoId;

            return View(matriculas);
        }

        // POST: Coordinador/ConfirmarMatricula/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmarMatricula(int id)
        {
            var matricula = await _context.Matriculas
                .Include(m => m.Curso)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (matricula == null)
            {
                return NotFound();
            }

            // Obtener el nombre de usuario
            var user = await _context.Users.FindAsync(matricula.UsuarioId);
            var userName = user?.UserName ?? "Usuario desconocido";

            matricula.Estado = EstadoMatricula.Confirmada;
            _context.Update(matricula);
            await _context.SaveChangesAsync();

            TempData["Success"] = $"Matrícula confirmada para {userName} en el curso {matricula.Curso.Nombre}.";
            return RedirectToAction(nameof(Matriculas), new { cursoId = matricula.CursoId });
        }

        // POST: Coordinador/CancelarMatricula/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelarMatricula(int id)
        {
            var matricula = await _context.Matriculas
                .Include(m => m.Curso)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (matricula == null)
            {
                return NotFound();
            }

            // Obtener el nombre de usuario
            var user = await _context.Users.FindAsync(matricula.UsuarioId);
            var userName = user?.UserName ?? "Usuario desconocido";

            matricula.Estado = EstadoMatricula.Cancelada;
            _context.Update(matricula);
            await _context.SaveChangesAsync();

            TempData["Success"] = $"Matrícula cancelada para {userName} en el curso {matricula.Curso.Nombre}.";
            return RedirectToAction(nameof(Matriculas), new { cursoId = matricula.CursoId });
        }

        // Método auxiliar que faltaba
        private async Task<bool> CursoExists(int id)
        {
            return await _context.Cursos.AnyAsync(e => e.Id == id);
        }
    }
}