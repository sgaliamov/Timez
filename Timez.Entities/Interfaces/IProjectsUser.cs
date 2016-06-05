namespace Timez.Entities
{
    public interface IProjectsUser
    {
        int BoardId { get; set; }
        int ProjectId { get; set; }
        int ReciveEMail { get; set; }
        int UserId { get; set; }
    }
}
