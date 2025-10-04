using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PARCIAL.Models;

namespace PARCIAL.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Curso> Cursos { get; set; }
        public DbSet<Matricula> Matriculas { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Configurar restricciones únicas
            builder.Entity<Curso>()
                .HasIndex(c => c.Codigo)
                .IsUnique();

            builder.Entity<Matricula>()
                .HasIndex(m => new { m.CursoId, m.UsuarioId })
                .IsUnique();

            // Validación: HorarioInicio < HorarioFin
            builder.Entity<Curso>()
                .HasCheckConstraint("CK_Curso_Horario", "[HorarioInicio] < [HorarioFin]");

            // Validación: Créditos > 0
            builder.Entity<Curso>()
                .HasCheckConstraint("CK_Curso_Creditos", "[Creditos] > 0");

            // Configurar relación Matricula - Curso
            builder.Entity<Matricula>()
                .HasOne(m => m.Curso)
                .WithMany(c => c.Matriculas)
                .HasForeignKey(m => m.CursoId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}