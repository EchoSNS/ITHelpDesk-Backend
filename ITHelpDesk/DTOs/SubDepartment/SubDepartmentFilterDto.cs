namespace ITHelpDesk.DTOs.SubDepartment
{
    public class SubDepartmentFilterDto
    {
        public string SearchTerm { get; set; }
        public int? DepartmentId { get; set; }
        public string? ManagerId { get; set; }
        public bool? UnassignedOnly { get; set; }
    }
}
