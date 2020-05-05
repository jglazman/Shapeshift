# Shape / Shift

## Release Notes

* The focus was mainly on the architecture and code, so content didn't go in until the very end, but of course that was kind of the point of the exercise; the game is essentially feature-complete and the project is structured so that adding new content (art, levels, animation, VFX, popups, scene transitions) is trivial.

* There's no sound.  The sound/music toggles are just for show.

* ~90% of the art is from a free 'buttons' pack from the Unity asset store.  It came with a 'Heart' icon but no 'Star' icon, so please forgive the obvious clash of themes ;P

* Only levels 1-5 have been created.  (FYI: they are more 'tech demo' than 'good design'...)

* After level 5 the level editor will automatically open.

* The editor interface is usable on a phone, but it's a bit archaic. The important buttons are 'Refresh' (to resize/reload the level) and 'Save'.  Tap the tiles to edit the level layout.  The rest should be easy enough to figure out with just a bit of experimentation.

* Access the 'Pause' menu to quit the level or to toggle the level editor (the floppy disk button).


## Unity Project Structure

### Scenes

#### Bootstrap

All singleton MonoBehaviours live in this scene.  They are marked as DontDestroyOnLoad, and we will never return to this scene again.  This is typically where prefabs provided by 3rd party services will live. (Adjust, etc.)

* Bootstrap: The 'entry point' of the application.
* CoroutineRunner: A simple Coroutine wrapper.  In general coroutines need a reliable GameObject to run on, else they risk being cancelled unexpectedly.
* EventSystem: Required for Unity UI (UGUI) input.
* PrefabPool: Given a MonoBehaviour type (which implements IPooledObject) and a prefab, this will stash away inactive prefab instances until they are needed again.
* TweenRunner: Runs the Update loop of our Tween class, and serializes AnimationCurves. Use the TweenRunner inspector to configure your tweens.

#### MainMenu

This is just a landing page, but it also showcases the static SceneController class, which controls all scene loading. Scenes are loaded async and additive, so we can seamlessly transition from one scene to the next:

* All SceneTransitioner scripts saved in a scene will register themselves with the SceneController on Awake.
* When a scene change is requested, all SceneTransitioners will be notified to play their Outro animations.
* While this is happening, the next scene will load in the background and the new scene's Transitioners will wake up.  When all Outros are complete, the old scene is unloaded and the new scene's Transitioners are notified to play their Intro animations.
* Animator Override Controllers are used in each scene to hook up custom animations to the states provided by the main state machine, found at /Assets/Animation/GenericTransitioner.
* Of course any knowledgeable tech artist or animator could opt to create their own custom blend trees; in that case they just need to support "PlayIntro" and "PlayOutro" triggers, and add a SceneTransitionStateBehaviour component to their Outro state. No extra code is needed to create custom scene transitions.

The MainMenu scene's transitioner is found in the scene at /Canvas-MainMenu/Panel. If you inspect this in the Animator window while the game is playing, you can toggle the PlayIntro and PlayOutro triggers to modify and test the transition animations.

#### WorldMap

This is a scroll view with all of the game's levels select buttons baked in.  New levels can be added to the game by dragging the /Assets/Prefabs/WorldMapNode prefab into the scroll view and assigning it an incremental level index.

The level itself does not need to be created immediately.  If you play the game and select a level that does not exist, then the level editor will automatically open and you can design the level.  If you do this from the Unity editor then the level file (JSON) will be saved into Resources, but if you are designing levels on a device then it will be saved to PersistentDataPath and you will need to copy the level file manually, or you can copy the level JSON from adb logcat.

#### Level

Here is the actual game, but only the UI is saved into the scene.  The game components (primarily the GridNode and GridItem prefabs from /Assets/Prefabs) will be instantiated at runtime, and will be dynamically resized to fit the screen.  The RectTransform in the scene at /LevelView/Panel-Playfield/Content/Playfield determines the maximum playable area.


## Game Data

### Pipeline

The content pipeline is perhaps the most crucial element determining the effectiveness and velocity of a game project's development, from preproduction all the way to live ops.  The final shape of a content pipeline depends on the specific game, technology, CI, developers, etc., but in general I prefer using Google Spreadsheets as the main interface for game data.

You can access the data for ShapeShift here:  https://docs.google.com/spreadsheets/d/15L4e8Yz0Oq5o9PjpjN1IquF185JiBrID310r2_XgSd8/edit?usp=sharing

For this prototype a quick-and-dirty approach was taken; the tabs are downloaded as .csv files and saved directly to /Assets/Data/Resources, with boilerplate loading code in the GameConfig class.

### Config

#### MatchRules

This controls the high-level rules of the game for each level:

* Which types of tiles are allowed on the playfield?
* How are valid matches determined? (e.g. match-3, words with 4+ letters, etc.)
* How many points are they worth?

#### GridNode

These are the definitions for the static elements of the playfield:

* Which sprite should be rendered on this part of the playfield?
* Can a tile be placed here?
* Any point modifiers? (e.g. double-letter-score, triple-word-score, etc.)

#### GridItem

These are the definitions for the dynamic elements of the playfield:

* Which prefab or sprite should be used for this item?
* What other items can be matched with this item?
* What are the drop rates and point modifiers?

## Credits

* Credit to my son for naming the game, and also providing the monster drawing.  The monster is called "Chameledon" -- it is half chameleon and half dinosaur, and you must feed it jewels (e.g. match the blocks) to keep it happy.
* Also credit to NASA for the space photos.


