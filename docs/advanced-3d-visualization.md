# Advanced 3D Visualization

## Status

This document specifies **Milestone 9: Advanced 3D Visualization and Realistic Robot Rendering**. It is future work. It does not authorize renderer integration, package installation, model import, or changes to the current schematic viewers during the present development phase.

`TODO.md` is the authoritative checklist for milestone progress. This document records the architectural constraints and intended outcome behind that checklist.

## Purpose

RobotStudio currently uses intentionally simple geometry, strong axis colors, grids, workspace bounds, paths, labels, and other schematic elements. That renderer has lasting pedagogical value and must remain available.

Milestone 9 adds a second visualization path for detailed robot models, mechanical parts, richer materials, lighting, shadows, textures, and PBR rendering where appropriate. Realism must improve student understanding and engagement without hiding coordinate systems, robot topology, motion constraints, or the relationship between commands and physical movement.

## Visualization Modes

The future desktop application must support three composable modes:

1. **Schematic / Didactic**: the existing style, optimized for mathematical and mechanical clarity.
2. **Realistic**: detailed models, proportions, materials, lighting, and environments.
3. **Realistic + Educational Overlays**: realistic models combined with axes, coordinate systems, workspace limits, trajectories, labels, selected-part highlights, and future collision bounds.

Mode switching must not restart or alter the simulation. Overlays should be independent scene layers so the same teaching aid can be reused across compatible renderers.

## Architectural Boundary

The deterministic simulation is the source of truth. Renderers only consume simulation state and must never define robot position by moving a graphical object.

```txt
Domain and deterministic simulation
              |
              v
Renderer-neutral state, poses, and semantic part identifiers
              |
       +------+------+
       |             |
       v             v
Schematic renderer   Realistic renderer
       \             /
        +---- overlays
```

The layers have distinct responsibilities:

- `RobotStudio.Domain` owns robot concepts, commands, limits, and state rules. It must not reference rendering or asset types.
- `RobotStudio.Motion` owns planning and kinematics. It must not reference rendering or asset types.
- `RobotStudio.Simulation` owns deterministic timelines and renderer-neutral robot state or component poses. It must not reference WPF, HelixToolkit, Stride, GPU resources, meshes, materials, or cameras.
- `RobotStudio.Desktop` owns renderers, scene graphs, assets, cameras, lighting, materials, interpolation for display, hit testing, and overlays.

The existing Cartesian scene primitives remain valid inputs for its schematic renderer. They must not become the universal contract for realistic rendering or for every robot topology. Milestone 9 should introduce small renderer-neutral pose/component contracts or adapters only after examining the needs shared by the existing Cartesian, mobile, articulated, parallel, and aerial models.

Simulation ticks and rendering frames must remain independent. Rendering may interpolate between immutable simulation samples for visual smoothness, but interpolation must not mutate deterministic results or feed visual state back into the simulator.

## Current Codebase Observations

The current architecture already provides useful boundaries for this milestone:

- WPF and `System.Windows.Media.Media3D` are confined to `RobotStudio.Desktop`.
- Domain, Motion, and Simulation do not reference a rendering framework.
- Shared playback contracts expose family-specific deterministic frames without forcing every robot into Cartesian coordinates.
- `RobotStudio.Desktop.Rendering` already centralizes basic camera, lighting, mesh, and pointer-interaction helpers.

Milestone 9 must address these constraints before realistic rendering grows across robot families:

- `MainWindow.xaml.cs` currently coordinates playback and contains robot-specific scene construction for multiple families. Renderer selection, scene composition, and viewport lifecycle need dedicated desktop services or presenters before a second renderer is integrated.
- Cartesian playback includes schematic `CartesianSceneFrame` primitives, while other viewers compose their geometry directly from family-specific playback frames. The project needs a deliberate adapter boundary instead of promoting either approach into a universal realistic-rendering contract.
- Current WPF viewers commonly rebuild scene models for rendered frames. A realistic renderer should retain a scene hierarchy and update component transforms where practical.
- Stable semantic part identifiers, visual asset manifests, asset caches, and mesh-to-domain selection mappings do not exist yet. They should be introduced together so raw asset node names never become public domain contracts.

