using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Portfolio.Server.Data;
using Portfolio.Server.Models;

namespace Portfolio.Server.Controllers;


[ApiController]
[Route("api/[controller]")]
public class LeagueController : ControllerBase
{
  private readonly AppDbContext _db;

  public LeagueController(AppDbContext db)
  {
    _db = db;
  }

  // Public Get endpoint
  [HttpGet]
  public async Task<IActionResult> GetAll()
  {
    var results = await _db.LeagueResults
      .OrderByDescending(r => r.Year)
      .ThenBy(r => r.Semester)
      .ThenByDescending(r => r.Score)
      .ToListAsync();

    return Ok(results);
  }

  [HttpGet("years")]
  public async Task<IActionResult> GetYears()
  {
      var years = await _db.LeagueResults
          .Select(r => r.Year)
          .Distinct()
          .OrderByDescending(y => y)
          .ToListAsync();

      return Ok(years);
  }


  [HttpGet("{year}/{semester}")]
  public async Task<IActionResult> GetBySemester(int year, int semester)
  {
      var results = await _db.LeagueResults
          .Where(r => r.Year == year && r.Semester == semester)
          .OrderByDescending(r => r.Score)
          .ToListAsync();

      return Ok(results);
  }


  //Admin only

  [Authorize]
  [HttpPost]
  public async Task<IActionResult> Create([FromBody] LeagueResult result)
  {
    _db.LeagueResults.Add(result);
    await _db.SaveChangesAsync();
    return CreatedAtAction(nameof(GetAll), new {id = result.Id}, result);
  }


  [Authorize]
  [HttpPut("{id}")]
  public async Task<IActionResult> Update(int id, [FromBody] LeagueResult result)
  {
      var existing = await _db.LeagueResults.FindAsync(id);
      if (existing == null) return NotFound();

      existing.Year = result.Year;
      existing.Semester = result.Semester;
      existing.PlayerName = result.PlayerName;
      existing.Score = result.Score;

      await _db.SaveChangesAsync();
      return Ok(existing);
  }

  [Authorize]
  [HttpDelete("{id}")]
  public async Task<IActionResult> Delete(int id)
  {
      var existing = await _db.LeagueResults.FindAsync(id);
      if (existing == null) return NotFound();

      _db.LeagueResults.Remove(existing);
      await _db.SaveChangesAsync();
      return NoContent();
  }

}