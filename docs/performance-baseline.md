# Visualization Performance Baseline

Measured: 2026-08-27 08:08 -03:00

## Environment

- Hardware role: Development workstation; NVIDIA GeForce RTX 5060 Ti
- OS: Microsoft Windows 10.0.26200
- Runtime: .NET 10.0.11
- Architecture: X64
- CPU: AMD64 Family 25 Model 33 Stepping 2, AuthenticAMD
- Logical processors: 32
- Available managed memory: 31,9 GiB
- Viewport: 1280 x 800, 60 presentation frames, 9 x 7 hit-test grid
- One-time import pipeline warm-up: 107.05 ms

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

## Measurements

| Robot | Status | GLB KiB | Parts | Nodes | Manifest ms | Import ms | Scene prep ms | Frame avg / p95 ms | Transform avg / p95 ms | Hit test avg / p95 ms | Semantic hits | Teardown |
| --- | :---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | :---: |
| Cartesian Robot | PASS | 13.0 | 23 | 105 | 0.49 | 2.67 | 0.13 | 16.92 / 17.56 | 0.265 / 0.316 | 0.162 / 0.300 | 13/63 | PASS |
| XY Plotter | PASS | 11.1 | 15 | 77 | 0.37 | 2.44 | 0.03 | 16.57 / 17.12 | 0.148 / 0.181 | 0.018 / 0.033 | 11/63 | PASS |
| Differential Drive Robot | PASS | 13.1 | 13 | 87 | 0.33 | 3.10 | 0.04 | 16.65 / 17.04 | 0.092 / 0.113 | 0.019 / 0.074 | 5/63 | PASS |
| SCARA Robot | PASS | 12.0 | 13 | 81 | 0.27 | 2.08 | 0.03 | 16.67 / 17.01 | 0.074 / 0.094 | 0.019 / 0.053 | 5/63 | PASS |
| Simple Articulated Arm | PASS | 12.1 | 16 | 80 | 0.29 | 2.00 | 0.01 | 16.49 / 17.13 | 0.064 / 0.088 | 0.018 / 0.034 | 3/63 | PASS |
| Delta Robot | PASS | 17.0 | 19 | 151 | 0.30 | 1.99 | 0.02 | 16.90 / 17.12 | 0.086 / 0.099 | 0.003 / 0.009 | 9/63 | PASS |
| Drone | PASS | 19.1 | 19 | 173 | 0.27 | 1.98 | 0.02 | 16.68 / 17.70 | 0.076 / 0.106 | 0.003 / 0.008 | 2/63 | PASS |
| 6-DOF Industrial Arm | PASS | 13.7 | 21 | 107 | 0.27 | 1.78 | 0.01 | 16.96 / 20.02 | 0.226 / 0.129 | 0.003 / 0.010 | 3/63 | PASS |

## Result

**PASS**

## Interpretation

- The one-time warm-up records native Assimp and runtime initialization separately. Per-model timings use a fresh loader after that warm-up so they remain comparable.
- GLB import includes Assimp parsing and semantic node binding.
- Scene preparation forces transform and bounds propagation before the scene is attached to the measured viewport.
- Frame cadence is observed from WPF composition while each frame samples a demonstration and applies procedural and imported transforms.
- Hit-test timing uses HelixToolkit `FindHits` across the live viewport and counts points that resolve to a RobotStudio semantic part.
- Exit code `0` means every measurement passed; a nonzero exit code means at least one performance budget was exceeded or the diagnostic could not complete.
- Passing on the development workstation does not qualify the intended teaching hardware. Repeat this command there before release.

Run again with:

```powershell
dotnet run --project tools/RobotStudio.VisualizationDiagnostics --configuration Release -- docs/performance-baseline.md "Development workstation; NVIDIA GeForce RTX 5060 Ti"
```
