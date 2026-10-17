# Prepared audio integration — 2026-10-17

Imported the Music and SFX folders from the supplied Nations-Audio pack, byte-for-byte. Source files remain untouched. Only the 80-second loop and eight individual effects are game audio; neither preview reel nor validation JSON was imported. The pack README is included alongside the assets for provenance.

## Playback and mapping

The existing Globe scene has all nine AudioClip references serialized on ExplorationAudio. Its public Play(0..3), volume, mute and Save API and PlayerPrefs keys remain compatible. Runtime oscillators and generated clips were removed. Exactly two owned 2D AudioSources are created: one music loop and one PlayOneShot effects source. Imported clips are never destroyed. Music fades in and volume/mute changes are smoothed; application pause resumes the existing playback position. Music defaults to 0.25 and SFX to 0.55 only when their saved keys are absent.

| Action | Clip |
| --- | --- |
| Play(0), generic tap | 01_UI_Tap |
| Play(1), menu open | 02_Menu_Open |
| Play(2), menu close | 03_Menu_Close |
| Play(3), country selection | 04_Country_Discovery |
| LanguageChanged | 05_Language_Change |
| SettingToggled | 06_Setting_Toggle |
| EarthReady | 07_Earth_Ready |
| ConnectionNotice | 08_Connection_Notice |

The shared UI button helper no longer adds a generic sound before a specific sound. Selecting the current language or requesting the current menu state does not repeat feedback. SFX mute suppresses new effects, including its own mute confirmation; sliders adjust continuously without click spam. Existing tap-versus-drag filtering remains responsible for country selection.

GlobeRuntime emits the ready event once after a real tileset mesh renderer is enabled and visible; fallback readiness never triggers it. Audio adds a second once-per-session guard. Failure transitions emit a separate event, with a 60-second audio cooldown. Retries do not themselves emit notices. Event subscriptions are paired in OnEnable/OnDisable, and neither language changes nor menus rebuild audio.

## Import settings

Music: stereo, original 48 kHz, Streaming, Vorbis quality 0.85, background loading. SFX: stereo, original 48 kHz, PCM, Decompress On Load, preloaded. Default, Android and iOS overrides are configured. These follow Unity's [AudioImporterSampleSettings documentation](https://docs.unity.com/en-us/engine/6000.5/script-reference/unityeditor/audioimportersamplesettings). AudioPackSetup is an Editor-only repeatable wiring/import utility; it has already been executed, so no manual setup is needed.

## Verification

- Unity compilation and portrait 1080×1920 Play Mode.
- Nine valid clip references; exactly two 2D sources; stereo 80-second music.
- Menu/language actions do not reset music; Play(0..3) and specific effects hooks invoked.
- Independent volume and mute controls; existing volume preferences restored after restart.
- Imported streaming loop wraps and keeps playing. A separate temporary decode of the same Vorbis settings measured the seam against neighboring waveform slopes: see audio-loop-check.txt. The temporary asset was removed; game audio stayed streaming.
- First visible real-tile event reached audio. A simulated failure notification hook verified transition/cooldown suppression without changing credentials or network configuration.
- Original WAVs match source hashes, have valid 48 kHz stereo PCM headers and no clipped samples.

These are technical playback and waveform checks, not listening tests. Phone-speaker/headphone audition, actual network outage recovery, OS interruption behavior and physical Android performance/touch/audio testing remain outstanding. Existing Cesium large collision-triangle/account-service warnings and editor reload cancellation messages are unrelated to the audio pack.

## Files

Modified: Scripts/Audio/ExplorationAudio.cs, Scripts/UI/ExplorationUI.cs, Scripts/Core/GlobeRuntime.cs, Scenes/Globe.unity (all under Assets/_Nations).

Added: Assets/_Nations/Audio/Music/*.wav, Audio/SFX/*.wav, Audio/README.txt and Unity metadata; Editor/AudioPackSetup.cs; Editor/AudioPackChecks.cs; this document and audio-pack-checks.txt/audio-loop-check.txt. Pre-edit copies are retained under .local-backups/audio-before.
