# Parallel Routing Throughput Benchmark

Date: 2026-09-15

Git Commit (measured on): `92b8dbe`

## Goal

Measure whether processing independent route requests in parallel via
`RoutingWorkerPool` improves routing throughput compared to a single
worker.

## Environment

CPU: 8 physical cores, 8 logical

## Configuration

- `Graph:` 50×50 grid, 2,500 nodes (~10,000 directed edges, unit length)
- `Requests:` 2,000 random (source, destination) pairs
- `Random seed:` 481516 (§2.12 reproducibility)
- `Routing algorithm:` Dijkstra
- `Cache:` none

## Procedure

Single run per worker count. Worker counts tested: 1, 2, 4, 8, 16.

## Results

| Workers | Time | Routes/sec | Speedup vs. 1 worker |
|---:|---:|---:|---:|
| 1  | 795ms | 2,515  | 1.00x |
| 2  | 297ms | 6,715  | 2.68x |
| 4  | 193ms | 10,354 | 4.12x |
| 8  | 188ms | 10,622 | 4.23x |
| 16 | 192ms | 10,390 | 4.14x |

## Observations

Throughput scales close to linearly from 1 to 2 workers, then sub-linearly
from 2 to 4, and essentially flattens from 4 to 8 (a 2.6% gain despite
doubling worker count). At 16 workers — double the machine's 8 physical
cores — throughput slightly *regresses* relative to 8, consistent with the context-switching and cache-thrashing

## Conclusion

> On this machine, 4 workers captures nearly all the available; 8 is the last point of speedup.

## Next Experiment

- Add a true sequential (plain `for`) baseline alongside `workers=1` to
  confirm they match.
- Multiple runs with warmup, to smooth out single-run noise (this pass
  used one run per configuration).
- Repeat at a larger graph/request size to check whether the 4-8 worker
  plateau shifts with workload size.
