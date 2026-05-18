using System.Security.Cryptography;
using Microsoft.AspNetCore.WebUtilities;
using Todo.Api.Models.DTOs;

namespace Todo.Api.Services;

public class DbCredentialsGenerator : IDbCredentialsGenerator
{
  public DbCredentials Generate()
  {
    string DbName = Guid.NewGuid().ToString("n");
    string DbUser = Guid.NewGuid().ToString("n");
    string DbPassword = GeneratePassword();

    return new DbCredentials(DbName, DbUser, DbPassword);
  }

  private static string GeneratePassword()
  {
    byte[] randomBytes = new byte[32];
    RandomNumberGenerator.Fill(randomBytes);
    return WebEncoders.Base64UrlEncode(randomBytes);
  }
}