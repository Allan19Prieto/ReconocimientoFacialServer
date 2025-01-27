namespace ReconocimientoFacialServer.Models
{
    public class UserModel
    {
        public int UserId { get; set; }
        public string Name { get; set; }
        public DateTime RegisteredDate { get; set; } = DateTime.Now;
        public List<byte[]> Images { get; set; } = new();

    }
}
