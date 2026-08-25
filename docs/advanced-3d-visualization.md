# Advanced 3D Visualization

## Status

This document specifies **Milestone 6: Advanced 3D Visualization and Realistic Robot Rendering**. Milestone 6 is the current development focus. Renderer integration must begin only after its product boundary, asset strategy, and library evaluation are explicitly settled.

`TODO.md` is the authoritative checklist for milestone progress. This document records the architectural constraints and intended outcome behind that checklist.

## Purpose

RobotStudio currently uses intentionally simple geometry, strong axis colors, grids, workspace bounds, paths, labels, and other schematic elements. That renderer has lasting pedagogical value and must remain available.

Milestone 6 adds a second visualization path for detailed robot models, mechanical parts, richer materials, lighting, shadows, textures, and PBR rendering where appropriate. This path is a mechanical showcase: it identifies components, explains how they relate, and uses short predefined movements to demonstrate the mechanism. It is not a replacement script executor or a second full simulation engine. Realism must improve student understanding and engagement without hiding coordinate systems, robot topology, or the relationship between components and physical movement.

## Visualization Modes

The future desktop application must support three composable modes:

1. **Schematic / Didactic Simulation**: the existing executable workspace, optimized for commands, planning, deterministic playback, mathematical clarity, and technical overlays.
2. **Realistic Mechanical Showcase**: detailed models, proportions, materials, lighting, component inspection, and short fixed demonstrations of mechanical operation.
3. **Realistic + Educational Overlays**: realistic models combined with relevant coordinate systems, labels, selected-part highlights, movement relationships, and other teaching aids.

The exact navigation between simulation and showcase remains a product decision for the first Milestone 6 increment. Entering the showcase must not alter a retained simulation session. Overlays should be independent scene layers so compatible teaching aids can be reused.

The catalog will initially expose two distinct actions for available robots: `Open Simulator` and `Explore Mechanics`. A later usability evaluation may add an internal switch, but the two experiences must remain conceptually distinct and must not crowd the simulation workspace with showcase-only controls.

## Visual And Interaction Direction

The target is stylized technical realism rather than photorealism, branded reproduction, or manufacturing-grade CAD. RobotStudio models must be original, generic teaching models with mechanically plausible proportions, recognizable assemblies, appropriately differentiated metal, plastic, rubber, cable, and fastener details, and convincing joint or actuator behavior. Assets must not copy manufacturer branding, trade dress, labels, or proprietary product geometry.

Visual fidelity may approach a polished industrial product presentation where it improves understanding, but matching photographic reference quality is not a requirement. Geometry detail, texture resolution, materials, lighting, shadows, and effects must remain subordinate to educational clarity, maintainability, loading cost, and performance on representative student computers. A complete factory cell is not required for every robot; contextual equipment should be included only when it explains operation, scale, mounting, safety, or interaction with the environment.

User freedom in the realistic showcase is intentionally limited. Users may inspect the model, control the camera, select semantic components, choose a curated demonstration, and use the educational viewing aids supplied for that model. They do not edit geometry or author arbitrary realistic animations.

Each robot may use the most legible internal-view technique for its mechanism:

- selective shell transparency when internal and external relationships remain clear;
- a curated cutaway when overlapping transparent surfaces would obscure the mechanism;
- a controlled exploded view when assembly order or connection between parts is the lesson.

These are authored teaching views, not unrestricted CAD editing tools.

## Architectural Boundary

The deterministic simulation is the source of truth. Renderers only consume simulation state and must never define robot position by moving a graphical object.

```txt
Domain, motion, and deterministic simulation
                    |
                    v
        Schematic rendering adapter
                    |
                    v
           Schematic renderer

Showcase definition and curated demonstration
                    |
                    v
 Presentation-only component pose controller
                    |
                    v
           Realistic renderer

Both rendering paths may compose compatible educational overlays.
Neither rendering path writes state back into the simulator.
```

The layers have distinct responsibilities:

