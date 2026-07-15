using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMR.Application.DTOs.Patients
{
    public class PatientQueryParams
    {
        public string? SearchTerm { get; set; }
        public string? Gender { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? SortBy { get; set; } = "FullName";
        public bool SortDescending { get; set; } = false;
    }
}
