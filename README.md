# 🤖 Fall Bots

Jeu de plateforme 3D développé avec Unity 6, dans lequel vous incarnez un petit robot qui saute et esquive les obstacles.

## Lancer le projet

1. Cloner le dépôt
2. Ouvrir le projet dans **Unity 6** avec le pipeline **URP**
3. Ouvrir la scène `Assets/Content/Scenes/Menu.unity`
4. ▶ Play

## Scènes

| Scène | Description |
|---|---|
| `Menu` | Menu principal avec sélection du personnage (randomize) |
| `Menu Level Select` | Sélection du niveau |
| `Tutorial` | Niveau didacticiel — découverte des contrôles librement |
| `Level 01` | Premier niveau : environnement urbain avec voitures |
| `Level 02` | Deuxième niveau |
| `Menu Level Win` | Écran de victoire |

## Contrôles

| Touche | Action |
|---|---|
| `Z / Q / S / D` ou flèches | Déplacement |
| `Espace` | Saut |
| `Souris` | Caméra |

## Scripts principaux

| Script | Rôle |
|---|---|
| `Player.cs` | Gestion du joueur (mouvement, états) |
| `PlayerVisual.cs` | Apparence et customisation du robot |
| `PlayerRespawn.cs` | Respawn après chute |
| `PortalTeleporter.cs` | Téléportation entre les scènes |
| `CarController.cs` | Déplacement des voitures en boucle |
| `KillZone.cs` | Zone de mort (chute) |
| `FallingPlatform.cs` | Plateformes qui tombent |
| `Springboard.cs` | Trampolines |
| `MenuManager.cs` | Navigation entre les menus |

## Technologies

- **Unity 6** — moteur de jeu
- **C#** — scripting
- **URP** (Universal Render Pipeline) — rendu
- **ProBuilder** — modélisation in-editor
- **Git / GitHub** — versioning
