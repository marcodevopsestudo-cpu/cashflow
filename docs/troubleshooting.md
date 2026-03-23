# Troubleshooting Guide

This document captures common operational issues and recommended resolution steps.

The goal is to provide quick diagnostics and recovery guidance for known failure scenarios.

---

## 1. Azure Functions Deployment Failure (Sync Trigger / Malformed Package)

### Symptoms

- GitHub Actions workflow reaches `Azure/functions-action@v1`;
- package upload succeeds;
- deployment fails during **"Sync Trigger Function App"** step.

---

### Likely Cause

The deployed package does not match the expected structure for Azure Functions (isolated worker model).

This typically happens when:

- the project is built but not properly published;
- the wrong folder is deployed (e.g., project root instead of publish output);
- required runtime files are missing.

---

### Resolution Steps

1. **Ensure proper publish step**

Use `dotnet publish`, not only `dotnet build`:

```bash
dotnet publish -c Release -o ./publish /p:UseAppHost=false
```
