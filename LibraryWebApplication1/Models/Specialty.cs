using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace LibraryWebApplication1.Models
{
    public partial class Specialty
    {
        [Key]
        public int SpecialtyId { get; set; }

        [Required(ErrorMessage = "Поле не повинно бути порожнім")]
        public string? SpecialtyName { get; set; }
        public virtual ICollection<Group>? Groups { get; set; } = new List<Group>();
    }
}