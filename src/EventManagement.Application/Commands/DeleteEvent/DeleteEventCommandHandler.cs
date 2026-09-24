/*
 * Perspective DDD (Conception pilotée par le domaine)

En conception pilotée par le domaine, les agrégats ne doivent pas être supprimés sur un simple caprice métier.
Un événement qui a été un jour Confirmé ou Fermé possède un historique métier. Le supprimer détruit cet historique et contredit le principe selon lequel le modèle de domaine doit refléter la réalité.
Annuler est l'opération métier adéquate lorsqu'un événement est annulé.
La suppression n'a de sens qu'en tant qu'opération technique pour des événements qui n'ont jamais constitué de faits métier réels, typiquement :

Un brouillon créé par erreur et jamais publié.

Un nettoyage lié au RGPD ou à la conservation des données (anonymisation).

Une suppression logique (soft delete) pour le filtrage de l'interface utilisateur (mais il ne s'agit alors pas d'une véritable suppression au sens DDD).
 * */