/*
 * Creator: Nate Smith
 * Creation Date: 4/11/2021
 * Description: Manager class that controls sequences.
 */
public static class SequenceManager
{
    public static bool inCutscene = false;
    private static readonly TaskManager _taskManager = new TaskManager();

    private static Task _enterDialogue;
    private static Task _enterDialogueFailsafe;
    private static Task _enterMidSequence;
    private static Task _enterEndSequence;
    private static Task _enterEndGameSequence;
    private static CutsceneObjectsManager cutsceneObjectsManager;

    public static bool[] specialCutsceneTriggers = new bool[4];

    public static void Init()
    {
        // Pre-define task.
        _enterDialogueFailsafe = DefineDialogueFailsafeSequence();
        _enterDialogue = DefineDialogueSequence();
        _enterMidSequence = DefineMidSequence();
        _enterEndSequence = DefineEndSequence();

        RegisterEvents();

        for (int i = 0; i < specialCutsceneTriggers.Length; i++)
        {
            specialCutsceneTriggers[i] = false;
        }
        cutsceneObjectsManager = UnityEngine.Object.FindObjectOfType<CutsceneObjectsManager>();
    }

    public static void OnDestroy() => UnregisterEvents(); // Possibly not useful.

    public static void Update() => _taskManager.Update();

    private static void RegisterEvents()
    {
        Services.EventManager.Register<OnEnterDialogue>(EnterDialogue);
        Services.EventManager.Register<OnEnterMidCutscene>(EnterMidCutscene);
        Services.EventManager.Register<OnEnterEndCutscene>(EnterEndCutscene);
    }

    private static void UnregisterEvents()
    {
        Services.EventManager.Unregister<OnEnterDialogue>(EnterDialogue);
        Services.EventManager.Unregister<OnEnterMidCutscene>(EnterMidCutscene);
        Services.EventManager.Unregister<OnEnterEndCutscene>(EnterEndCutscene);
    }

    public static void EnterDialogue()
    {
        Logger.Warning("EnterDialogue event triggered in SequenceManager");
        _enterDialogueFailsafe = DefineDialogueFailsafeSequence();
        _enterDialogue = DefineDialogueSequence();
        _taskManager.Do(_enterDialogueFailsafe);
        _taskManager.Do(_enterDialogue);
    }

    private static void EnterDialogue(AGPEvent e) => EnterDialogue();

    public static void EnterMidCutscene()
    {
        _enterMidSequence = DefineMidSequence();
        _taskManager.Do(_enterMidSequence);
    }

    private static void EnterMidCutscene(AGPEvent e) => EnterMidCutscene();

    public static void EnterEndCutscene()
    {
        _enterEndSequence = DefineEndSequence();
        _taskManager.Do(_enterEndSequence);
    }

    private static void EnterEndCutscene(AGPEvent e) => EnterEndCutscene();

    public static void Save()
    {
        Task save = new ActionTask(() => { Services.SaveManager.SaveData(); });
        Task wait = new WaitTask(.5f);
        Task finish = new ActionTask(() => { UnityEngine.SceneManagement.SceneManager.LoadScene(1); });
        save.Then(wait).Then(finish);
        _taskManager.Do(save);
    }

    private static Task DefineDialogueSequence()
    {
        
        Task enterDialogue = new DelegateTask(() => { Logger.Warning("DefineDialogueSequence start"); }, () =>
        {
            return Services.PlayerMovement.inPlaceForSequence;
        });

        ActionTask triggerCameraAndUI = new ActionTask(() =>
        {
            Logger.Warning("DefineDialogueSequence triggering camera to enter dialogue");
            Services.CameraManager.EnterDialogue();
            Services.UIManager.EnterDialogue();
        });

        Task fadeIn = new WaitTask(.5f);

        Task startConvo = new ActionTask(() =>
        {
            Logger.Warning("DefineDialogueSequence entering dialogue");
            Services.DialogueController.EnterDialogue();
            _enterDialogueFailsafe.Abort();
        });
        
        enterDialogue.Then(triggerCameraAndUI).Then(fadeIn).Then(startConvo);
        return enterDialogue;
    }

    private static Task DefineDialogueFailsafeSequence()
    {
        WaitTask failsafeWait = new WaitTask(9.1f);
        ActionTask failsafeAction = new ActionTask(() =>
        {
            if (!Services.PlayerMovement.inPlaceForSequence)
            {
                Services.CameraManager.EnterDialogue();
                Services.UIManager.EnterDialogue();
                Logger.Warning("Dialogue Failsafe Triggered in SequenceManager");
            }
        });

        failsafeWait.Then(failsafeAction);

        return failsafeWait;
    }

