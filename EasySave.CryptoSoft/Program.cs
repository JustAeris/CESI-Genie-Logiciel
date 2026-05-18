using EasySave.CryptoSoft;

// Mutex global Windows — garantit qu'une seule instance de cryptosoft.exe tourne à la fois sur la machine.
// CryptoSoftRunner.cs s'appuie sur ce comportement pour sérialiser les chiffrements entre jobs parallèles.
using var mutex = new Mutex(initiallyOwned: false, "Global\\EasySaveCryptoSoft");
if (!mutex.WaitOne(0))
{
    // Une autre instance est déjà active — on quitte sans rien faire
    Console.Error.WriteLine("CryptoSoft is already running.");
    return -1;
}

// Vérifie qu'un chemin de fichier a bien été fourni en argument
if (args.Length == 0)
{
    Console.Error.WriteLine("Usage: cryptosoft <filepath> [--decrypt]");
    return 0;
}

// Le flag --decrypt demande une opération inverse (déchiffrement) ; par défaut on chiffre
bool decrypt = args.Length > 1 && args[1] == "--decrypt";
bool ok = decrypt
    ? AesEncryptor.DecryptFile(args[0])
    : AesEncryptor.EncryptFile(args[0]);

// Code de sortie : 1 = succès, 0 = échec — lu par CryptoSoftRunner pour déterminer le temps d'exécution
return ok ? 1 : 0;
