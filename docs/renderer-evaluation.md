# Renderer And Asset Pipeline Evaluation

## Status

This evaluation was revalidated at the start of Milestone 5. The pinned candidate is now integrated only in `RobotStudio.Desktop` for the Cartesian proof of concept; it does not become the permanent renderer before every proof-of-concept acceptance criterion passes.

## Decision

Use `HelixToolkit.Wpf.SharpDX` 3.1.2 as the first realistic-renderer candidate while RobotStudio remains a Windows WPF application. Use the matching `HelixToolkit.SharpDX.Assimp` package only where model import is required.

The selection is based on the current project shape and these verified capabilities:

- active 3.x development and a stable 3.1.2 release;
- MIT licensing with no subscription, royalty, or commercial-use fee;
- WPF scene-graph integration over DirectX 11;
- compatibility with `net10.0-windows` through its supported .NET targets;
- PBR and traditional materials, environment maps, shadows, SSAO, FXAA, and order-independent transparency;
- GLB/glTF import through the Assimp integration;
- model transforms, animation support, hit testing, outline or X-ray effects, and cross-section clipping;
- a smaller architectural footprint than embedding a complete game engine.

RobotStudio must pin the package version used by the proof of concept and must not mix stable 3.x packages with nightly or alpha builds.

## Alternatives

### Stride

Stride 4.3 is MIT-licensed, active, aligned with .NET 10, and technically stronger for complex environments, advanced lighting, PBR, post-processing, and game-engine-scale scenes. It remains the first alternative if HelixToolkit fails the proof of concept.

Stride is not the initial choice because its engine, editor, content pipeline, runtime model, and integration cost are disproportionate to a WPF engineering and teaching application whose realistic viewer is only one product capability.

### Veldrid

Veldrid remains a low-level fallback. Selecting it would make RobotStudio responsible for too much scene, material, loading, interaction, and rendering infrastructure.

### Ab4d.SharpEngine

Ab4d.SharpEngine remains rejected as the default because potential commercial licensing requirements conflict with RobotStudio's zero-license-cost dependency policy.

## Known Risks

- HelixToolkit 3.1.2 targets supported earlier .NET Windows frameworks rather than shipping a dedicated `net10.0-windows` assembly; compatibility must be proven by build and runtime smoke tests in RobotStudio.
- The WPF SharpDX path retains dependencies on the SharpDX package family. RobotStudio must isolate those types inside the desktop renderer so that a future renderer replacement remains practical.
- Imported PBR assets can require renderer-specific lighting and environment-map tuning. The proof of concept must verify dark, metallic, rough, transparent, and emissive materials instead of assuming visual parity with Blender.
- Model import, selection, transparency, clipping, and animation are individually supported, but their combined behavior and resource lifetime must be validated in one retained scene.
- The renderer is Windows-specific. This matches the current WPF product target but must remain outside portable projects.

## Asset Authoring Pipeline

The proposed authoring pipeline is:

```txt
Original RobotStudio model in Blender
                 |
                 v
Named component hierarchy and transform pivots
                 |
                 v
glTF 2.0 binary export (.glb)
                 |
                 v
Khronos glTF Validator
                 |
                 v
RobotStudio semantic manifest validation
                 |
                 v
Packaged offline application asset
```

Blender is an acceptable authoring tool because it is free and open source, and the Blender Foundation states that artwork created with it remains the creator's property. Blender is a development tool, not an application runtime dependency.

The Khronos glTF Validator is Apache-2.0 licensed and validates GLB structure, references, buffers, accessors, animations, images, and supported extensions. RobotStudio-specific validation remains necessary for semantic component identifiers, required nodes, pivots, supported materials, and demonstration bindings.

Models, textures, manifests, and demonstrations must be bundled for offline use. External downloads at application runtime are not part of the Milestone 5 design.

## Proof-Of-Concept Scope

The Cartesian Robot is the first vertical slice. The proof of concept must remain isolated from the existing schematic renderer and demonstrate:

1. A dedicated WPF mechanical-showcase view using the candidate renderer.
2. Loading one original GLB with a hierarchy containing at least base, X assembly, Y assembly, Z assembly, and tool nodes.
3. Retaining the scene and updating component transforms without rebuilding every mesh per frame.
4. Orbit, pan, zoom, reset, and fit-to-model camera behavior.
5. Hit testing that resolves a selected mesh to a RobotStudio semantic part identifier.
6. Selection highlighting and a small component-information surface.
7. One curated movement demonstration with play, pause, reset, and demonstration selection boundaries.
8. At least one internal teaching view using transparency, clipping, or a curated exploded pose.
9. Representative opaque, metallic, rough, rubber-like, transparent, and emissive material checks.
10. Clear failure output for a missing, invalid, or semantically incompatible asset.
11. Build, startup, resize, model-load, interaction, and disposal smoke validation.

The proof of concept is rejected or triggers a Stride comparison if it cannot provide acceptable material readability, stable WPF interaction, predictable resource disposal, or adequate performance without renderer-specific workarounds spreading through the desktop application.

The initial vertical slice now loads an original packaged Cartesian GLB into the retained showcase scene. The asset contains a semantic desktop-machine hierarchy, reusable primitive meshes, and representative metallic, rough, rubber-like, transparent, and emissive PBR material definitions. The desktop maps hit-tested descendants to `RobotPartId`, updates only semantic component-root transforms during curated demonstrations, highlights selected parts, falls back to the procedural scene on a deterministic asset error, and explicitly disposes imported scene resources. Four layers reuse the imported hierarchy: assembled, transparent drive-system inspection, motion-axis overlays, and a controlled exploded assembly. The first three expose coordinated and individual-axis tours. The exploded layer instead exposes a staged assembly sequence whose inverse offsets progressively return each semantic assembly to its authored pose. This preserves child inheritance, selection, and animation without altering the asset. The camera supports full-viewport orbit, camera-plane pan, bounded zoom, reset, and framing derived from the imported scene bounds. Tests import both a minimal binary fixture and the production package, verify complete semantic coverage and resource reuse, exercise invalid assets, and validate demonstrations, pose composition, view layers, and camera calculations. Broader material/lighting evaluation, resize and interaction smoke automation, and measured performance remain open, so the proof of concept is still in progress.

## Sources

- [HelixToolkit repository and license](https://github.com/helix-toolkit/helix-toolkit)
- [HelixToolkit 3.1.2 package compatibility](https://www.nuget.org/packages/HelixToolkit.Wpf.SharpDX/3.1.2)
- [HelixToolkit release history](https://github.com/helix-toolkit/helix-toolkit/releases)
- [Stride repository and license](https://github.com/stride3d/stride)
- [Stride 4.3 and .NET 10 announcement source](https://github.com/stride3d/stride-website/blob/master/posts/2025-11-14-announcing-stride-4-3-in-dotnet-10.md)
- [Blender licensing and ownership of artwork](https://www.blender.org/about/license/)
- [Khronos glTF Validator](https://github.com/KhronosGroup/glTF-Validator)