- `RobotStudio.Domain` owns robot concepts, commands, limits, and state rules. It must not reference rendering or asset types.
- `RobotStudio.Motion` owns planning and kinematics. It must not reference rendering or asset types.
- `RobotStudio.Simulation` owns deterministic timelines and renderer-neutral robot state or component poses. It must not reference WPF, HelixToolkit, Stride, GPU resources, meshes, materials, or cameras.
- `RobotStudio.Desktop` owns renderers, scene graphs, assets, cameras, lighting, materials, interpolation for display, hit testing, and overlays.

The existing Cartesian scene primitives remain valid inputs for its schematic renderer. They must not become the universal contract for realistic rendering or for every robot topology. Milestone 6 should introduce small renderer-neutral pose/component contracts or adapters only after examining the needs shared by the existing Cartesian, mobile, articulated, parallel, and aerial models.

Simulation ticks and rendering frames must remain independent. Rendering may interpolate between immutable simulation samples for visual smoothness, but interpolation must not mutate deterministic results or feed visual state back into the simulator.

## Current Codebase Observations

The current architecture already provides useful boundaries for this milestone:

- WPF and `System.Windows.Media.Media3D` are confined to `RobotStudio.Desktop`.
- Domain, Motion, and Simulation do not reference a rendering framework.
- Shared playback contracts expose family-specific deterministic frames without forcing every robot into Cartesian coordinates.
- `RobotStudio.Desktop.Rendering` already centralizes basic camera, lighting, mesh, and pointer-interaction helpers.

Milestone 6 must address these constraints before realistic rendering grows across robot families:

- `MainWindow.xaml.cs` currently coordinates playback and contains robot-specific scene construction for multiple families. Renderer selection, scene composition, and viewport lifecycle need dedicated desktop services or presenters before a second renderer is integrated.
- Cartesian playback includes schematic `CartesianSceneFrame` primitives, while other viewers compose their geometry directly from family-specific playback frames. The project needs a deliberate adapter boundary instead of promoting either approach into a universal realistic-rendering contract.
- Current WPF viewers commonly rebuild scene models for rendered frames. A realistic renderer should retain a scene hierarchy and update component transforms where practical.
- Stable semantic part identifiers, hierarchical visual models, component poses, and a version 1 visual-asset manifest now exist. Desktop package discovery validates and caches manifests, resolves local GLB files, imports HelixToolkit scene hierarchies through Assimp, and binds named nodes to semantic parts. The Cartesian showcase consumes the first original packaged GLB, updates semantic component roots without rebuilding meshes, reuses authored mesh/material definitions, and explicitly disposes the imported scene. Cross-package GPU resource caching remains pending. Raw asset node names do not become public domain contracts.
- The Cartesian assembled and drive-system teaching views now compose different appearances over one imported hierarchy. Semantic part kinds determine transparency and technical highlighting, while selecting a ghosted component preserves the cutaway instead of making the obstruction opaque again.

## Robot Visual Models

A realistic robot must be assembled from a hierarchy of meaningful components rather than treated as one indivisible mesh. Depending on the family, components may include bases, rails, carriages, axes, joints, links, rotors, platforms, tools, and end effectors.

Each selectable visual node must map to a stable RobotStudio semantic part identifier. Raw mesh names and renderer object references are implementation details. This mapping will allow selection to expose a component's name, axis or joint, pose, velocity, limits, coordinates, and educational description without coupling those concepts to a graphics library.

The visual hierarchy describes how a model is displayed; domain and simulation types continue to describe how the robot behaves.

Semantic metadata should also describe each inspectable component's teaching name, mechanical purpose, parent or connected parts, movement type, and relevant demonstration. This educational information must not be embedded only in mesh node names or UI event handlers.

## Asset Direction

glTF 2.0 is the preferred interoperable format, with `.glb` favored for packaged application assets when convenient. RobotStudio must not depend on proprietary model formats.

Visual data belongs in the model asset. RobotStudio-specific semantics use a separate, versioned manifest that maps asset node names to stable `RobotPartId` values. Version 1 intentionally contains only `schemaVersion`, `modelId`, `assetFile`, and `nodes`; it does not encode materials, animation, cameras, demonstrations, or renderer objects. Several asset nodes may map to one semantic part, and every selectable visual-model part must have at least one mapping.

