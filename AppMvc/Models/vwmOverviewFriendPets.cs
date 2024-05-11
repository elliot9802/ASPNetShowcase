namespace AppMvc.Models
{
    public class vwmOverviewFriendPets
    {
        public List<csFriendPetByCity> FriendsAndPetsByCity { get; set; } = new List<csFriendPetByCity>();
        public string Country { get; set; } = "Finland";

    }

    public class csFriendPetByCity
    {
        public string City { get; set; }
        public int NrFriends { get; set; }
        public int NrPets { get; set; }
    }
}
