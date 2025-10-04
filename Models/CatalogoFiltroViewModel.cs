using System.ComponentModel.DataAnnotations;

namespace PARCIAL.Models
{
    public class CatalogoFiltroViewModel
    {
        [Display(Name = "Nombre del curso")]
        public string? Nombre { get; set; }

        [Display(Name = "Créditos mínimos")]
        [Range(0, 10, ErrorMessage = "Los créditos deben estar entre 0 y 10")]
        public int? CreditosMin { get; set; }

        [Display(Name = "Créditos máximos")]
        [Range(0, 10, ErrorMessage = "Los créditos deben estar entre 0 y 10")]
        public int? CreditosMax { get; set; }

        [Display(Name = "Horario desde")]
        [DataType(DataType.Time)]
        public TimeSpan? HorarioDesde { get; set; }

        [Display(Name = "Horario hasta")]
        [DataType(DataType.Time)]
        public TimeSpan? HorarioHasta { get; set; }
    }
}