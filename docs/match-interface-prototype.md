# Match interface prototype

The six new WPF views reuse the existing mint palette and pixel font. Every new
player-facing label is stored in the Spanish and English `UiStrings` resources.
Each view uses the same settings gear button as the main menu. Spanish and English
are selected in the settings dialog, updating open windows immediately through
the existing `LocalizationViewModel`.

## Navigation

| Starting view | Action | Destination |
| --- | --- | --- |
| Main menu | Play | New game |
| Main menu | Scores | Scores and results |
| New game | Create | Game setup |
| New game | Join | Invitation code |
| Game setup | Accept | Waiting room |
| Invitation code | Enter room | Waiting room, regardless of the input |
| Game setup / Waiting room | Start | Sample scores and results |
| Scores and results | Next round | Waiting room |
| Scores and results | Exit to menu | Main menu |
| New game | Back | Main menu |
| Setup / Invitation / Waiting room / Results | Back | New game |
| Any new view or main menu | Game rules | Rules dialog |
| Game rules | Back or window close | The view that opened it |
| Any new view or main menu | Settings gear | Settings dialog |
| Settings | Back or window close | The view that opened it |

Start goes directly to the results preview because gameplay is outside this UI
task. It does not run or simulate a match. Next round does not modify scores.
Accept navigates to a separate ready-state preview; it does not update players.
The maximum-double options only change their visual selection on the current
screen. No match settings or invitation input are stored or validated.

## Reference and sample content

`TrenDomino.pdf`, supplied by the user, contains eight pages. Its setup prototypes
on pages 1 and 3 provide the room code, doubles 6 / 9 / 12, player list, readiness
and Start action. Page 5 provides the score table, winner, Exit and Next round.
The new-game, invitation and rules layouts extend the existing application's
visual style because this PDF has no separate prototypes for those screens.

The room code (`XXXXXX`), three occupied seats and player labels are fixed examples.
The results reproduce the prototype's two rounds (47/8, 12/9, 0/0 and 5/10) with
fixed totals. The room and results are independent sample screens, not live data.

The rules view contains a concise Mexican Train guide, checked against
[Mexican Train rules](https://mexicantrain.com/rules.html). The project's own
rule variants can be substituted in the resources without changing the UI.

There are no database connections, network services, input validation, score
calculations or game-state changes in this flow. Navigation is centralized in
`WindowNavigation`; rules use an owned dialog so returning preserves the parent.

## Verification

- Built the application in Debug and Release configurations.
- Loaded and rendered all six views in Spanish and English, plus the existing
  main menu, login and settings screens, with no binding errors reported.
- Changed the language on already-created windows and checked their updated titles.
- Checked Spanish/English resource coverage for the new flow and menu and removed
  unreferenced strings from both resource files.
- Reviewed the rendered layouts; screenshots are saved in the task's
  `outputs/match-interfaces` directory.
- Desktop automation could not acquire the launched app window, so the navigation
  click-through has not been verified interactively.
