using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace backend.Dto
{
    public class PlannerCreateDto
{
    [Required]
    public string Name { get; set; }

    [Required, EmailAddress]
    public string Email { get; set; }


        public string Phone { get; set; }
        public IFormFile Logo { get; set; }
}
    public class PlannerResponseDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }

      
        public string Email { get; set; }


        public string Phone { get; set; }
        public string Logo { get; set; }
    }
}