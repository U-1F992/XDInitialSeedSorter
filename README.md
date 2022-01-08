# XDInitialSeedSorter

The tool that automates the initial seed selection for 「ポケモンXD 闇の旋風ダーク・ルギア」

## Requirements

- [WHALE](https://github.com/mizuyoukanao/WHALE)
- [Tesseract OCR](https://github.com/tesseract-ocr/) and traineddata `xdn`, `xdj`
- [XDDatabase](https://github.com/yatsuna827/XDDatabase)
  - some tweaks would be needed
  
```csharp
    static class Program
    {
        static void Main(string[] args)
        {
            // MakeDB();
            // Load().CreateDB();
            // while (true)
            // {
                Directory.SetCurrentDirectory(AppContext.BaseDirectory);
                LoadDB().SearchSeed();
            // }
```

## How to use

Edit the contents of `setting.json` as necessary.

```javascript
{
    "pokemonXD": {
        // 目標seedを10進数で列挙します。
        "targets": [
            1364174178,
            235987496,
            3089508904
        ],
        // 強制消費を考慮して余分に残す消費数を指定します。
        "leftover": 500000,
        // いますぐバトルでファイヤーが場に出ている場合の3713.6消費/s
        "consumptionPerSec": 3713.6
    },
    "binaries": {
        // パスが通っている場合は編集の必要はありません。
        "tesseractOCR": "tesseract",
        "xdDatabase": "XDDatabase"
    },
    "devices": {
        // コントローラーに使用するArduinoのポートを指定します。
        "controller": {
            "port": "COM6"
        },
        // カメラ番号と解像度を指定します。解像度を変更した場合は切り取り範囲の較正が必要になるでしょう。
        "capture": {
            "index": 0,
            "width": 1600,
            "height": 1200
        }
    },
    // 各要素の切り取り範囲を設定します。
    "crops": {
        "player": {
            "width": 215,
            "height": 53,
            "x": 324,
            "y": 650
        },
        "com": {
            "width": 215,
            "height": 53,
            "x": 999,
            "y": 650
        },
        "hp_1": {
            "width": 130,
            "height": 50,
            "x": 409,
            "y": 760
        },
        "hp_2": {
            "width": 130,
            "height": 50,
            "x": 409,
            "y": 993
        },
        "hp_3": {
            "width": 130,
            "height": 50,
            "x": 1088,
            "y": 760
        },
        "hp_4": {
            "width": 130,
            "height": 50,
            "x": 1088,
            "y": 993
        }
    }
}
```

## Build instruction

1. You need to add [PokemonPRNG](https://github.com/yatsuna827/PokemonPRNG) NuGet package to your local NuGet source.
2. Build like below.

```powershell
dotnet build
```

3. Or you can use `publish.bat` to get `XDInitialSeedSorter.zip`

