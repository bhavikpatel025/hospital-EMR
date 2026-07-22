using System.Collections.Generic;

namespace EMR.Application.DTOs.Dashboard
{
    public class ChartDataDto
    {
        public string Label { get; set; } = string.Empty;
        public int Value { get; set; }
    }

    public class DashboardAnalyticsDto
    {
        public List<ChartDataDto> AppointmentsByStatus { get; set; } = new();
        public List<ChartDataDto> PatientsByGender { get; set; } = new();
        public List<ChartDataDto> PatientsByAgeGroup { get; set; } = new();
        public List<ChartDataDto> AppointmentsByDoctor { get; set; } = new();
    }
}
