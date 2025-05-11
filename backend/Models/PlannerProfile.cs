using System.ComponentModel.DataAnnotations;

namespace backend.Models
{
    public class PlannerProfile
    {
        [Key]
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public Guid PlannerId { get; set; }
        public virtual User User { get; set; }
        public virtual Planner Planner { get; set; }
    }
}
