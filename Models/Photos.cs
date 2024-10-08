﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using InsuranceServices.Models.DTO.HomeInsurance;

namespace InsuranceServices.Models
{
    public class Photos
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        [ForeignKey(nameof(Home_Insurance))]
        public int Home_InsuranceId { get; set; }
        public Home_Insurance? Home_Insurance { get; set; }
        public string? PhotoUrl { get; set; }

    }
}
