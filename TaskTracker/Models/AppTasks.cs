using System;
using System.Collections.Generic;
using System.Text;
using TaskTracker.Enums;

namespace TaskTracker.Models
{
    public class AppTask
    {
        public int id { get; set; }
        public string? description { get; set; }
        public StatusEnum status { get; set; }
        public DateTime createdAt { get; set; }
        public DateTime updatedAt { get; set; }

    }


}