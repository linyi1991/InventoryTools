# TW localization checks

Compile the actual API13/net9 plugin separately. This small executable links only its real display-localization sources and has no game dependencies.

```sh
dotnet build tests/TwLocalization/TwLocalization.csproj -c Release
dotnet --roll-forward Major tests/TwLocalization/bin/Release/net9.0/TwLocalization.dll
python3 tests/audit_tw_localization.py
```

The explicit runtime roll-forward is **only for this pure-string test harness** on the ARM Mac build host, which has .NET 10 but not ARM .NET 9 installed. It does not alter the plugin's API13/net9 target, game runtime, packages, or machine configuration. These checks do not replace in-game acceptance.
