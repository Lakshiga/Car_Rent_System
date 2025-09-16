namespace Car_Rent_System.ViewModels.Car
{
    public class CarFilterViewModel
    {
        public string? Make { get; set; }
        public string? Model { get; set; }
        public int? MinYear { get; set; }
        public int? MaxYear { get; set; }
        public decimal? MinRate { get; set; }
        public decimal? MaxRate { get; set; }
        public bool? OnlyAvailable { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}