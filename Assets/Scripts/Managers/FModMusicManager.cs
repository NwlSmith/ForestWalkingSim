using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMOD.Studio;

public static class FModMusicManager
{

    private static EventInstance musicSoundState;

    private static EventInstance itemFoundSoundState;
    private static EventInstance itemReturnedSoundState;
    private static EventInstance endCutsceneSoundState;
    private static EventInstance[] uiSoundStates = new EventInstance[3];
    private static int curUISoundState = 0;

    private static Dictionary<string, PARAMETER_ID> StrToID = new Dictionary<string, PARAMETER_ID>();

    public static void Init()
    {

        musicSoundState = FMODUnity.RuntimeManager.CreateInstance("event:/Music");
        CompileSounds();
        musicSoundState.start();

        itemFoundSoundState = FMODUnity.RuntimeManager.CreateInstance("event:/Object Acquired");
        itemReturnedSoundState = FMODUnity.RuntimeManager.CreateInstance("event:/Object Returned");

        for (int i = 0; i < 3; i++)
        {
            uiSoundStates[i] = FMODUnity.RuntimeManager.CreateInstance("event:/UI");
        }
    }

    public static void OnDestroy()
    {

        musicSoundState.clearHandle();

        itemFoundSoundState.clearHandle();
        itemReturnedSoundState.clearHandle();

        for (int i = 0; i < 3; i++)
        {
            uiSoundStates[i].clearHandle();
        }
    }

    private static void CompileSounds()
    {
        StrToID.Clear();
        EventDescription soundEventDescription;
        musicSoundState.getDescription(out soundEventDescription);
        PARAMETER_DESCRIPTION soundParameterDescription;

        // ADD OTHER ANIMALS

        soundEventDescription.getParameterDescriptionByName("Fox Theme", out soundParameterDescription);
        StrToID.Add("Fox Theme", soundParameterDescription.id);

        soundEventDescription.getParameterDescriptionByName("Layer 1", out soundParameterDescription);
        StrToID.Add("Layer 1", soundParameterDescription.id);

        soundEventDescription.getParameterDescriptionByName("Layer 2", out soundParameterDescription);
        StrToID.Add("Layer 2", soundParameterDescription.id);

        soundEventDescription.getParameterDescriptionByName("Layer 3", out soundParameterDescription);
        StrToID.Add("Layer 3", soundParameterDescription.id);

        soundEventDescription.getParameterDescriptionByName("End", out soundParameterDescription);
        StrToID.Add("End", soundParameterDescription.id);
        
        soundEventDescription.getParameterDescriptionByName("Spirit Speaking", out soundParameterDescription);
        StrToID.Add("Spirit Speaking", soundParameterDescription.id);
        
        soundEventDescription.getParameterDescriptionByName("Warbler Speaking", out soundParameterDescription);
        StrToID.Add("Warbler Speaking", soundParameterDescription.id);

        soundEventDescription.getParameterDescriptionByName("Baby Bird Speaking", out soundParameterDescription);
        StrToID.Add("Baby Bird Speaking", soundParameterDescription.id);

        soundEventDescription.getParameterDescriptionByName("Toad Speaking", out soundParameterDescription);
        StrToID.Add("Toad Speaking", soundParameterDescription.id);

        soundEventDescription.getParameterDescriptionByName("Frog Speaking", out soundParameterDescription);
        StrToID.Add("Frog Speaking", soundParameterDescription.id);
        
        soundEventDescription.getParameterDescriptionByName("Turtle Speaking", out soundParameterDescription);
        StrToID.Add("Turtle Speaking", soundParameterDescription.id);
    }

    public static void PlayTrack(string track) => musicSoundState.setParameterByID(StrToID[track], 1);

    public static void EndTrack(string track) => musicSoundState.setParameterByID(StrToID[track], 0);

    public static void EndFoxTheme() => musicSoundState.setParameterByID(StrToID["Fox Theme"], 0);

    public static void StartFoxTheme() => musicSoundState.setParameterByID(StrToID["Fox Theme"], 1);

    public static void EndMusicLayers()
    {
        musicSoundState.setParameterByID(StrToID["Layer 1"], 0);
        musicSoundState.setParameterByID(StrToID["Layer 2"], 0);
        musicSoundState.setParameterByID(StrToID["Layer 3"], 0);
    }

    public static void FoundItem() => itemFoundSoundState.start();

    public static void ReturnedItem() => itemReturnedSoundState.start();

    public static void EndCutscene() => musicSoundState.setParameterByName("End", 1);

    public static void PlayUISound()
    {
        curUISoundState = (curUISoundState + 1) % 3;
        uiSoundStates[curUISoundState].start();
    }
}
