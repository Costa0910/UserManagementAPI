using System;
using System.Collections.Generic;
using System.Linq;
using UserManagementAPI.Models;

namespace UserManagementAPI.Repositories;

public class UserRepository
{
    private readonly List<User> _users = [];
    private int _nextId = 1;

    public IReadOnlyList<User> GetAll() => _users;

    public User? GetById(int id) => _users.FirstOrDefault(u => u.Id == id);

    public bool EmailExists(string email, int? excludingId = null) =>
        _users.Any(u => u.Email.Equals(email, StringComparison.OrdinalIgnoreCase) && (!excludingId.HasValue || u.Id != excludingId.Value));

    public User Add(string name, string email)
    {
        var user = new User(_nextId++, name, email);
        _users.Add(user);
        return user;
    }

    public User? Update(int id, string name, string email)
    {
        var index = _users.FindIndex(u => u.Id == id);
        if (index < 0) return null;
        var updated = _users[index] with { Name = name, Email = email };
        _users[index] = updated;
        return updated;
    }

    public bool Delete(int id)
    {
        var user = GetById(id);
        return user is not null && _users.Remove(user);
    }
}

