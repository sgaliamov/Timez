namespace Timez.Entities
{
    public interface IUsersInvite
    {
        int OrganizationId { get; set; }
        string EMail { get; set; }
        string InviteCode { get; set; }
        int UserId { get; set; }
    }
}
