using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GovConnect.Models
{
    public class Eligibility
    {
        [Key]
        public int EligibilityId { get; set; }
        [ForeignKey("Scheme")]
        public int SchemeId { get; set; }
        public string Gender { get; set; }
        public int Age { get; set; }

        public int MinAge {  get; set; }

        public int MaxAge { get; set; }
        public string Caste { get; set; }
        public bool? IsDifferentlyAbled { get; set; }
        public bool? IsStudent { get; set; }
        public bool? IsBPL { get; set; }
    }
}
