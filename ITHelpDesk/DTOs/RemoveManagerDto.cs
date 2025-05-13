namespace ITHelpDesk.DTOs
{
    public class RemoveManagerDto
    {
        public int EntityId { get; set; }
        public string EntityType { get; set; } // "department" or "subdepartment"
    }
}
