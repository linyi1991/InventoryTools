#!/usr/bin/env python3
"""Read-only coverage and persistence-contract audit for the API13 TW port."""
import json
import re
import subprocess
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
SRC = ROOT / 'InventoryTools'
BASELINE = 'eee10d3c2ab2e7eeba32d49f183db1be7b4dedcf'
CJK = re.compile(r'[\u3400-\u9fff]')
LITERAL = r'"((?:[^"\\]|\\.)*)"'
PROP = re.compile(r'\b(Name|HelpText|SingularName|PluralName)\s*(?:=>|\{[^}]*\}\s*=)\s*' + LITERAL)
settings_map = dict(re.findall(r'\["([^"]+)"\] = "([^"]+)"', (SRC / 'Services/TwSettingsLocalization.cs').read_text()))
ui_map = dict(re.findall(r'\["([^"]+)"\] = "([^"]+)"', (SRC / 'Services/TwUiLocalization.cs').read_text()))
errors = []
counts = {}
for folder in ['Logic/Settings', 'Logic/Filters', 'Logic/Columns/ColumnSettings', 'Logic/ItemRenderers']:
    count = 0
    for path in (SRC / folder).rglob('*.cs'):
        for m in PROP.finditer(path.read_text()):
            value = m[2]
            if value in {"HQ？", "NPC：", "NPC ID："} or not value or CJK.search(value) or not re.search('[A-Za-z]', value):
                continue
            count += 1
            mapped = settings_map.get(value, value) if folder == 'Logic/Settings' else value
            if not CJK.search(mapped):
                errors.append(f'Untranslated {path.relative_to(ROOT)} {m[1]}: {value}')
    counts[folder + '_mapped_english_constants'] = count

column_names = re.compile(r'\b(?:Name|RenderName)\s*(?:=>|\{[^}]*\}\s*=)\s*' + LITERAL)
for path in (SRC / 'Logic/Columns').rglob('*.cs'):
    for m in column_names.finditer(path.read_text()):
        value = m[1]
        if re.search('[A-Za-z]', value) and not CJK.search(value) and not CJK.search(ui_map.get(value, value)):
            errors.append(f'Unmapped column name: {value}')

# Persistent keys, defaults, commands, and IPC contracts must survive localization unchanged.
protected_properties = re.compile(r'\b(?:Key|GenericKey|DefaultValue)\s*(?:=>|\{[^}]*\}\s*=)\s*([^;]+);')
changed = subprocess.check_output(['git', '-C', str(ROOT), 'diff', '--name-only', BASELINE], text=True).splitlines()
for rel in changed:
    path = ROOT / rel
    if not path.is_file() or path.suffix != '.cs':
        continue
    before = subprocess.check_output(['git', '-C', str(ROOT), 'show', f'{BASELINE}:{rel}'], text=True)
    after = path.read_text()
    if protected_properties.findall(before) != protected_properties.findall(after):
        errors.append(f'Persistent key/default changed: {rel}')
    if '/IPC/' in rel or '/Ipc/' in rel or '/CraftAvailability/' in rel or rel.endswith('InventoryToolsConfiguration.cs'):
        errors.append(f'Protected integration/configuration source changed: {rel}')

# Boolean display localization must not translate converter input or serialized column values.
bool_filter = (SRC / 'Logic/Filters/Abstract/BooleanFilter.cs').read_text()
for token in ['selection == "N/A"', 'selection == "Yes"', 'new []{"N/A", "Yes", "No"}', 'ConvertSelection(item)']:
    if token not in bool_filter:
        errors.append(f'Boolean selection contract missing: {token}')

# Static direct display-call audit. Skip developer diagnostics and allow technical labels only.
call = re.compile(r'ImGui\.(?:Text|TextUnformatted|TextWrapped|BulletText|Button|SmallButton|Checkbox|BeginTabItem|BeginMenu|MenuItem|TableSetupColumn|Selectable|CollapsingHeader|SetTooltip|InputText|InputInt)\(\s*' + LITERAL)
allow = {'Ko-Fi', 'GT##GT', 'R##', ' x ', 'X##RM', 'X##Column', 'DraggerBtn', 'ID', 'NPC：', 'NPC ID：'}
for path in SRC.rglob('*.cs'):
    if any(part in {'obj', 'bin', 'Debug'} for part in path.parts):
        continue
    for m in call.finditer(path.read_text(encoding='utf-8-sig')):
        value = m[1]
        if re.search('[A-Za-z]', value) and not CJK.search(value) and not value.startswith('##') and value not in allow:
            errors.append(f'Untranslated direct display: {path.relative_to(ROOT)}: {value}')
report = {'baseline': BASELINE, 'changed_files': len(changed), 'coverage': counts, 'unmatched': errors,
          'limits': 'Static source coverage only; interpolated runtime data, external libraries and game UI require in-game acceptance.'}
print(json.dumps(report, ensure_ascii=False, indent=2))
raise SystemExit(bool(errors))
