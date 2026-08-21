# RobotStudio Examples

Examples are grouped by robot model. Each model directory contains scripts that can be loaded by the desktop app or passed to the CLI.

The Cartesian directory also contains focused teaching scenarios:

- `basic`: introductory homing, movement, and waiting;
- `relative-positioning`: absolute and relative G-code positioning;
- `invalid-axis-limit`: intentionally invalid X target for studying validation feedback;
- `speed-comparison`: requested speeds below and above axis limits;
- `jog-wait-home`: small jog-style moves, waits, sequencing, and homing.
- `manual-jogging.md`: guided desktop activity using HOME, jog buttons, and the command console.

The `.robot` files use the Simple DSL. The `.gcode` files use the introductory Cartesian G-code subset. The invalid-axis example is expected to fail validation.
