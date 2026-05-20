namespace Quiz_Application.Models.BaseEntities
{
    public class BaseEntity
    {
        public Guid Id { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime DateTime { get; set; } = DateTime.UtcNow;
    }
}
