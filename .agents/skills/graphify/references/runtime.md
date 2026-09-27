# Graphify PowerShell runtime

All executable snippets in this skill are PowerShell command bodies. Run them through `pwsh -NoProfile -Command "<command>"` (or `powershell` fallback). For multiline bodies, use proper PowerShell quoting or invoke a temporary `.ps1` through `pwsh -NoProfile -Command "& '<script-path>'"`; never interpolate untrusted values into shell/Python source. Single-quoted here-strings pass Python code through stdin, preserving quotes, dollar signs, and backticks. Pass user paths/questions as process arguments or environment values rather than substituting raw text into literals.

## Resolve an existing interpreter

Prefer the saved interpreter if it exists and imports Graphify. Otherwise inspect the installed uv environment, then the system Python. Do not read an `.exe` launcher as a shebang. This resolution is read-only and requires no provider key.

```powershell
$OutputEncoding = [System.Text.UTF8Encoding]::new($false)
$graphifyPython = $null
$graphifyCandidates = @()
if (Test-Path -LiteralPath 'graphify-out/.graphify_python') {
    $graphifyCandidates += (Get-Content -LiteralPath 'graphify-out/.graphify_python' -Raw).Trim()
}
if (Get-Command uv -ErrorAction SilentlyContinue) {
    $graphifyToolDirectory = uv tool dir
    if ($LASTEXITCODE -eq 0) {
        $graphifyCandidates += Join-Path $graphifyToolDirectory.Trim() 'graphifyy/Scripts/python.exe'
    }
}
if (Get-Command python -ErrorAction SilentlyContinue) {
    $graphifyCandidates += (Get-Command python).Source
}
foreach ($graphifyCandidate in $graphifyCandidates) {
    if (Test-Path -LiteralPath $graphifyCandidate) {
        & $graphifyCandidate -c 'import graphify' 2>$null
        if ($LASTEXITCODE -eq 0) {
            $graphifyPython = $graphifyCandidate
            break
        }
    }
}
if (-not $graphifyPython) {
    throw 'No installed Graphify interpreter found. Report the unavailable runtime; do not fabricate results.'
}
```

If Graphify installation is part of the authorized task, use the existing tool manager (`uv tool install graphifyy`, or the selected Python's `-m pip install graphifyy`) and resolve again. Do not upgrade dependencies or modify personal configuration during a read-only query. Import/probing failure is an unavailable check, not a passed check.

## Build/update initialization only

For authorized graph mutation, initialize output and save the resolved interpreter and scan root. Set `$graphifyInputPath` to the requested path (default `.`). Omit these writes during read-only queries/reviews and Plan Mode.

```powershell
$graphifyInputPath = '.'
$graphifyScanRoot = (Resolve-Path -LiteralPath $graphifyInputPath).Path
$env:GRAPHIFY_INPUT_PATH = $graphifyScanRoot
# Use the actual repository root if invoked elsewhere.
$env:GRAPHIFY_SPEC_PATH = (Resolve-Path -LiteralPath '.agents/skills/graphify/references/extraction-spec.md').Path
New-Item -ItemType Directory -Path 'graphify-out' -Force | Out-Null
[System.IO.File]::WriteAllText((Join-Path $PWD 'graphify-out/.graphify_python'), $graphifyPython, [System.Text.UTF8Encoding]::new($false))
[System.IO.File]::WriteAllText((Join-Path $PWD 'graphify-out/.graphify_root'), $graphifyScanRoot, [System.Text.UTF8Encoding]::new($false))
```

Use the selected interpreter for Python/module snippets. If the `graphify` launcher is broken but its interpreter imports the package, use `& $graphifyPython -m graphify` with the same subcommand arguments. Confirm availability before claiming runtime validation.

Reference Python templates read input/specification paths from the process environment initialized above. Before execution, set `IS_DIRECTED` to `True`/`False` and supply the requested query/mode/budget safely through arguments or environment data. Never execute an unresolved template. The explicit UTF-8 writes also work with the PowerShell 5.1 fallback.
