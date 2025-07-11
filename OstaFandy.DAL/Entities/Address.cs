﻿ // <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using NetTopologySuite.Geometries;
using Microsoft.EntityFrameworkCore;


namespace OstaFandy.DAL.Entities;

public partial class Address
{
    [Key]
    public int Id { get; set; }

    public int UserId { get; set; }

    [Required]
    [Column("Address")]
    [StringLength(255)]
    public string Address1 { get; set; }

    [Required]
    [StringLength(100)]
    public string City { get; set; }

    private decimal _latitude;
    [Column(TypeName = "decimal(9, 6)")]
    public decimal Latitude
    {
        get => _latitude;
        set
        {
            _latitude = value;
            UpdateLocation();
        }
    }

    private decimal _longitude;
    [Column(TypeName = "decimal(9, 6)")]
    public decimal Longitude
    {
        get => _longitude;
        set
        {
            _longitude = value;
            UpdateLocation();
        }
    }

    public Point Location { get; set; }

    private void UpdateLocation()
    {
        
        if (_latitude != 0 && _longitude != 0)
        {
            
            Location = new Point((double)_longitude, (double)_latitude) { SRID = 4326 };
        }
        else
        {
            Location = null;
        }
    }


    [Required]
    [StringLength(20)]
    public string AddressType { get; set; }

    public bool IsDefault { get; set; }

    public bool IsActive { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime CreatedAt { get; set; }

    [InverseProperty("Address")]
    public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();

    [InverseProperty("DefaultAddress")]
    public virtual ICollection<Client> Clients { get; set; } = new List<Client>();

    [InverseProperty("DefaultAddress")]
    public virtual ICollection<Handyman> Handymen { get; set; } = new List<Handyman>();

    [ForeignKey("UserId")]
    [InverseProperty("Addresses")]
    public virtual User User { get; set; }

    
}