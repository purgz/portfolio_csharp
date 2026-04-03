namespace Portfolio.Models;

public class LeagueResultDto
{
    public int Id { get; set; }
    public int Year { get; set; }
    public int Semester { get; set; }
    public string PlayerName { get; set; } = string.Empty;
    public float Score { get; set; }
}