    /*
     * 1. Move player to position. Move camera behind player. ~2s
     * 2. Move Camera to cutsceneCamera, have camera slowly focus on item. Make player walk to tree. ~2s
     * 3. Trigger Quest item repository to take item, trigger sounds and particle effects, smoothly animate it into position and have large particle effect. 2s
     * 4. Fade to white? black? 2s
     * 5. stay there for a sec as music fades. Place player into new position. 3s
     * 6. Fade back in and have player turned around as environment changes are triggered. 2s
     * 7. 1 sec later have player get up and return to normal controls. 1s // turns around!
     */
    private static Task DefineMidSequence()
    {
        // 1. Move player to position. Move camera behind player. ~2s
        Task enterSequence = new DelegateTask(() => 
        {
            inCutscene = true;
        }, () =>
        {
            Services.UIManager.HideItemPickupPrompt();
            return Services.PlayerMovement.inPlaceForSequence;
        });

        Task waitForTime1 = new WaitTask(1f);

        // 2. Move Camera to cutsceneCamera, have camera slowly focus on item. Make player walk to tree. ~2s
        Task secondSequence = new DelegateTask(() =>
        {
            // Trigger particles?
            // Trigger music?
        }, () =>
        {
            Services.UIManager.HideItemPickupPrompt();
            return Services.PlayerMovement.inPlaceForSequence;
        });

        Task dropItem = new ActionTask(() => { Services.PlayerItemHolder.DropItem(); });

        Task waitForTime2 = new WaitTask(.66f);

        // 3.Trigger Quest item repository to take item, trigger sounds and particle effects, smoothly animate it into position and have large particle effect. 2s
        ActionTask thirdSequence = new ActionTask(() =>
        {
            Services.QuestItemRepository.StartSequence();

            FModMusicManager.ReturnedItem();
            // Quest item Repository takes Item.
            // trigger other stuff.
        });

        // Add in phase here to show plants growing?????????????????????????????????????

        Task waitForTime3 = new WaitTask(1.5f);

        // 4. Fade to white? black? 2s
        ActionTask fourthSequence = new ActionTask(() =>
        {
            // Fade out?
            Services.UIManager.CutsceneFadeIn();
        });

        // 5. stay there for a sec as music fades. Place player into new position. 3s
        Task waitForTime4 = new WaitTask(4.5f);

        // 6. Fade back in and have player turned around as environment changes are triggered. 2s
        ActionTask fifthSequence = new ActionTask(() =>
        {
            Services.PostProcessingManager.AdvanceStage();
            PlayerAnimation.Sitting(true);
            // Fade in?
            Services.UIManager.CutsceneFadeOut();
            Services.UIManager.HideDialogueEnterPrompt();
        });

        Task waitForTime5 = new WaitTask(1f);

        ActionTask triggerPlantAnims = new ActionTask(() =>
        {
            cutsceneObjectsManager.Transition();
            cutsceneObjectsManager.Transition();
            cutsceneObjectsManager.Transition();
        });

        Task waitForTime6 = new WaitTask(10.5f);
        // 7. 1 sec later have player get up and return to normal controls. 1s
        ActionTask sixthSequence = new ActionTask(() =>
        {
            PlayerAnimation.Sitting(false);
            NPCInteractionManager.FindClosestNPC();
            Services.GameManager.EnterDialogue();
            inCutscene = false;
        });

        enterSequence.Then(waitForTime1).Then(secondSequence).Then(dropItem).Then(waitForTime2).Then(thirdSequence).Then(waitForTime3).Then(fourthSequence).Then(waitForTime4).Then(fifthSequence).Then(waitForTime5).Then(triggerPlantAnims).Then(waitForTime6).Then(sixthSequence);
        return enterSequence;
    }

    private static Task DefineEndSequence()
    {
        ActionTask firstSequence = new ActionTask(() =>
        {
            Services.UIManager.CutsceneFadeIn();
            inCutscene = true;
        });
        Task waitForTime1 = new WaitTask(2f);

        ActionTask secondSequence = new ActionTask(() =>
        {
            Services.UIManager.CutsceneFadeOut();
            Services.UIManager.HideAllUI();
            PlayerAnimation.Sitting(true);
            FModMusicManager.EndCutscene();
            cutsceneObjectsManager.EndNPCs();

        });

        Task waitForTime2 = new WaitTask(14f);

        ActionTask thirdSequence = new ActionTask(() =>
        {
            Services.UIManager.CutsceneFadeIn();
        });

        Task waitForTime3 = new WaitTask(4f);

        Task endGame = new ActionTask(() =>
        {
            inCutscene = false;
            FModMusicManager.EndMusicLayers();
            UnityEngine.SceneManagement.SceneManager.LoadScene(2);
        });

        firstSequence.Then(waitForTime1).Then(secondSequence).Then(waitForTime2).Then(thirdSequence).Then(waitForTime3).Then(endGame);
        return firstSequence;
    }

}
