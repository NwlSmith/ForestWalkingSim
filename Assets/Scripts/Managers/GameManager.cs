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
 * 
 * not clear where end of race is
 * 
 * STILL DIALOGUE ENTRY BUG
 * 
 * also people aren't able to get into dialogue properly after the cutscene?
 * 
 * Save is disabled in SaveManager at Line 177 and UIManager at Line 185
 * 
 * 
 * spirit fox tails are freaking out again
 * end song didn't trigger in time?
 * figure out flower materials - apply root animation?
 * fix path to heart
 * holy shit wtf happened at the end??
 * add grass to heart batch 1
 * fix issue of item appearing above player's head?
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

    private void OnDestroy()
    {
        Services.OnDestroy();
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
        if (!_endingGame)
             _fsm.TransitionTo<PlayState>();
    }

    // Called on Quest item trigger.
    public void MidrollCutscene() => _fsm.TransitionTo<MidCutsceneState>();

    public void EndCutscene()
    {
        _endingGame = true;
        _fsm.TransitionTo<EndCutsceneState>();
    }

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
                TransitionTo<EndCutsceneState>();
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

        private float _elapsedTime = 0f;
        private const float _maxTimeUntilFailsafe = 10f;

        public override void OnEnter()
        {
            _elapsedTime = 0f;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            Logger.Warning("GameManager entering dialogue");
            Services.EventManager.Fire(new OnEnterDialogue());
        }

        public override void Update()
        {
            base.Update();
            _elapsedTime += Time.deltaTime;
            if (Services.DialogueController.inDialogue)
                _elapsedTime = 1000f;
            else if (_elapsedTime > _maxTimeUntilFailsafe && _elapsedTime < _maxTimeUntilFailsafe + .5f)
            {
                _elapsedTime = 1000f;
                Logger.Warning("Dialogue Entry failsafe triggered in GameManager InDialogueState.");
                SequenceManager.EnterDialogue();
                Services.UIManager.HideDialogueEnterPrompt();
            }
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

        private float _elapsedTime = 0f;
        private const float _maxTimeUntilFailsafe = 10f;

        public override void OnEnter()
        {
            _elapsedTime = 0f;
            Services.EventManager.Fire(new OnEnterMidCutscene());
        }

        public override void Update()
        {
            base.Update();
            _elapsedTime += Time.deltaTime;
            if (SequenceManager.inCutscene)
                _elapsedTime = 1000f;
            else if (_elapsedTime > _maxTimeUntilFailsafe && _elapsedTime < _maxTimeUntilFailsafe + .5f)
            {
                _elapsedTime = 1000f;
                Logger.Warning("Cutscene Entry failsafe triggered in GameManager MidCutsceneState.");
                SequenceManager.EnterMidCutscene();
                Services.CameraManager.EnterMidCutsceneFailsafe();
                Services.UIManager.HideDialogueEnterPrompt();
            }
        }
    }

    // Use delegates to control player movement, Fade out screen, load main menu.
    private class EndCutsceneState : GameState
    {
        private float _elapsedTime = 0f;
        private const float _maxTimeUntilFailsafe = 10f;

        public override void OnEnter()
        {
            _elapsedTime = 0f;
            Services.EventManager.Fire(new OnEnterEndCutscene());
        }

        public override void Update()
        {
            base.Update();
            _elapsedTime += Time.deltaTime;
            if (SequenceManager.inCutscene)
                _elapsedTime = 1000f;
            else if (_elapsedTime > _maxTimeUntilFailsafe && _elapsedTime < _maxTimeUntilFailsafe + .5f)
            {
                _elapsedTime = 1000f;
                Logger.Warning("Cutscene Entry failsafe triggered in GameManager EndCutsceneState.");
                SequenceManager.EnterEndCutscene();
                Services.CameraManager.EnterEndCutsceneFailsafe();
                Services.UIManager.HideDialogueEnterPrompt();
            }
        }

        public override void OnExit() => Context._endingGame = true;

    }

    #endregion

}
