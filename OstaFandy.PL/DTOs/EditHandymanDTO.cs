namespace OstaFandy.PL.DTOs
{
    public class EditHandymanDTO
    {
        public int UserId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public int SpecializationId { get; set; }
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
        public string NationalId { get; set; }
        public string NationalIdImg { get; set; }
        public string Img { get; set; }
        public int ExperienceYears { get; set; }
        public string Status { get; set; }
        public string? DefaultAddressPlace { get; set; }
        public string AddressType { get; set; }
        public string? DefaultAddressCity { get; set; }
        public decimal? DefaultAddressLatitude { get; set; }
        public decimal? DefaultAddressLongitude { get; set; }
    }
}
