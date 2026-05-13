namespace EasySave.CryptoSoft;

/// <summary>
/// XOR-based symmetric file encryptor.
/// Applying it twice on the same file restores the original content.
/// </summary>
public static class XorEncryptor
{
    private const byte Key = 0x55;

    /// <summary>
    /// Encrypts (or decrypts) a file in-place using XOR with a fixed key.
    /// </summary>
    /// <returns>True on success, false if the file cannot be read or written.</returns>
    public static bool EncryptFile(string filePath)
    {
        try
        {
            var bytes = File.ReadAllBytes(filePath);
            for (int i = 0; i < bytes.Length; i++)
                bytes[i] ^= Key;
            File.WriteAllBytes(filePath, bytes);
            return true;
        }
        catch
        {
            return false;
        }
    }
}
