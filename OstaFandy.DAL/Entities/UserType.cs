﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace OstaFandy.DAL.Entities;

[Index("TypeName", Name = "UQ__UserType__D4E7DFA8C8994E39", IsUnique = true)]
public partial class UserType
{
    [Key]
    public int Id { get; set; }

    [Required]
    [StringLength(100)]
    public string TypeName { get; set; }

    [ForeignKey("UserTypeId")]
    [InverseProperty("UserTypes")]
    public virtual ICollection<User> Users { get; set; } = new List<User>();
}