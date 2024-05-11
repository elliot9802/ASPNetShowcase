using Models.DTO;

namespace AppMvc.Models
{
    public class vwmOverviewByCountry
    {
        public List<csFriendsByCountry> FriendsByCountry { get; set; } = new List<csFriendsByCountry>();
    }

    public class csFriendsByCountry
    {
        public string Country { get; set; }
        public List<gstusrInfoFriendsDto> Cities { get; set; }
    }
}