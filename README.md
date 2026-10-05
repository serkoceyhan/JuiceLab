\# JuiceLab



The same game, twice. The left side is mechanically correct and completely dead.

The right side runs the same code with 11 layers of game feel on top — no gameplay

rule was changed, only the feedback layer.



!\[before and after](docs/juice\_comparison.gif)



\*\*Playable WebGL demo:\*\* \_coming soon\_ — every effect can be toggled at runtime.



\## Why it is built this way



Juice is usually written straight into gameplay code. That breaks two things at once:

the game logic becomes unreadable, and the effects become impossible to turn off.



Here the gameplay only announces \*what happened\*:



```csharp

GameEvents.RaiseBrickDestroyed(new ImpactInfo(point, normal, 1f, color, center, size, combo));

```



It knows nothing about any effect. Eleven modules listen to these events. Each one can

be enabled, disabled and scaled independently, and one global slider scales all of them

from 0 to 1 — that slider is the demo.



Delete `JuiceManager` from the scene and the game still runs, unchanged.



\## Architecture



| Type | Role |

|---|---|

| `GameEvents` | Static event bus — six events, the only contact point between gameplay and feel |

| `ImpactInfo` | `readonly struct` payload: point, normal, strength, color, center, size, combo |

| `JuiceModule` | Abstract base — every module has an on/off flag and a 0–1 intensity |

| `JuiceManager` | Collects the modules, owns the global multiplier |

| `JuiceProfile` | `ScriptableObject` presets ("No Juice", "Full") |

| `JuiceDebugPanel` | Builds its rows from the module list at runtime, not by hand |



\## Effect layers



| Module | What it actually does |

|---|---|

| Camera Shake | Trauma-based: shake scales with `trauma²`, offset comes from Perlin noise rather than `Random`, decays at a fixed rate |

| Hit Stop | `Time.timeScale` drops to \~0.03 for 50–70 ms; the timer runs on `unscaledDeltaTime` so it cannot freeze itself |

| Ball Squash \& Stretch | Squashes along the contact normal and stretches on the perpendicular axis to preserve volume; `OutQuad` in, `OutElastic` back |

| Brick Death | Gameplay destroys the brick immediately; a pooled visual corpse plays the pop-and-fade |

| Impact Particles | A single `ParticleSystem` fed per-particle through `Emit(EmitParams)` — no instantiation, no pooling needed |

| Camera Punch | Drives `orthographicSize` rather than position, so it never fights the shake module |

| Audio | 8-voice round-robin pool, pitch jitter, and a combo pitch that climbs in semitone steps (`2^(n/12)`) |

| Score Pop | The score counts up instead of jumping, with an `OutBack` punch; takes ownership of the label from the HUD while active |

| Post FX Punch | Short chromatic aberration and vignette pulse through the URP Volume |

| Ball Trail / Paddle Squash | One continuous module and one event-driven one |



\## Performance notes



\- `ImpactInfo` is a `readonly struct` — dozens of events per second, zero heap allocation

\- `collision.GetContact(0)` instead of `collision.contacts\[]`, which allocates an array on every access

\- Brick corpses are pooled with `UnityEngine.Pool.ObjectPool`

\- Static events are reset on every play through `RuntimeInitializeOnLoadMethod`, so subscribers cannot leak between sessions when domain reload is disabled



\## Running it



Unity 6.3 LTS (6000.3), URP 2D. Open `**[Playable WebGL demo](https://serkocyhn.itch.io/juicelab)** — every effect can be toggled at runtime.` and press Play.

\*\*Tab\*\* toggles the effect panel.

