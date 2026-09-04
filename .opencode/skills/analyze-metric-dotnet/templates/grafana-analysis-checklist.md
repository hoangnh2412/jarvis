# Checklist: Phân tích Grafana Dotnet Runtime Metrics

Copy vào ghi chú incident / ticket.

## Context

- [ ] Dashboard: Dotnet Runtime Metrics
- [ ] `job`: _______________
- [ ] `instance`: _______________
- [ ] Time range: _______________
- [ ] Deploy / incident time: _______________

## Panel review

- [ ] Exceptions / assemblies — spike? deploy-related?
- [ ] GC collections — Gen2 elevated?
- [ ] Memory committed — monotonic growth?
- [ ] GC pause — correlates with latency?
- [ ] Thread pool queue — sustained > 0?
- [ ] CPU process (.NET 9+ only?) — high sustained?
- [ ] JIT — post-deploy spike only?

## Correlation

- [ ] HTTP / custom metrics same window
- [ ] Logs / traces for exception spike
- [ ] Compare instances (one pod vs all)

## Conclusion template

**Observation:**

**Interpretation:**

**Next steps:**
