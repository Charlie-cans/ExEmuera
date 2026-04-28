---
name: uemuera-check
description: Check compilation of Unity uEmuera EM+EE project. Use after editing any .cs file in D:/code/era/uEmuera to verify it compiles. Reports errors from Unity Editor.log. Triggers when user mentions compiling, checking compilation, checking errors, building, or testing after code changes in this project, or when they say /check or /compile.
---

# Unity uEmuera Compile Check

Checks whether the Unity uEmuera project at `D:/code/era/uEmuera` compiles successfully after code changes.

## When code is edited

After editing any `.cs` file under `D:/code/era/uEmuera/Assets/`:

1. Touch the modified file to ensure Unity detects the change:
```bash
touch "D:/code/era/uEmuera/Assets/Scripts/<path-to-file>.cs"
```

2. Wait 8-15 seconds for Unity to auto-recompile (Unity detects file changes automatically):

3. Read the Editor.log and extract ONLY the most recent compilation errors:
```bash
grep "error CS" "/c/Users/charlie/AppData/Local/Unity/Editor/Editor.log" | tail -10
```

4. If no errors found, verify compilation succeeded by checking for the compile timestamp:
```bash
grep "Finished compile Library/ScriptAssemblies/Assembly-CSharp.dll" "/c/Users/charlie/AppData/Local/Unity/Editor/Editor.log" | tail -1
```

## IMPORTANT: Distinguish old from new errors

Editor.log accumulates logs. Old errors from previous compilations remain in the log. Always check the MOST RECENT entries:

- Look for the line `- Starting compile Library/ScriptAssemblies/Assembly-CSharp.dll` — errors AFTER this line are from the current compilation
- If you only see the same error count repeating (exactly the same errors), the file may not have been recompiled yet — wait longer or touch the file again

## When Unity Editor is NOT running

If the Unity Editor is closed, use batchmode for a full compilation check:

```bash
"/c/Program Files/Unity/Hub/Editor/2020.3.49f1/Editor/Unity.exe" -batchmode -quit -nographics -projectPath "D:/code/era/uEmuera" -logFile "D:/code/era/unity_build.log" 2>&1 && echo "BUILD OK" || grep "error CS" "D:/code/era/unity_build.log"
```

The batchmode version is more reliable — it gives a definitive pass/fail.

## Quick brace balance check

Before waiting for Unity, quickly verify no obvious brace mismatches in all modified files:

```bash
/c/Users/charlie/AppData/Local/Programs/Python/Python312/python.exe -c "
with open('<file-path>', 'r', encoding='utf-8') as f:
    c = f.read()
bal = c.count('{') - c.count('}')
if bal != 0:
    print(f'BRACE IMBALANCE: {bal}')
"
```

## Project context

- Project: `D:/code/era/uEmuera` (Unity 2020.3.34f1, tested with 2020.3.49f1)
- EM+EE source reference: `D:/code/era/emuera.em.gitlab/`
- Game data: `D:/butter/era/eratw-sub-modding-main/`
- TODO list: `D:/code/era/uEmuera/TODO.md`
- CLAUDE.md: `D:/code/era/uEmuera/CLAUDE.md`
- Memory: `C:/Users/charlie/.claude/projects/D--code-era/memory/emuera_em_ee_port.md`
