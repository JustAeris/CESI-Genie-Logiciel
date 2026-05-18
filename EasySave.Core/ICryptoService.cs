namespace EasySave.Core;

// Interface pour le service de chiffrement — permet de remplacer CryptoSoftRunner par un mock dans les tests.
public interface ICryptoService
{
    // Chiffre le fichier indiqué et retourne le temps d'exécution en ms (valeur négative = erreur)
    long Encrypt(string filePath);
}
