# Visual Asset Manifest

## Purpose

RobotStudio packages realistic robot geometry as glTF 2.0 binary assets (`.glb`). A separate JSON manifest connects renderer-owned node names to stable RobotStudio semantic parts without making meshes or node names part of the domain model.

The manifest is an integration boundary. It does not define robot behavior, simulation state, materials, camera settings, or demonstrations.

## Version 1

A version 1 package contains a manifest and its referenced GLB in the same package directory or a child directory:

```txt
Assets/
  Robots/
    Cartesian/
      robot.json
      robot.glb
```

The minimal manifest shape is:

```json
{
  "schemaVersion": 1,
  "modelId": "cartesian",
  "assetFile": "robot.glb",
  "nodes": [
    { "nodeName": "Machine_Base", "partId": "base" },
    { "nodeName": "Tool_Body", "partId": "tool" },
    { "nodeName": "Tool_Nozzle", "partId": "tool" }
  ]
}
```

- `schemaVersion` selects the manifest contract and must currently be `1`.
- `modelId` must match the renderer-neutral `RobotVisualModelDefinition.Id`.
- `assetFile` is a forward-slash relative path to a packaged `.glb` file.
- `nodes` maps each uniquely named asset node to a stable `RobotPartId`.

Several nodes may map to one semantic part because a meaningful component can contain multiple meshes. Each selectable part in the visual model must have at least one node mapping. Non-selectable decorative parts do not require mappings.

## Validation

`RobotStudio.Visualization` parses and validates the portable contract. Validation rejects:

- invalid JSON or incomplete manifest values;
- unsupported schema versions or mismatched model identifiers;
- absolute paths, parent traversal, backslashes, empty path segments, or non-GLB assets;
- duplicate asset node names;
- mappings to unknown semantic parts;
- selectable semantic parts with no mapped node.

`RobotStudio.Desktop` resolves files relative to the manifest, verifies that the resolved asset remains inside the package directory, reports missing files with stable error codes, and caches validated package metadata.

## Deferred Concerns

Version 1 intentionally does not encode materials, textures, pivots, transforms, animations, demonstrations, cameras, lighting, or renderer-specific configuration. These belong in the GLB, existing visualization contracts, or future schema versions only when a concrete asset proves that additional metadata is necessary.

The current package loader does not import GLB scene nodes or allocate GPU resources. That renderer integration, node-existence verification, and mesh/material resource caching remain part of the Cartesian proof of concept.
