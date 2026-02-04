
using DnsClient;
using Microsoft.AspNetCore.Identity;
using Microsoft.Win32;
using shot_reminder_2.Application.Interfaces;
using shot_reminder_2.Domain.Entities;
using System.ComponentModel.DataAnnotations;

namespace shot_reminder_2.Infrastructure.Auth;

public class PasswordHasherAdapter : IPasswordHasher
{
    private readonly PasswordHasher<User> _hasher = new();

    public string Hash(string password)
        => _hasher.HashPassword(null!, password);

    public bool Verify(string password, string passwordHash)
        => _hasher.VerifyHashedPassword(null!, passwordHash, password)
           is PasswordVerificationResult.Success
           or PasswordVerificationResult.SuccessRehashNeeded;
}

//Register use-case

// -Validate email/password

// -Ensure email unique

// -Hash password

// -Insert user

//Login use-case

// -Find user by email

// -Verify password

// -Issue JWT access token(+ refresh token if you want)