The portable visualization layer parses and validates manifests, including schema/model compatibility, safe relative `.glb` paths, unique node names, known semantic parts, and complete selectable-part coverage. The desktop layer resolves package files, reports missing manifests or assets deterministically, caches validated package metadata, imports GLB scene nodes through the isolated Assimp adapter, and applies semantic identifiers to imported subtrees. An explicitly mapped child starts a new semantic subtree, allowing authored hierarchies to preserve meaningful assemblies. The first Cartesian package proves this path with reusable mesh/material definitions; caching GPU resources across separately loaded package instances remains future work. The complete contract is documented in [Visual Asset Manifest](visual-asset-manifest.md).

## Rendering Technology Evaluation

RobotStudio must use rendering technology that remains available without paid licenses, subscriptions, royalties, or commercial licensing fees if the project becomes public, open source, redistributed, or commercial. Permissive open-source licensing is strongly preferred.

| Candidate | Direction | Rationale |
| --- | --- | --- |
| HelixToolkit | Preferred | MIT-licensed and aligned with .NET engineering visualization. If WPF remains in use, investigate the current `HelixToolkit.Wpf.SharpDX` integration first rather than assuming it is still the correct package. |
| Stride | Advanced alternative | MIT-licensed and appropriate if PBR, lighting, shadows, post-processing, or scene complexity outgrow HelixToolkit, at the cost of game-engine-level architectural complexity. |
| Veldrid | Low-level fallback, not preferred | MIT-licensed and flexible, but would require RobotStudio to own substantially more rendering infrastructure than this educational robotics application should normally maintain. |
| Ab4d.SharpEngine | Rejected as the default | Technically suitable, but potential commercial licensing requirements conflict with the zero-license-cost dependency policy and could make future distribution choices conditional. |

Package health, licensing, framework compatibility, platform support, and maintenance status must be revalidated before the first Milestone 6 renderer proof of concept. No dependency is introduced by this initial scope-alignment decision.

The current revalidation selects `HelixToolkit.Wpf.SharpDX` 3.1.2 as the proof-of-concept candidate while retaining Stride as the first alternative. This is a provisional implementation choice rather than permanent approval. The evidence, risks, asset pipeline, and acceptance criteria are recorded in [Renderer And Asset Pipeline Evaluation](renderer-evaluation.md).

## Performance Direction

Milestone 6 must evaluate:

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

The implementation is successful only if students can distinguish executable simulation from mechanical demonstration, move between both without losing their simulation session, and understand how realistic components produce the abstract motion shown by the simulator.

## Non-Goals

The current planning task does not include:

- integrating HelixToolkit, Stride, Veldrid, or another renderer;
- loading glTF/GLB models;
- creating realistic meshes, materials, lighting, shadows, or environments;
- implementing object picking or collision visualization;
- adding new robot families;
- removing or rewriting the schematic renderer.

## Completion Criteria

Milestone 6 is complete only when:

- schematic visualization remains fully available;
- the realistic showcase uses explicit renderer-neutral component poses or animation state and never treats mesh transforms as domain state;
- predefined showcase demonstrations remain presentation-only and cannot mutate an active deterministic simulation session;
- no domain, motion, or simulation project depends on a rendering library;
- all eight currently available models have an original realistic mechanical showcase: Cartesian Robot, XY Plotter, Differential Drive Robot, SCARA Robot, Simple Articulated Arm, Delta Robot, Drone, and 6-DOF Industrial Arm;
- every showcase uses a validated glTF/GLB-based visual hierarchy or another explicitly approved open asset representation selected during the renderer proof of concept;
- selectable visual components resolve to RobotStudio semantic identifiers;
- simulation timing remains independent from rendering frame timing;
- the selected dependency satisfies the zero-license-cost policy;
- architecture, asset validation, selection mapping, and rendering smoke tests protect the new boundaries.

Milestone completion is capability-based rather than calendar-based. The next product release requires the selected work from Milestones 2 through 6 to be genuinely complete. Milestone 7 robot-family expansion remains a candidate for a later release and does not block that release.
