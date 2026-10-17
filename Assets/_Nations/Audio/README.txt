NATIONS — QUIET HORIZONS
Original synthesized music and interface sound pack

MUSIC
Music/Nations_Quiet_Horizons_LOOP_80s.wav
80 seconds • 72 BPM • D-major / B-minor palette • seamless stereo loop
Warm pads, sparse glass-like melody and soft bass. No vocals or percussion.
The separate 25-second preview has fades; use the full LOOP file in the app.

SFX / PREVIEW ORDER
01 UI Tap — ordinary button press
02 Menu Open — opening settings
03 Menu Close — returning to the globe
04 Country Discovery — selecting a country
05 Language Change — confirming a language
06 Setting Toggle — switching a setting
07 Earth Ready — first successful map load; play once, not on every tile
08 Connection Notice — a gentle notice; do not repeat on every retry

FORMAT
All assets: WAV, 48 kHz, stereo, 16-bit PCM.
Peaks leave headroom. Technical checks: valid headers, no clipped samples,
loop boundary continuity. No physical-device listening test performed.

UNITY HANDOFF
The existing ExplorationAudio.cs generates its own audio at runtime.
These files have been prepared separately; they are not installed in the project.
To use them, replace the runtime-generated clips with imported AudioClip references.
Preserve the current volume, mute and PlayerPrefs behavior.
Map existing Play(kind) values: 0=01 UI Tap, 1=02 Menu Open,
2=03 Menu Close, 3=04 Country Discovery.
Add distinct hooks for language, toggles, first load and connection notices.
Music AudioSource: loop on, spatial blend 0 (2D).
SFX AudioSource: loop off, spatial blend 0 (2D), PlayOneShot.
Suggested starting levels: music 0.25, SFX 0.55; tune on a phone speaker.
Keep the WAV masters. Audition mobile import compression before release.
Never loop the SFX preview reel in the app.

PROVENANCE
Newly composed and synthesized for this Nations request using oscillators,
original note arrangements and generated envelopes. No third-party recordings,
commercial song samples or downloaded audio were used.
This is a production provenance statement, not a copyright-registration claim.
