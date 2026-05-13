using EasySave.CryptoSoft;

if (args.Length == 0)
{
    Console.Error.WriteLine("Usage: cryptosoft <filepath>");
    return 0;
}

return XorEncryptor.EncryptFile(args[0]) ? 1 : 0;
