namespace PARCIAL.Models
{
    public class CatalogoViewModel
    {
        public List<Curso> Cursos { get; set; } = new List<Curso>();
        public CatalogoFiltroViewModel Filtros { get; set; } = new CatalogoFiltroViewModel();
    }
}