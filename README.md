# EasySave Console

Application console en C# .NET pour gérer et exécuter des jobs de sauvegarde,
conçue selon l'architecture **MVVM**.

> Migration en cours vers l'architecture MVVM — voir la section [Architecture](#architecture-mvvm).

---
 
## version
V1.0.0

---

## Fonctionnalités

- **Multilinguisme** — interface disponible en français (FR) et anglais (EN)
- **Gestion des jobs** — création, affichage, recherche et exécution de sauvegardes
- **Validation** — vérification stricte des chemins absolus (fichiers et dossiers)

---

## Prérequis

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- Git

---

## Installation

1. Clonez le dépôt :
   ```bash
   git clone https://github.com/votre-repo/easysave.git
   cd easysave
   ```

2. Lancez l'application :
   ```bash
   dotnet run
   ```

---

## Architecture MVVM

Le projet migre d'un modèle monolithique (tout dans `Program.cs`) vers le pattern MVVM.
Répartition stricte des responsabilités :

- Le **Model** contient la logique métier pure et les données (par exemple : `BackUpJob`, `ActiveJob`, gestion de fichiers).
- Le **ViewModel** sert d'intermédiaire entre le Model et la View (par exemple : `MainViewModel`).
- La **View** s'occupe uniquement de l'interface utilisateur (par exemple : `Program.cs`, menus console).

**Règles de séparation :**

- La *View* ne contient aucune logique métier (pas de manipulation de listes, pas de création de sauvegarde directe)
- Le *Model* ne contient aucun affichage console (ni `Console.WriteLine`, ni `Console.ReadLine`)

---

## Conventions de code

### Nommage (standard C#)

- Les classes, méthodes et propriétés publiques suivent la convention `PascalCase` (ex: `BackUpJob`, `ExecuteJob()`).
- Les variables locales et les paramètres s'écrivent en `camelCase` (ex: `jobName`, `sourcePath`).
- Les champs privés utilisent le `_camelCase` avec un tiret bas (ex: `_translations`, `_currentJob`).

### Gestion des langues

Toute chaîne affichée à l'utilisateur doit passer par `LanguageService` :

```csharp
// Interdit
Console.WriteLine("Bienvenue");

// Obligatoire
Console.WriteLine(LanguageService.T("menu.welcome"));
```

Toute nouvelle chaîne doit être ajoutée dans les deux fichiers de traduction :

```
languages/
├── LanguageFR.json   ← "menu.welcome": "Bienvenue"
└── LanguageEN.json   ← "menu.welcome": "Welcome"
```

Format à respecter — syntaxe plate uniquement :
```json
{
  "menu.welcome": "Bienvenue",
  "create.error": "Erreur lors de la création"
}
```

---

## Tests d'acceptation

- **Test 1 :** Créer un job avec un chemin valide. *Résultat attendu :* Job ajouté à la liste (Statut : OK).
- **Test 2 :** Créer un job avec un chemin invalide. *Résultat attendu :* Message d'erreur affiché (Statut : OK).
- **Test 3 :** Lister les jobs sans aucun job créé. *Résultat attendu :* Message "liste vide" affiché (Statut : OK).
- **Test 4 :** Changer la langue en cours d'exécution. *Résultat attendu :* Interface traduite immédiatement (Statut : OK).

---

## Roadmap

- Déplacer la logique métier de `Program.cs` vers `MainViewModel`
- Créer des classes View distinctes pour alléger `Program.cs`
- Implémenter le système de logs (JSON / XML)
- Afficher la progression en temps réel pour les gros volumes

---

## Auteurs

Projet réalisé dans le cadre du cursus **CESI — 3ème année**.
ROURE Antoine
AUGER Benjamin
NGOUONPE-FEZEU-TAMEU jeffrey
DOUBLET Amaury