namespace EasySave.Core;

// Full         = copie tous les fichiers de la source, sans condition.
// Differential = copie uniquement les fichiers absents de la destination ou plus récents que leur copie.
public enum BackupType { Full, Differential }
