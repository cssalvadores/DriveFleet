namespace DriveFleet.Application.DTOs.Reservations;

public class ReservationExtraResponse
{
    public int ExtraId { get; set; }

    public string Name { get; set; } = string.Empty;

    public int Quantity { get; set; }

    public decimal Price { get; set; }

    public decimal Total => Quantity * Price;
}
