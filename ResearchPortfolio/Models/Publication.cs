using System.ComponentModel.DataAnnotations;

namespace ResearchPortfolio.Models
{
    public class Publication
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Задължително поле")]
        [StringLength(200, ErrorMessage = "Полето надвишава 200 символа")]
        [Display(Name = "Заглавие")]
        public string Title { get; set; }

        [Required(ErrorMessage = "Задължително поле")]
        [StringLength(2000, ErrorMessage = "Полето надвишава 2000 символа")]
        [Display(Name = "Резюме")]
        public string Abstract { get; set; }

        [Required(ErrorMessage = "Задължително поле")]
        [Range(1900, 2100, ErrorMessage = "Въведете валидна година")]
        [Display(Name = "Година")]
        public int Year { get; set; }
        public string CreatedByUserId { get; set; }

        public ICollection<AuthorPublication> AuthorPublications { get; set; }
        public ICollection<Reference> References { get; set; }
    }
}
