using System;
using System.Security.Cryptography;
using System.Text;

namespace MccSoft.IntegreSqlClient.DatabaseInitialization;

public class Md5Hasher
{
    /// <summary>
    /// Creates MD5 hash of given <paramref name="input"/>
    /// </summary>
    public static string CreateMD5(string input)
    {
        using var md5 = MD5.Create();
        byte[] inputBytes = Encoding.ASCII.GetBytes(input);
        byte[] hashBytes = md5.ComputeHash(inputBytes);

        return Convert.ToHexString(hashBytes);
    }
}
