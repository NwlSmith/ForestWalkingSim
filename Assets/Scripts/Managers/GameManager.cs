using System.Collections;
using UnityEngine;

/*
 * Creator: Nate Smith
 * Date Created: 2/11/2021
 * Description: Game Manager class
 * 
 * Controls Game State and holds directory of other services.
 * 
 * Allows different game states to have different Update functions.
 * 
 * 
 * To do:
 * - In URPExampleAssets > Settings > UniversalRenderPipeline Shadow Max distance was initially 50
 * - We have freedom to go places, sure, but it doesn't feel intentional?
 * 
 * LOD is too noticeable, especially on rock
 * FIX CAMERA TWITCH!!!
 * 
 * dialogue sounds repetitive
 * - Add array of sounds?
 * 
 * make jumping animation faster
 * 
 * have camera just look at other animal?
 * 
 * Add text that says Soil returned?
 * 
 * fix turtle position at end of race
 * fix turtle dialogue, doesn't have something for quest stage = 1
 * 
 * fix positions of animals
 * turtle home player camera messed up
 * 
 * hard to see 3rd bird
 * frog and toad dialogue is messed up
 * get rid of any mention of that dastardly carp 
 * not clear where end of race is
 * 
 * make the fox collider thinner so it can move between trees?
 * STILL DIALOGUE ENTRY BUG
 * 
 * also people aren't able to get into dialogue properly after the cutscene?
 * 
 * Put colliders behind tree, only allowed to approach from 1 angle
 */

public class GameManager : MonoBehaviour
{

    #region Variables

    // The finite state machine of the current gamestate.
    private FiniteStateMachine<GameManager> _fsm;

    private bool _gameStarted = false;
    private bool _endingGame = false;

    private float _checkControllerElapsedTime = 0f;
    private const float _checkControllerInterval = 1f;

    #endregion 

    #region Lifecycle Management

    private void Awake()
    {
        Services.InitializeServices(this);

        _fsm = new FiniteStateMachine<GameManager>(this);
    }

    void Start()
    {
        _fsm.TransitionTo<StartMenu>();
    }

    void Update()
    {
        _fsm.Update();
        SequenceManager.Update();
        _checkControllerElapsedTime += Time.deltaTime;
        if (_checkControllerElapsedTime > _checkControllerInterval)
        {
            _checkControllerElapsedTime = 0f;
            InputManager.CheckUsingController();
        }
    }

    #endregion

    #region Triggers.

    public void NewGame()
    {
        if (!_gameStarted)
        {
            _gameStarted = true;
            _fsm.TransitionTo<StartPlay>();
        }
        else
        {
            Services.SaveManager.NewGameSave();
            LoadSave();
        }
    }

    public void LoadSave()
    {
        if (!_gameStarted)
        {
            if (Services.SaveManager.SaveExists())
                StartCoroutine(LoadSaveCO());
            else
                Logger.Debug("Trying to load a save that does not exist");
        }
        else
        {
            _fsm.TransitionTo<StartPlay>();
        }
    }

    private IEnumerator LoadSaveCO()
    {
        Services.UIManager.EnterLoadSave();
        Coroutine loadCO = StartCoroutine(Services.SaveManager.LoadDataCO());
        yield return loadCO;
        _fsm.TransitionTo<StartPlay>();
    }

    // Called on Pause.
    public void Pause() => _fsm.TransitionTo<PauseState>();

    // Called on Unpause.
    public void Unpause() => _fsm.TransitionTo<PlayState>();

    // Called on Enter dialogue.
    public void EnterDialogue() => _fsm.TransitionTo<InDialogueState>();

    // Called on Exit dialogue.
    public void ReturnToPlay()
    {
        if (_endingGame)
            EndGame();
        else
            _fsm.TransitionTo<PlayState>();
    }

    // Called on Quest item trigger.
    public void MidrollCutscene() => _fsm.TransitionTo<MidCutsceneState>();

    public void EndCutscene() => _fsm.TransitionTo<EndCutsceneState>();

    public void EndGame() => _fsm.TransitionTo<EndGameState>();

    // Called when the player goes to main menu.
    public void MainMenu()
    {
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
        SequenceManager.Save();
#endif
    }

    // Called on Quit.
    public void Quit() => Application.Quit();

    #endregion

    #region States

    private abstract class GameState : FiniteStateMachine<GameManager>.State { }

    private class StartMenu : GameState
    {
        public override void OnEnter()
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            Logger.Debug("GameManager: Entered StartMenu");
            Services.EventManager.Fire(new OnStartMenu());
        }

        public override void OnExit()
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    // Probably not needed?
    private class StartPlay : GameState
    {
        public override void OnEnter()
        {
            Logger.Debug("GameManager: Entered StartPlay");
            FModMusicManager.EndFoxTheme();
        }

        public override void Update()
        {
            base.Update();
            // DELETE

            TransitionTo<PlayState>();
            InputManager.ProcessPlayInput();
        }

        public override void OnExit()
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    private class PlayState : GameState
    {

        public override void OnEnter()
        {
            if (Context._endingGame)
            {
                TransitionTo<EndGameState>();
            }
            Logger.Debug("GameManager: Entered PlayState");

            Services.EventManager.Fire(new OnEnterPlay());
        }

        public override void Update()
        {
            base.Update();

            InputManager.ProcessPlayInput();
        }

        public override void OnExit() { }
    }

    private class PauseState : GameState
    {
        public override void OnEnter()
        {
            Logger.Debug("GameManager: Entered Pause");
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            
            Services.EventManager.Fire(new OnPause());
        }

        public override void Update()
        {
            base.Update();

            InputManager.ProcessPauseMenuInput();
        }

        public override void OnExit()
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    private class InDialogueState : GameState
    {

        public override void OnEnter()
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            Services.EventManager.Fire(new OnEnterDialogue());
        }

        public override void OnExit()
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            NPCInteractionManager.ExitDialogue();
        }
    }

    // Use delegates to control player movement, show item taken away, and coordinate UI and sound.
    private class MidCutsceneState : GameState
    {
        /*
         * 1. Move player to position. Move camera behind player. ~2s
         * 2. Move Camera to cutsceneCamera, have camera slowly focus on item. Make player walk to tree. ~2s
         * 3. Trigger Quest item repository to take item, trigger sounds and particle effects, smoothly animate it into position and have large particle effect. 2s
         * 4. Fade to white? black? 2s
         * 5. stay there for a sec as music fades. Place player into new position. 3s
         * 6. Fade back in and have player turned around as environment changes are triggered. 2s
         * 7. 1 sec later have player get up and return to normal controls. 1s // turns around!
         */

        

        public override void OnEnter() => Services.EventManager.Fire(new OnEnterMidCutscene());
    }

    // Use delegates to control player movement, show item taken away, and coordinate UI and sound.
    // FIX COMMENTS!!!!!!!!!!!!!!!!!!!!!!!!!!!!
    private class EndCutsceneState : GameState
    {


        public override void OnEnter() => Services.EventManager.Fire(new OnEnterEndCutscene());

        public override void OnExit() => Context._endingGame = true;

    }

    // Use delegates to control player movement, Fade out screen, load main menu.
    private class EndGameState : GameState
    {
        public override void OnEnter() => Services.EventManager.Fire(new OnEnterEndGame());
    }

    #endregion

}
