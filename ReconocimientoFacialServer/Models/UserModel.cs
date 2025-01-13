namespace ReconocimientoFacialServer.Models
{
    public class UserModel
    {
        public int UserId { get; set; }
        public string Name { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public DateTime RegisteredDate { get; set; } = DateTime.Now;
        public byte[] Image { get; set; }

    }
}
