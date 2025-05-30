﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace OstaFandy.DAL.Entities;

[Table("Booking")]
public partial class Booking
{
    [Key]
    public int Id { get; set; }

    public int ClientId { get; set; }

    public int AddressId { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime PreferredDate { get; set; }

    public int? EstimatedMinutes { get; set; }

    [Column(TypeName = "decimal(10, 2)")]
    public decimal? TotalPrice { get; set; }

    [StringLength(500)]
    public string Note { get; set; }

    [Required]
    [StringLength(20)]
    public string Status { get; set; }

    public bool IsActive { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime CreatedAt { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime UpdatedAt { get; set; }

    [ForeignKey("AddressId")]
    [InverseProperty("Bookings")]
    public virtual Address Address { get; set; }

    [InverseProperty("Booking")]
    public virtual ICollection<BookingService> BookingServices { get; set; } = new List<BookingService>();

    [InverseProperty("Booking")]
    public virtual ICollection<Chat> Chats { get; set; } = new List<Chat>();

    [ForeignKey("ClientId")]
    [InverseProperty("Bookings")]
    public virtual Client Client { get; set; }

    [InverseProperty("Booking")]
    public virtual JobAssignment JobAssignment { get; set; }

    [InverseProperty("Booking")]
    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();

    [InverseProperty("Booking")]
    public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();
}