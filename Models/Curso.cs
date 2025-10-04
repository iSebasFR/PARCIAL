using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PARCIAL.Models
{
    public class Curso
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El código es requerido")]
        [StringLength(20, ErrorMessage = "El código no puede exceder 20 caracteres")]
        public string Codigo { get; set; } = string.Empty;

        [Required(ErrorMessage = "El nombre es requerido")]
        [StringLength(100, ErrorMessage = "El nombre no puede exceder 100 caracteres")]
        public string Nombre { get; set; } = string.Empty;

        [Range(1, 10, ErrorMessage = "Los créditos deben estar entre 1 y 10")]
        public int Creditos { get; set; }

        [Range(1, 100, ErrorMessage = "El cupo máximo debe estar entre 1 y 100")]
        [Display(Name = "Cupo Máximo")]
        public int CupoMaximo { get; set; }

        [DataType(DataType.Time)]
        [Display(Name = "Horario Inicio")]
        public TimeSpan HorarioInicio { get; set; }

        [DataType(DataType.Time)]
        [Display(Name = "Horario Fin")]
        public TimeSpan HorarioFin { get; set; }

        public bool Activo { get; set; } = true;

        // Navigation property
        public ICollection<Matricula> Matriculas { get; set; } = new List<Matricula>();

        // Validación personalizada para HorarioFin > HorarioInicio
        public bool HorarioValido()
        {
            return HorarioFin > HorarioInicio;
        }
    }
}