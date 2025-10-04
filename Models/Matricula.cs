using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace PARCIAL.Models
{
    public class Matricula
    {
        public int Id { get; set; }

        public int CursoId { get; set; }
        public Curso Curso { get; set; } = null!;

        [Required]
        public string UsuarioId { get; set; } = string.Empty;
        
        [DataType(DataType.DateTime)]
        public DateTime FechaRegistro { get; set; } = DateTime.Now;

        public EstadoMatricula Estado { get; set; } = EstadoMatricula.Pendiente;
    }

    public enum EstadoMatricula
    {
        Pendiente,
        Confirmada,
        Cancelada
    }
}