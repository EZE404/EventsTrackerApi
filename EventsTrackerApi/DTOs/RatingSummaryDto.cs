namespace EventsTrackerApi.DTOs;

public class RatingSummaryDto
{
    public double Average { get; set; } // 0..5
    public int Count { get; set; }

    public RatingSummaryDto() { } // para serialización

    public RatingSummaryDto(double average, int count)
    {
        Average = average;
        Count = count;
    }
}