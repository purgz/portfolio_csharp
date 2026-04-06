namespace Portfolio.Server.Models;

using Microsoft.AspNetCore.Identity;


public class ApplicationUser : IdentityUser
{
  public bool Activated { get; set; } = false;
}