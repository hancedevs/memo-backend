using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace backend.Models
{
    public class Planner
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; }
    public string Email { get; set; }
        public string Phone { get; set; }
        public string Logo { get; set; }
        public bool IsActive { get; set; }=true;
        public bool IsDeleted { get; set; } = false;
        public List<WeddingStory> Weddings { get; set; }
        public virtual List<PlannerProfile> PlannerProfiles { get; set; }
    }

}