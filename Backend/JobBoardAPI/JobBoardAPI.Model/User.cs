﻿namespace JobBoardAPI.Model
{
    public class User
    {
        public Guid Id { get; set; } 
        public string Email { get; set; }
        public string PasswordHash { get; set; } 
        public string Role { get; set; } // User role (candidate, employer)
        public string FullName { get; set; } 
        public DateTime CreatedAt { get; set; } 
        

    }
}
