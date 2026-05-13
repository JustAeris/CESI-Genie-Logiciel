using System.Security.Cryptography;
using System.Text;

namespace EasySave.CryptoSoft;

/// <summary>
/// AES-256 CBC file encryptor. Password defaults to EASYSAVE_CRYPTO_KEY env var,
/// falling back to a built-in default if the variable is not set.
/// The IV is prepended to the cipher output so DecryptFile can recover it.
/// </summary>
public static class AesEncryptor
{
    private const string DefaultPassword = "EasySaveProSoft!";
    private static readonly byte[] Salt = "EasySaveSalt2024"u8.ToArray();

    private static string Password =>
        Environment.GetEnvironmentVariable("EASYSAVE_CRYPTO_KEY") ?? DefaultPassword;

    // --- public API ---

    public static bool EncryptFile(string filePath, string? password = null)
        => Transform(filePath, password, encrypt: true);

    public static bool DecryptFile(string filePath, string? password = null)
        => Transform(filePath, password, encrypt: false);

    // --- internals ---

    private static bool Transform(string filePath, string? password, bool encrypt)
    {
        try
        {
            password ??= Password;
            var input = File.ReadAllBytes(filePath);
            var output = encrypt ? Encrypt(input, password) : Decrypt(input, password);
            File.WriteAllBytes(filePath, output);
            return true;
        }
        catch { return false; }
    }

    private static byte[] Encrypt(byte[] data, string password)
    {
        using var keyMaterial = new Rfc2898DeriveBytes(password, Salt, 10_000, HashAlgorithmName.SHA256);
        using var aes = Aes.Create();
        aes.Key = keyMaterial.GetBytes(32);
        aes.GenerateIV();

        using var ms = new MemoryStream();
        ms.Write(aes.IV);
        using (var cs = new CryptoStream(ms, aes.CreateEncryptor(), CryptoStreamMode.Write))
            cs.Write(data);
        return ms.ToArray();
    }

    private static byte[] Decrypt(byte[] data, string password)
    {
        using var keyMaterial = new Rfc2898DeriveBytes(password, Salt, 10_000, HashAlgorithmName.SHA256);
        using var aes = Aes.Create();
        aes.Key = keyMaterial.GetBytes(32);

        var iv = data[..16];
        var cipher = data[16..];
        aes.IV = iv;

        using var ms = new MemoryStream();
        using (var cs = new CryptoStream(new MemoryStream(cipher), aes.CreateDecryptor(), CryptoStreamMode.Read))
            cs.CopyTo(ms);
        return ms.ToArray();
    }
}
