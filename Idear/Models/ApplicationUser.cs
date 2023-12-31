﻿using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace Idear.Models
{
    public class ApplicationUser : IdentityUser
    {
        [StringLength(100)]
        [Required]
        public string? FullName { get; set; }

     
        public Department? Department { get; set; }

        public List<Idea>? Ideas { get; set; }

        public List<Comment>? Comments { get; set; }

        public List<View>? Views { get; set; }

        public List<React>? Reacts { get; set; }

		[DataType(DataType.Date)]
		public DateTime? BannedDate { get; set; }
    }
}
