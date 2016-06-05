namespace Timez.DAL.DataContext
{
    abstract class BaseRepository<T>
        where T : BaseRepository<T>, new()
    {
        protected TimezDataContext _Context;

        internal static T Create(TimezDataContext context)            
        {
            T t = new T { _Context = context };
            return t;
        }
    }
}
