# Manual Jogging Lesson

Goal: observe how manual controls become the same commands used by scripts and playback.

1. Open the Cartesian Robot and select Simple DSL.
2. Set `Step mm` to `10` and `Speed mm/s` to `40`.
3. Press `HOME`, `X+`, `X+`, `Y+`, and `Z+` in that order.
4. Execute `WAIT 700` in the command console.
5. Press `HOME` again.
6. Inspect the generated script, timeline states, TCP path, and final position.

Expected observations:

- each jog adds an absolute `MOVE` target using the selected step and speed;
- `WAIT` advances simulated time without changing the TCP position;
- command order is preserved in the timeline;
- the final `HOME` returns the TCP to `X=0`, `Y=0`, `Z=0`;
- selecting G-code before repeating the activity produces equivalent G-code commands.

The executable `jog-wait-home.robot` and `jog-wait-home.gcode` files reproduce the same movement sequence without requiring button input.
