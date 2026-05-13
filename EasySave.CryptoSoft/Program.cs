using EasySave.CryptoSoft;

using var mutex = new Mutex(initiallyOwned: false, "Global\\EasySaveCryptoSoft");
if (!mutex.WaitOne(0))
{
    Console.Error.WriteLine("CryptoSoft is already running.");
    return -1;
}

if (args.Length == 0)
{
    Console.Error.WriteLine("Usage: cryptosoft <filepath> [--decrypt]");
    return 0;
}

bool decrypt = args.Length > 1 && args[1] == "--decrypt";
bool ok = decrypt
    ? AesEncryptor.DecryptFile(args[0])
    : AesEncryptor.EncryptFile(args[0]);

return ok ? 1 : 0;
