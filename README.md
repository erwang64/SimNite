# SimNite 


**Réinstalle tous tes mods MSFS en un seul clic.**

SimNite est une application desktop pour Microsoft Flight Simulator 2020/2024 qui te permet de retrouver tout ton setup de mods après une réinstallation Windows — sans passer des heures à retrouver chaque addon un par un.

---

## Le problème

Tu réinstalle Windows. Ou tu changes de SSD. Ou MSFS corrompt ton Community Folder.
Et là tu passes 3 heures à retrouver chaque mod, chaque livrée, chaque scène que t'avais.

SimNite règle ça.

---

## Comment ça marche

1. Tu parcours la liste des mods et tu coches ce que tu veux
2. Tu exportes ta sélection en tant que profil (`monsetup.json`)
3. Après n'importe quelle réinstallation, tu importes ton profil et tu cliques sur **Restaurer**
4. SimNite télécharge et installe tout automatiquement

Zéro téléchargement manuel. Zéro copier-coller de liens. Zéro oubli.

---

## Comment ça marche

- **Restauration en un clic** — sélectionne tes mods, clique sur installer, c'est fait
- **Système de profils** — sauvegarde et partage ton loadout sous forme de fichier JSON simple
- **Détection automatique** — trouve ton Community Folder tout seul
- **Deux modes d'installation**
  - *Auto* — télécharge le ZIP et le dépose directement dans ton Community Folder
  - *Assisté* — télécharge les installeurs externes (ElevateX, GSX...) et les ouvre pour toi
- **Téléchargements parallèles** — installe plusieurs mods en même temps
- **Progression en temps réel** — tu vois exactement ce qui télécharge, s'extrait ou attend

---

## Stack technique

- C# / .NET 10
- WPF
- MVVM (sans framework externe)

---

## Statut du projet

> 🚧 En cours de développement — phase initiale

---

## Contribuer

La base de données de mods est un effort communautaire. Si tu veux ajouter un mod à la liste, ouvre une PR ou une issue avec le nom du mod, l'URL de téléchargement et la catégorie.

---

## Licence

MIT
