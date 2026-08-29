# Visualization Performance Baseline: Teaching Hardware

Measured: 2026-08-29

## Environment

- Hardware role: Teaching hardware; ASUS VivoBook X515JA
- CPU: Intel Core i3-1005G1, 2 cores / 4 threads
- GPU: Intel UHD Graphics
- Physical memory: 4 GB RAM
- Display: 1366 x 768
- Runtime: .NET 10.0.11
- Diagnostic viewport: 1280 x 800, 60 presentation frames, 9 x 7 hit-test grid
- One-time import pipeline warm-up: 1548.48 ms

## Acceptance Budget

The minimum profile targets responsive use on teaching hardware. Every robot must remain within every limit.

| Measurement | Maximum |
| --- | ---: |
| One-time pipeline warm-up | 2000.00 ms |
| Manifest load | 100.00 ms |
| Warmed GLB import | 500.00 ms |
| Scene preparation | 50.00 ms |
| Frame interval p95 | 35.00 ms (30 FPS target + 5% scheduler tolerance) |
| Transform update p95 | 8.00 ms |
| Semantic hit test p95 | 8.00 ms |

## Result

**PASS**

All eight packaged GLB showcases passed the Release diagnostic. Transform updates, semantic hit tests, and renderer teardown passed for every model.

## Worst Measurements

| Measurement | Observed | Limit | Model | Status |
| --- | ---: | ---: | --- | :---: |
| One-time pipeline warm-up | 1548.48 ms | 2000.00 ms | Import pipeline | PASS |
| Frame interval p95 | 30.96 ms | 35.00 ms | Differential Drive Robot | PASS |
| Warmed GLB import | 49.98 ms | 500.00 ms | 6-DOF Industrial Arm | PASS |

## Interpretation

- The baseline validates the current eight mechanical showcases on representative lower-spec teaching hardware with integrated Intel graphics and 4 GB RAM.
- The frame-cadence result remains within the 30 FPS teaching profile, including the configured scheduler tolerance.
- Warmed model loading has substantial margin beneath the current import budget.
- No packaged asset requires simplification, and no rendering-budget change is required for this hardware profile.
- The temporary `MSBuildEnableWorkloadResolver=false` environment setting used during validation worked around incomplete local .NET 10 workload manifests. It does not require a repository change.

Run again with:

```powershell
dotnet run --project tools/RobotStudio.VisualizationDiagnostics --configuration Release -- docs/performance-baseline-lower-spec.md "Teaching hardware; ASUS VivoBook X515JA, Intel Core i3-1005G1, Intel UHD Graphics, 4 GB RAM"
```
