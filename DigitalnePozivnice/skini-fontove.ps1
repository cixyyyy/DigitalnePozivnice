﻿# Skida fontove (SIL OFL) u wwwroot\fonts, da se sajt ne oslanja na Google Fonts.
# Pokreni iz foldera projekta (tamo gdje je DigitalnePozivnice.csproj):  powershell -ExecutionPolicy Bypass -File .\skini-fontove.ps1
$ErrorActionPreference = "Continue"
$dir = Join-Path $PSScriptRoot "wwwroot\fonts"
New-Item -ItemType Directory -Force -Path $dir | Out-Null
$base = "https://cdn.jsdelivr.net/npm/@fontsource"
$files = @(
  "cormorant-garamond|cormorant-garamond-latin-ext-400-normal.woff2",
  "cormorant-garamond|cormorant-garamond-latin-400-normal.woff2",
  "cormorant-garamond|cormorant-garamond-latin-ext-500-normal.woff2",
  "cormorant-garamond|cormorant-garamond-latin-500-normal.woff2",
  "cormorant-garamond|cormorant-garamond-latin-ext-600-normal.woff2",
  "cormorant-garamond|cormorant-garamond-latin-600-normal.woff2",
  "cormorant-garamond|cormorant-garamond-latin-ext-400-italic.woff2",
  "cormorant-garamond|cormorant-garamond-latin-400-italic.woff2",
  "cormorant-garamond|cormorant-garamond-latin-ext-500-italic.woff2",
  "cormorant-garamond|cormorant-garamond-latin-500-italic.woff2",
  "cormorant-garamond|cormorant-garamond-latin-ext-600-italic.woff2",
  "cormorant-garamond|cormorant-garamond-latin-600-italic.woff2",
  "great-vibes|great-vibes-latin-ext-400-normal.woff2",
  "great-vibes|great-vibes-latin-400-normal.woff2",
  "pinyon-script|pinyon-script-latin-ext-400-normal.woff2",
  "pinyon-script|pinyon-script-latin-400-normal.woff2",
  "jost|jost-latin-ext-300-normal.woff2",
  "jost|jost-latin-300-normal.woff2",
  "jost|jost-latin-ext-400-normal.woff2",
  "jost|jost-latin-400-normal.woff2",
  "jost|jost-latin-ext-500-normal.woff2",
  "jost|jost-latin-500-normal.woff2",
  "lora|lora-latin-ext-400-normal.woff2",
  "lora|lora-latin-400-normal.woff2",
  "lora|lora-latin-ext-500-normal.woff2",
  "lora|lora-latin-500-normal.woff2",
  "playfair-display|playfair-display-latin-ext-400-normal.woff2",
  "playfair-display|playfair-display-latin-400-normal.woff2",
  "playfair-display|playfair-display-latin-ext-500-normal.woff2",
  "playfair-display|playfair-display-latin-500-normal.woff2"
)
$ok = 0; $fail = @()
foreach ($f in $files) {
  $p = $f.Split("|")
  $url = "$base/$($p[0])/files/$($p[1])"
  $dest = Join-Path $dir $p[1]
  try { Invoke-WebRequest -Uri $url -OutFile $dest -UseBasicParsing; $ok++ } catch { $fail += $p[1] }
}
Write-Host "Preuzeto: $ok od $($files.Count)"
if ($fail.Count -gt 0) { Write-Host "Nije uspjelo:"; $fail | ForEach-Object { Write-Host "  $_" } } else { Write-Host "Sve OK. Fontovi su sada na vasem serveru." }
