using System.Security.Cryptography;

namespace Crypto.Tests.TestHelpers;

internal class UnreusableHash : HMACMD5
{
    public override bool CanReuseTransform => false;
}
