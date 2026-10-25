# LearnOnMe

Windowsowy hub do nauki przedmiotów na studiach: kafelki przedmiotów, karta przedmiotu z przypinkami
(foldery, pliki, aplikacje, linki) i zakładka „Nauka” z własnymi quizami HTML w WebView2.

## Stos

- C# / .NET 10, **WPF**
- SQLite (`Microsoft.Data.Sqlite`), baza w `%LOCALAPPDATA%\LearnOnMe\learnonme.db`
- WebView2 (w kolejnym etapie)

## Struktura

| Projekt | Rola |
|---|---|
| `src/LearnOnMe.Core` | Modele (`Subject`, `Pin`, `PinType`), interfejsy repozytoriów, `PinTargets` (normalizacja i otwieranie przypinek) |
| `src/LearnOnMe.Data` | SQLite: migracje (`PRAGMA user_version`), seed przedmiotów, repozytoria |
| `src/LearnOnMe.App` | Aplikacja WPF: ciemny motyw, ekran kafelków, karta przedmiotu z przypinkami |
| `tests/LearnOnMe.Core.Tests` | Testy logiki przypinek (xUnit) |
| `tests/LearnOnMe.Data.Tests` | Testy warstwy danych (xUnit) |

## Uruchomienie

```
dotnet run --project src/LearnOnMe.App
dotnet test
```

Albo otwórz `LearnOnMe.sln` w Visual Studio 2026 i uruchom `LearnOnMe.App`.

Kolory motywu są w `src/LearnOnMe.App/Themes/Dark.xaml`. Przedmioty startowe są w
`src/LearnOnMe.Data/SeedData.cs` i trafiają do bazy tylko przy jej pierwszym utworzeniu.

## Przypinki

Na karcie przedmiotu: **Dodaj przypinkę** (Ctrl+N), kliknięcie otwiera, prawy przycisk albo ikony na
najechaniu dają **Edytuj** i **Usuń**. Rodzaje:

- **Folder** i **Plik** otwierają się tak jak dwuklik w Eksploratorze (plik w domyślnym programie).
- **Aplikacja**: plik `.exe` albo skrót `.lnk` z menu Start (SSMS, Packet Tracer, PAC Machine Edition, Visual Studio).
  Skróty są zostawiane jako skróty, bo niosą argumenty i folder roboczy.
- **Link**: dowolny adres, w tym Google Drive, Teams i OneDrive w przeglądarce. Brakujące `https://` jest dodawane.

Pliki i foldery można też przeciągnąć z Eksploratora na kartę, a link z paska adresu przeglądarki.
Przypinka, której ścieżki nie ma na dysku, ma żółtą ikonę ostrzeżenia.

## Etapy

1. ✅ Szkielet: solucja, SQLite, kafelki przedmiotów
2. ✅ Karta przedmiotu: przypinki (CRUD) i otwieranie przez `Process.Start`
3. Zakładka „Nauka” z WebView2
