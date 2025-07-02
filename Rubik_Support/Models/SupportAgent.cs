using System;
using System.Collections.Generic;
using System.Linq;

namespace Rubik_Support.Models
{
    public class SupportAgent
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public bool IsActive { get; set; }
        public bool IsOnline { get; set; }
        public DateTime? LastOnlineDate { get; set; }
        public int MaxConcurrentTickets { get; set; }
        public int CurrentActiveTickets { get; set; }
        public string Specialties { get; set; }
        public string WorkingHours { get; set; }
        public int Priority { get; set; }
        public int TotalHandledTickets { get; set; }
        public int? AverageResponseTime { get; set; }
        public decimal? AverageRating { get; set; }
        public string Notes { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime? ModifiedDate { get; set; }

        // Navigation Properties
        public string UserFullName { get; set; }
        public string UserMobile { get; set; }
        public string UserEmail { get; set; }

        // Helper Properties
        public List<string> SpecialtyList =>
            string.IsNullOrEmpty(Specialties) ? new List<string>() :
                Specialties.Split(',').Select(s => s.Trim()).ToList();

        public bool IsAvailable => IsActive && IsOnline &&
                                   CurrentActiveTickets < MaxConcurrentTickets;
    }
}