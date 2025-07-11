﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace OstaFandy.DAL.Entities
{
    [Keyless]
    public class AvailableTimeSlotForHandyman
    {
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public int SlotLength { get; set; }
    }
}
