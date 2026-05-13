using EasySave.CryptoSoft;

namespace EasySave.Tests;

public class AesEncryptorTests : IDisposable
{
    private readonly string _tempFile = Path.Combine(Path.GetTempPath(), $"cryptosoft_test_{Guid.NewGuid()}.bin");

    public void Dispose()
    {
        if (File.Exists(_tempFile)) File.Delete(_tempFile);
    }

    // Encrypting a file changes its content
    [Fact]
    public void EncryptFile_ChangesContent()
    {
        var original = new byte[] { 0x48, 0x65, 0x6C, 0x6C, 0x6F }; // "Hello"
        File.WriteAllBytes(_tempFile, original);

        AesEncryptor.EncryptFile(_tempFile);

        Assert.NotEqual(original, File.ReadAllBytes(_tempFile));
    }

    // Encrypt then Decrypt restores the original
    [Fact]
    public void EncryptThenDecrypt_RestoresOriginal()
    {
        var original = new byte[] { 0x48, 0x65, 0x6C, 0x6C, 0x6F }; // "Hello"
        File.WriteAllBytes(_tempFile, original);

        AesEncryptor.EncryptFile(_tempFile);
        AesEncryptor.DecryptFile(_tempFile);

        Assert.Equal(original, File.ReadAllBytes(_tempFile));
    }

    // Custom password round-trip
    [Fact]
    public void EncryptDecrypt_WithCustomPassword_RestoresOriginal()
    {
        var original = System.Text.Encoding.UTF8.GetBytes("EasySave v3.0");
        File.WriteAllBytes(_tempFile, original);

        AesEncryptor.EncryptFile(_tempFile, "MySecret");
        AesEncryptor.DecryptFile(_tempFile, "MySecret");

        Assert.Equal(original, File.ReadAllBytes(_tempFile));
    }

    // Returns true on a valid file
    [Fact]
    public void EncryptFile_ValidFile_ReturnsTrue()
    {
        File.WriteAllBytes(_tempFile, new byte[] { 0x42 });

        Assert.True(AesEncryptor.EncryptFile(_tempFile));
    }

    // Returns false on a non-existent file
    [Fact]
    public void EncryptFile_NonExistentFile_ReturnsFalse()
    {
        Assert.False(AesEncryptor.EncryptFile("no_such_file_xyz.bin"));
    }

    // Encrypted size is larger than original (IV + padding)
    [Fact]
    public void EncryptFile_EncryptedSizeLargerThanOriginal()
    {
        var original = new byte[] { 0x01, 0x02, 0x03 };
        File.WriteAllBytes(_tempFile, original);

        AesEncryptor.EncryptFile(_tempFile);

        Assert.True(File.ReadAllBytes(_tempFile).Length > original.Length);
    }
}
