using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PARCIAL.Models;
using PARCIAL.Data;

namespace PARCIAL.Data
{
    public static class SeedData
    {
        public static async Task Initialize(IServiceProvider serviceProvider)
        {
            using (var context = new ApplicationDbContext(
                serviceProvider.GetRequiredService<DbContextOptions<ApplicationDbContext>>()))
            {
                // Verificar si ya existen cursos
                if (!context.Cursos.Any())
                {
                    // Crear cursos
                    var cursos = new Curso[]
                    {
                        new Curso
                        {
                            Codigo = "MAT101",
                            Nombre = "Matemáticas Básicas",
                            Creditos = 4,
                            CupoMaximo = 30,
                            HorarioInicio = TimeSpan.FromHours(8),
                            HorarioFin = TimeSpan.FromHours(10),
                            Activo = true
                        },
                        new Curso
                        {
                            Codigo = "PROG101",
                            Nombre = "Programación I",
                            Creditos = 5,
                            CupoMaximo = 25,
                            HorarioInicio = TimeSpan.FromHours(10),
                            HorarioFin = TimeSpan.FromHours(12),
                            Activo = true
                        },
                        new Curso
                        {
                            Codigo = "FIS101",
                            Nombre = "Física General",
                            Creditos = 4,
                            CupoMaximo = 35,
                            HorarioInicio = TimeSpan.FromHours(14),
                            HorarioFin = TimeSpan.FromHours(16),
                            Activo = true
                        }
                    };

                    context.Cursos.AddRange(cursos);
                    await context.SaveChangesAsync();
                }

                // Crear rol de Coordinador y usuario (SOLO si no existen)
                await EnsureCoordinadorRoleAndUser(serviceProvider);
            }
        }

        private static async Task EnsureCoordinadorRoleAndUser(IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<IdentityUser>>();

            // Crear rol Coordinador si no existe
            string roleName = "Coordinador";
            var roleExist = await roleManager.RoleExistsAsync(roleName);
            if (!roleExist)
            {
                await roleManager.CreateAsync(new IdentityRole(roleName));
            }

            // Crear usuario coordinador si no existe
            var coordinadorEmail = "coordinador@universidad.edu";
            var coordinadorUser = await userManager.FindByEmailAsync(coordinadorEmail);
            if (coordinadorUser == null)
            {
                coordinadorUser = new IdentityUser
                {
                    UserName = coordinadorEmail,
                    Email = coordinadorEmail,
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(coordinadorUser, "Coordinador123!");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(coordinadorUser, roleName);
                }
            }
            else
            {
                // Asegurar que el usuario tenga el rol
                if (!await userManager.IsInRoleAsync(coordinadorUser, roleName))
                {
                    await userManager.AddToRoleAsync(coordinadorUser, roleName);
                }
            }
        }
    }
}