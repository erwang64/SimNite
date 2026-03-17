# SimNite

![Logo SimNite](Logo/LogoFull.png)

**Reinstalle tous tes mods MSFS en un seul clic.**

SimNite est une application desktop pour Microsoft Flight Simulator 2020/2024 qui te permet de retrouver tout ton setup de mods apres une reinstallation Windows, sans passer des heures a retrouver chaque addon un par un.

## Le probleme

Tu reinstalle Windows. Ou tu changes de SSD. Ou MSFS corrompt ton Community Folder.
Et la, tu passes 3 heures a retrouver chaque mod, chaque livree, chaque scene que t'avais.

SimNite regle ca.

## Comment ca marche

1. Tu parcours la liste des mods et tu coches ce que tu veux.
2. Tu exportes ta selection en tant que profil (monsetup.json).
3. Apres n'importe quelle reinstallation, tu importes ton profil et tu cliques sur Restaurer.
4. SimNite telecharge et installe tout automatiquement.

Zero telechargement manuel. Zero copier-coller de liens. Zero oubli.

## Fonctionnalites

- Restauration en un clic: selectionne tes mods, clique sur installer, c'est fait.
- Systeme de profils: sauvegarde et partage ton loadout sous forme de fichier JSON simple.
- Detection automatique: trouve ton Community Folder tout seul.
- Deux modes d'installation:
  - Auto: telecharge le ZIP et le depose directement dans ton Community Folder.
  - Assiste: telecharge les installeurs externes (ElevateX, GSX...) et les ouvre pour toi.
- Telechargements paralleles: installe plusieurs mods en meme temps.
- Progression en temps reel: tu vois exactement ce qui telecharge, s'extrait ou attend.

## Identite visuelle

![Icone SimNite](Logo/SimNite_icon.png)
![Wordmark SimNite](Logo/SimNite_text.png)

## Stack technique

- C# / .NET 10
- WPF
- MVVM (sans framework externe)

## Statut du projet

En cours de developpement.

## Contribuer

La base de donnees de mods est un effort communautaire. Si tu veux ajouter un mod a la liste, ouvre une PR ou une issue avec le nom du mod, l'URL de telechargement et la categorie.

## Licence

MIT