## Robot Visual Models

A realistic robot must be assembled from a hierarchy of meaningful components rather than treated as one indivisible mesh. Depending on the family, components may include bases, rails, carriages, axes, joints, links, rotors, platforms, tools, and end effectors.

Each selectable visual node must map to a stable RobotStudio semantic part identifier. Raw mesh names and renderer object references are implementation details. This mapping will allow selection to expose a component's name, axis or joint, pose, velocity, limits, coordinates, and educational description without coupling those concepts to a graphics library.

The visual hierarchy describes how a model is displayed; domain and simulation types continue to describe how the robot behaves.

## Asset Direction

glTF 2.0 is the preferred interoperable format, with `.glb` favored for packaged application assets when convenient. RobotStudio must not depend on proprietary model formats.

Visual data belongs in the model asset. RobotStudio-specific semantics may use separate, versioned metadata that maps asset nodes to axes, joints, links, tools, transform rules, and other stable identifiers. The exact manifest schema is intentionally deferred until Milestone 9 so current examples do not become accidental contracts.

Future asset loading must validate versions and required semantic mappings, report failures clearly, and cache reusable models, meshes, textures, and materials.

## Rendering Technology Evaluation

RobotStudio must use rendering technology that remains available without paid licenses, subscriptions, royalties, or commercial licensing fees if the project becomes public, open source, redistributed, or commercial. Permissive open-source licensing is strongly preferred.

| Candidate | Direction | Rationale |
| --- | --- | --- |
| HelixToolkit | Preferred | MIT-licensed and aligned with .NET engineering visualization. If WPF remains in use, investigate the current `HelixToolkit.Wpf.SharpDX` integration first rather than assuming it is still the correct package. |
| Stride | Advanced alternative | MIT-licensed and appropriate if PBR, lighting, shadows, post-processing, or scene complexity outgrow HelixToolkit, at the cost of game-engine-level architectural complexity. |
| Veldrid | Low-level fallback, not preferred | MIT-licensed and flexible, but would require RobotStudio to own substantially more rendering infrastructure than this educational robotics application should normally maintain. |
| Ab4d.SharpEngine | Rejected as the default | Technically suitable, but potential commercial licensing requirements conflict with the zero-license-cost dependency policy and could make future distribution choices conditional. |

Package health, licensing, framework compatibility, platform support, and maintenance status must be revalidated when Milestone 9 begins. No dependency is introduced by this planning decision.

## Performance Direction

Milestone 9 must evaluate:

- scene graph organization and incremental transform updates;
- GPU-side rendering and resource lifetime;
- mesh, texture, and material reuse;
- asynchronous model loading and caching;
- selection and hit-testing cost;
- simulation sample consumption and render interpolation;
- independent simulation and rendering update rates;
- representative performance on student hardware.

The renderer should update component transforms for each frame instead of rebuilding complete robot scenes when the selected technology supports that approach.

## Educational Requirements

The realistic renderer must continue to teach coordinate systems, axes, joints, workspace, trajectories, end-effector position, robot topology, and the physical effect of commands. Selection and overlays must provide technical context, not merely decoration.

The implementation is successful only if students can switch between abstraction and realism while observing the same deterministic simulation.

## Non-Goals

The current planning task does not include:

- integrating HelixToolkit, Stride, Veldrid, or another renderer;
- loading glTF/GLB models;
- creating realistic meshes, materials, lighting, shadows, or environments;
- implementing object picking or collision visualization;
- adding new robot families;
- removing or rewriting the schematic renderer.

## Completion Criteria

Milestone 9 is complete only when:

- schematic visualization remains fully available;
- realistic and overlay-composed modes consume the same deterministic simulation results;
- no domain, motion, or simulation project depends on a rendering library;
- at least one existing robot uses a validated glTF/GLB-based visual hierarchy;
- selectable visual components resolve to RobotStudio semantic identifiers;
- simulation timing remains independent from rendering frame timing;
- the selected dependency satisfies the zero-license-cost policy;
- architecture, asset validation, selection mapping, and rendering smoke tests protect the new boundaries.
