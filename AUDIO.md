# Adaptive score

The game includes fifteen original synthesized stereo cues, built without external samples. Rebuild the WAV files with `python scripts/generate-music.py` (numpy required). Godot imports them as compressed looping streams; keep their `.import` settings in source control.

Meadows, woodland, outskirts, city and underground have regional arrangements. Night has a darker arrangement. Nearby sleeping bags and friendly non-Ledger settlements use the home theme. Combat overrides exploration only after an engaged enemy has line of sight or is very close, or the player takes damage. Hidden enemies alone never trigger music. A ten-second danger hold avoids rapid switching; regional changes require three seconds of stability. Four-second fades blend tracks. Quiet exploration includes forty seconds of silence every 200 active seconds.

Foreman, Wrecker, Bellkeeper, Broodheart and the three mega bosses have separate arrangements, with a modest intensity lift below half health and a softer recovery phase. Birthday-house music suppresses exploration scoring nearby. The fever cinematic and recent damage lower score volume. These are synthesized musical arrangements, not recorded instrumental performances.

Escape → Music Volume adjusts music independently and saves to `user://audio.cfg`, outside the installation folder. U pauses all audio. Menus pause the score even in co-op; music is local to each player's situation. Headless servers and automated gameplay fixtures do not start the score.

`--music-check` validates all fifteen imported loops, starting-region and night selection, damage escalation and release, camp selection, silent periods, pause and mute. The standard build script runs it before integration and co-op checks.
