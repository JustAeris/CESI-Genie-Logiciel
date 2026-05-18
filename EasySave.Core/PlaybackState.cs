namespace EasySave.Core;

// État de contrôle fin d'un job de sauvegarde (T5 — Pause/Reprise/Arrêt).
// Running = en train de copier des fichiers.
// Paused  = en attente sur un ManualResetEventSlim ; aucun fichier n'est en cours de copie.
// Stopped = job terminé ou annulé via CancellationToken.
public enum PlaybackState { Running, Paused, Stopped }
