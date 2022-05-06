using System;
using System.Security.Cryptography;
using System.Text;

namespace MccSoft.IntegreSqlClient.DbSetUp;

public class Md5Hasher
{
    /// <summary>
    /// Creates MD5 hash of given <see cref="input"/>
    /// </summary>
    public static string CreateMD5(string input)
    {
        using var md5 = MD5.Create();
        byte[] inputBytes = Encoding.ASCII.GetBytes(input);
        byte[] hashBytes = md5.ComputeHash(inputBytes);

        return Convert.ToHexString(hashBytes);
    }
}
