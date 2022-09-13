using Crypto.Keying;

namespace Crypto.Tests.TestObjects;

public class TestKeyDeriver : IKeyDeriver
{
    public byte[] DeriveCryptoKey(byte[] userKey, byte[] salt, byte[] pepper)
        => userKey.Concat(salt).Concat(pepper).ToArray();

    public byte[] DeriveKey(string seed, params byte[][] hashes)
        => throw new NotImplementedException();
}
