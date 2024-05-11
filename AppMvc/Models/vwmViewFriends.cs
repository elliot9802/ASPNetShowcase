using Models;

namespace AppMvc.Models
{
    public class vwmViewFriends
    {
        public List<IFriend> Friends { get; set; }
        public int NrOfFriends => Friends.Count;
    }

    public class vwmViewFriend
    {
        public IFriend Friend { get; set; }
    }
}
