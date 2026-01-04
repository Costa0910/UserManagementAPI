using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace UserManagementAPI.Services;

public class TokenStore(IEnumerable<string> initialTokens)
{
    private readonly HashSet<string> _tokens = new(initialTokens, StringComparer.Ordinal);
    private readonly Lock _lock = new();

    public bool Contains(string token)
    {
        lock (_lock)
        {
            return _tokens.Contains(token);
        }
    }

    public string Add(string token)
    {
        lock (_lock)
        {
            _tokens.Add(token);
            return token;
        }
    }

    public string? FirstOrDefault()
    {
        lock (_lock)
        {
            return _tokens.FirstOrDefault();
        }
    }
}

