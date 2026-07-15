using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMR.Application.DTOs.Patients
{
    public class PatientUpdateDto : PatientCreateDto
    {
        [System.ComponentModel.DataAnnotations.Required]
        public int PatientId { get; set; }
    }
}
