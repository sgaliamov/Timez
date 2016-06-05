using Timez.Entities;

namespace Timez.DAL.DataContext
{
    internal partial class User : IUser
    {
        public RegistrationType GetRegistrationType()
        {
            return (RegistrationType) RegistrationType;
        }
    }

    internal partial class UsersInvite : IUsersInvite { }
    
    //public partial class UsersWorkingTime : IUsersWorkingTime { }
}
