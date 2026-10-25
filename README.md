# LearnOnMe

Windowsowy hub do nauki przedmiotów na studiach: kafelki przedmiotów, karta przedmiotu z przypinkami
(foldery, pliki, aplikacje, linki) i zakładka „Nauka” z własnymi quizami HTML w WebView2.

## Stos

- C# / .NET 10, **WPF**
- SQLite (`Microsoft.Data.Sqlite`), baza w `%LOCALAPPDATA%\LearnOnMe\learnonme.db`
- WebView2 (`Microsoft.Web.WebView2`): zakładka „Nauka” i wbudowana przeglądarka; profil w `%LOCALAPPDATA%\LearnOnMe\WebView2`

## Struktura

| Projekt | Rola |
|---|---|
| `src/LearnOnMe.Core` | Modele (`Subject`, `Pin`, `PinType`), interfejsy repozytoriów, `PinTargets` (normalizacja i otwieranie przypinek), `Study/` (szukanie plików HTML, podgląd folderu) |
| `src/LearnOnMe.Data` | SQLite: migracje (`PRAGMA user_version`), seed przedmiotów, repozytoria |
| `src/LearnOnMe.App` | Aplikacja WPF: ciemny motyw, ekran kafelków, karta przedmiotu z zakładkami Przypinki, Nauka i Przeglądarka |
| `tests/LearnOnMe.Core.Tests` | Testy logiki przypinek, plików do nauki i podglądu folderu (xUnit) |
| `samples/` | Przykładowy quiz HTML do sprawdzenia zakładki „Nauka” |
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

- **Folder** otwiera się w podglądzie na karcie: lista plików i podfolderów, dwuklik (albo Enter) otwiera plik
  w domyślnym programie, Backspace wraca folder wyżej, a przycisk **Otwórz w Eksploratorze** otwiera go w Windows.
- **Plik** otwiera się tak jak dwuklik w Eksploratorze. Plik `.html` otwiera się w zakładce **Nauka**.
- **Aplikacja**: plik `.exe` albo skrót `.lnk` z menu Start (SSMS, Packet Tracer, PAC Machine Edition, Visual Studio).
  Skróty są zostawiane jako skróty, bo niosą argumenty i folder roboczy.
- **Link**: dowolny adres, w tym Google Drive, Teams i OneDrive. Strony `http(s)` otwierają się w zakładce
  **Przeglądarka** w aplikacji, `mailto:` i inne w Windows. Brakujące `https://` jest dodawane.

Pliki i foldery można też przeciągnąć z Eksploratora na kartę, a link z paska adresu przeglądarki.
Przypinka, której ścieżki nie ma na dysku, ma żółtą ikonę ostrzeżenia.

Prawy przycisk na przypince daje też **Otwórz w Eksploratorze** albo **Otwórz w przeglądarce systemowej**.

## Zakładki karty przedmiotu

Ctrl+1, Ctrl+2 i Ctrl+3 przełączają zakładki.

- **Przypinki**: to, co wyżej.
- **Nauka**: wszystkie pliki `.html` i `.htm` z przypiętych folderów (do 4 poziomów w głąb, bez `node_modules`,
  `bin`, `obj` i ukrytych folderów) oraz przypięte pliki HTML. Każdy przypięty folder jest serwowany pod własnym adresem
  `https://f<hash>.nauka.learnonme/`, więc quiz może wczytać skrypty, obrazki i JSON obok siebie, a to, co zapisze
  w `localStorage`, zostaje między uruchomieniami. Linki z quizu do stron w sieci otwierają się w zakładce Przeglądarka.
  Do sprawdzenia: `samples/quiz-przyklad.html`.
- **Przeglądarka**: wstecz, dalej, odśwież, pasek adresu, **Przypnij stronę** i otwieranie w przeglądarce systemowej.
  Logowania (Microsoft, Moodle) są zapamiętywane. Google bywa niechętne logowaniu w osadzonych przeglądarkach; jeśli
  odmówi, użyj przycisku otwierania w przeglądarce systemowej.

WebView2 Runtime jest standardowo w Windows 10 i 11. Jeśli go brakuje, aplikacja pokazuje przycisk do pobrania.

## Etapy

1. ✅ Szkielet: solucja, SQLite, kafelki przedmiotów
2. ✅ Karta przedmiotu: przypinki (CRUD) i otwieranie przez `Process.Start`
3. ✅ Zakładka „Nauka” z WebView2, wbudowana przeglądarka linków i podgląd przypiętych folderów
