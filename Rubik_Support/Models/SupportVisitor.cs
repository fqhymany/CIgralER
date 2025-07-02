using System;

namespace Rubik_Support.Models
{
    public class SupportVisitor
    {
        public int Id { get; set; }
        public string Mobile { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime? LastVisitDate { get; set; }
        public bool IsBlocked { get; set; }

        public string FullName => $"{FirstName} {LastName}".Trim();
    }
}