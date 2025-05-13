namespace ITHelpDesk.DTOs.SubDepartment
{
    public class SubDepartmentDto
    {

        public int SubDepartmentId { get; set; }
        public string SubDepartmentName { get; set; }
        public string SubDepartmentDescription { get; set; }
        public int? DepartmentId { get; set; }
        public string DepartmentName { get; set; }
        public string? SubDepartmentManagerId { get; set; }
        public string SubDepartmentManagerName { get; set; }
        public string SubDepartmentManagerEmail { get; set; }
        public string Remarks { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
