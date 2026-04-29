namespace EasySave.Core;

public interface ICryptoService
{
    // Encrypt a file and return execution time in ms (negative = error)
    long Encrypt(string filePath);
}
