namespace Timez.Entities
{
    public class TimezProjectsUser : IProjectsUser
    {

        public int BoardId
        {
            get;
            set;
        }

        public int ProjectId
        {
            get;
            set;
        }

        public int ReciveEMail
        {
            get;
            set;
        }

        public int UserId
        {
            get;
            set;
        }
    }
}
