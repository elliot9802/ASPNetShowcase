using Models;

namespace AppMvc.Models
{
    public class vwmListFriends
    {
        public List<IFriend> FriendsList { get; set; }
        public Dictionary<string, List<IFriend>> GroupedFriends { get; set; }
    }
}
