using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;
using PARCIAL.Models;
using PARCIAL.Data;
using Microsoft.EntityFrameworkCore;

namespace PARCIAL.Services
{
    public interface IRedisService
    {
        Task<List<Curso>> GetCursosActivosCachedAsync();
        Task RemoveFromCache(string key);
    }

    public class RedisService : IRedisService
    {
        private readonly IDistributedCache _cache;
        private readonly ApplicationDbContext _context;
        private const string CURSOS_ACTIVOS_CACHE_KEY = "cursos_activos";

        public RedisService(IDistributedCache cache, ApplicationDbContext context)
        {
            _cache = cache;
            _context = context;
        }

        public async Task<List<Curso>> GetCursosActivosCachedAsync()
        {
            // 1) Intentar leer la cache
            try
            {
                var json = await _cache.GetStringAsync(CURSOS_ACTIVOS_CACHE_KEY);
                if (!string.IsNullOrEmpty(json))
                    return JsonSerializer.Deserialize<List<Curso>>(json)!;
            }
            catch
            {
                // Si hay error, continuar con BD
            }

            // 2) Ir a BD
            var data = await _context.Cursos
                .Where(c => c.Activo)
                .AsNoTracking()
                .ToListAsync();

            // 3) Guardar en cache (no esperar si hay error)
            try
            {
                var opts = new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(60)
                };
                await _cache.SetStringAsync(CURSOS_ACTIVOS_CACHE_KEY, JsonSerializer.Serialize(data), opts);
            }
            catch
            {
                // Ignorar errores de cache
            }

            return data;
        }

        public async Task RemoveFromCache(string key)
        {
            try
            {
                await _cache.RemoveAsync(key);
            }
            catch
            {
                // Ignorar errores
            }
        }
    }
}