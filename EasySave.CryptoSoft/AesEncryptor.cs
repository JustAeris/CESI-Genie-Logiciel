using System.Security.Cryptography;
using System.Text;

namespace EasySave.CryptoSoft;

// Chiffreur/déchiffreur de fichiers AES-256 en mode CBC.
// Le mot de passe est lu depuis la variable d'environnement EASYSAVE_CRYPTO_KEY,
// avec repli sur une valeur par défaut intégrée si la variable n'est pas définie.
// L'IV est préfixé au contenu chiffré pour que DecryptFile puisse le récupérer sans stockage externe.
public static class AesEncryptor
{
    // Mot de passe utilisé si EASYSAVE_CRYPTO_KEY n'est pas défini
    private const string DefaultPassword = "EasySaveProSoft!";

    // Sel fixe utilisé pour la dérivation de clé — identique pour chiffrement et déchiffrement
    private static readonly byte[] Salt = "EasySaveSalt2024"u8.ToArray();

    // Lit le mot de passe depuis l'environnement ou retourne le mot de passe par défaut
    private static string Password =>
        Environment.GetEnvironmentVariable("EASYSAVE_CRYPTO_KEY") ?? DefaultPassword;

    // --- API publique ---

    // Chiffre le fichier en place (écrase le fichier source par son contenu chiffré)
    public static bool EncryptFile(string filePath, string? password = null)
        => Transform(filePath, password, encrypt: true);

    // Déchiffre le fichier en place (restaure le contenu original)
    public static bool DecryptFile(string filePath, string? password = null)
        => Transform(filePath, password, encrypt: false);

    // --- Méthodes internes ---

    // Lit le fichier, applique le chiffrement ou déchiffrement, et réécrit le résultat au même emplacement
    private static bool Transform(string filePath, string? password, bool encrypt)
    {
        try
        {
            password ??= Password; // utilise le mot de passe par défaut si aucun n'est fourni
            var input = File.ReadAllBytes(filePath);
            var output = encrypt ? Encrypt(input, password) : Decrypt(input, password);
            File.WriteAllBytes(filePath, output);
            return true;
        }
        catch { return false; } // retourne false sans exception pour ne pas interrompre le job
    }

    // Dérive une clé AES-256 depuis le mot de passe, génère un IV aléatoire,
    // puis retourne [IV (16 octets)] + [données chiffrées]
    private static byte[] Encrypt(byte[] data, string password)
    {
        using var keyMaterial = new Rfc2898DeriveBytes(password, Salt, 10_000, HashAlgorithmName.SHA256);
        using var aes = Aes.Create();
        aes.Key = keyMaterial.GetBytes(32); // clé 256 bits
        aes.GenerateIV();                   // IV aléatoire différent à chaque chiffrement

        using var ms = new MemoryStream();
        ms.Write(aes.IV); // préfixe l'IV pour pouvoir le relire au déchiffrement
        using (var cs = new CryptoStream(ms, aes.CreateEncryptor(), CryptoStreamMode.Write))
            cs.Write(data);
        return ms.ToArray();
    }

    // Relit l'IV depuis les 16 premiers octets, dérive la même clé, puis déchiffre le reste
    private static byte[] Decrypt(byte[] data, string password)
    {
        using var keyMaterial = new Rfc2898DeriveBytes(password, Salt, 10_000, HashAlgorithmName.SHA256);
        using var aes = Aes.Create();
        aes.Key = keyMaterial.GetBytes(32); // même clé que lors du chiffrement

        var iv = data[..16];     // les 16 premiers octets sont l'IV
        var cipher = data[16..]; // le reste est le texte chiffré
        aes.IV = iv;

        using var ms = new MemoryStream();
        using (var cs = new CryptoStream(new MemoryStream(cipher), aes.CreateDecryptor(), CryptoStreamMode.Read))
            cs.CopyTo(ms);
        return ms.ToArray();
    }
}
