using EasySave.CryptoSoft;

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
