using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Security.Cryptography;

namespace Prostoquasha.PersistentTasks.Sample.FileSystem;

internal readonly struct Md5Hash : IEquatable<Md5Hash>
{
    private readonly byte[] _value = Array.Empty<byte>();

    public Md5Hash(Stream data)
    {
        using var hasher = MD5.Create();
        _value = hasher.ComputeHash(data);
    }

    public bool Equals(Md5Hash other)
    {
        return _value.SequenceEqual(other._value);
    }

    public override bool Equals([NotNullWhen(true)] object? obj)
    {
        return obj is Md5Hash other ? Equals(other) : false;
    }

    public override int GetHashCode()
    {
        return _value[0].GetHashCode();
    }

    public static bool operator ==(Md5Hash left, Md5Hash right)
    {
        return Equals(left, right);
    }

    public static bool operator !=(Md5Hash left, Md5Hash right)
    {
        return !Equals(left, right);
    }
}
