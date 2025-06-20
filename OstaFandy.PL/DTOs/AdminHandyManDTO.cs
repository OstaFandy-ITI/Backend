﻿using OstaFandy.DAL.Entities;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace OstaFandy.PL.DTOs
{
    public class AdminHandyManDTO
    {
        public int UserId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string SpecializationCategory { get; set; }
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
        public string? DefaultAddressPlace { get; set; }
        public string NationalId { get; set; }
        public string NationalIdImg { get; set; }
        public string Img { get; set; }
        public int ExperienceYears { get; set; }
        public string Status { get; set; }
        public List<AdminBlockDateDTO> AdminBlockDateDTO { get; set; } = new List<AdminBlockDateDTO>();
        public AddressDTO DefaultAddress { get; set; } // ← Changed to DTO
        public ICollection<JobAssignmentDTO> JobAssignments { get; set; } = new List<JobAssignmentDTO>(); // ← Changed to DTO
    }

    public class HandyManApplicationDto
    {
        //uer part
        public string Email { get; set; } = null!;

        public string FirstName { get; set; } = null!;

        public string LastName { get; set; } = null!;

        public string Phone { get; set; } = null!;

        public string Password { get; set; } = null!;

        public string ConfirmPassword { get; set; } = null!;


        //handyman part
        public int SpecializationId { get; set; }

        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
        public string NationalId { get; set; }

        public IFormFile NationalIdImg { get; set; }

        public IFormFile Img { get; set; }

        public int ExperienceYears { get; set; }

        //address part
        public string Address { get; set; }
        public string City { get; set; }
        public string AddressType { get; set; }
        public bool IsDefault { get; set; }

    }
}
