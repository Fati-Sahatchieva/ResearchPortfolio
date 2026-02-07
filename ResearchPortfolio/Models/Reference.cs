using System.ComponentModel.DataAnnotations;

namespace ResearchPortfolio.Models
{
    public class Reference
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Задължително поле")]
        [StringLength(300, ErrorMessage = "Полето надвишава 300 символа")]
        [Display(Name = "Заглавие")]
        public string Title { get; set; }

        [Required(ErrorMessage = "Задължително поле")]
        [StringLength(500, ErrorMessage = "Полето надвишава 500 символа")]
        [Display(Name = "Източник")]
        public string Source { get; set; }

        public int PublicationId { get; set; }
        public Publication Publication { get; set; }
    }
}
