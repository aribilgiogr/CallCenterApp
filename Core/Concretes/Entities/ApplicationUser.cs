using Core.Abstracts.Bases;
using Core.Concretes.Enums;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Concretes.Entities
{
    public class ApplicationUser : IdentityUser
    {
        [Required]
        public string FirstName { get; set; } = null!;

        [Required]
        public string LastName { get; set; } = null!;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? LastLoginDate { get; set; }
        public bool IsDeleted { get; set; } = false;

        public virtual ICollection<Customer> Customers { get; set; } = [];
    }

    public class Customer : BaseEntity
    {
        [Required]
        public string Name { get; set; } = null!;
        public string? TaxNumber { get; set; }
        public string? Industury { get; set; }
        public string? Address { get; set; }
        public string? City { get; set; }
        public string? Country { get; set; }

        [ForeignKey(nameof(AssignedUser))]
        public string? AssignedUserId { get; set; }
        public virtual ApplicationUser? AssignedUser { get; set; }
        public bool IsPerson { get; set; }
        public CustomerStatus Status { get; set; } = CustomerStatus.Potential;

        public virtual ICollection<Contact> Contacts { get; set; } = [];
    }

    public class Activity : BaseEntity
    {
        [Required]
        public string Subject { get; set; } = null!;
        public string? Description { get; set; }
        public DateTime? DueDate { get; set; }
        public DateTime? CompletedAt { get; set; }
        public bool IsCompleted { get; set; } = false;

        [ForeignKey(nameof(AssignedUser))]
        public string? AssignedUserId { get; set; }
        public virtual ApplicationUser? AssignedUser { get; set; }

        [ForeignKey(nameof(RelatedCustomer))]
        public string? RelatedCustomerId { get; set; }
        public virtual Customer? RelatedCustomer { get; set; }

        public ActivityType Type { get; set; }
    }
}