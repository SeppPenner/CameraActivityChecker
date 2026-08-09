# Project rules for Claude

## Commits

- Commit messages are written **in English only**.
- Short, precise summary in the subject line, plus an explanatory body when needed.

## Punctuation

- **No em dashes or en dashes** (`—`, `–`), neither in prose, commit messages, code comments
  nor documentation.
- Use a regular hyphen, comma, colon, parentheses or a separate sentence instead.

## Code comments

- Comments in code (and in project files such as `.csproj`) are **always written in English**,
  regardless of the language used in the rest of the communication.

## German texts

- In German texts (documentation, chat replies) always use **real umlauts and ß**, never ASCII
  transliterations.
- Rewrite where needed:
  - `ae` -> `ä`
  - `oe` -> `ö`
  - `ue` -> `ü`
  - `Ae` -> `Ä`, `Oe` -> `Ö`, `Ue` -> `Ü`
  - `ss` -> `ß` (only where orthographically correct, e.g. `Strasse` -> `Straße`; `dass` stays
    `dass`)
- This applies to documentation files and chat, **not** to code comments (those are English,
  see above).
- Exception: identifiers, file names, configuration keys and similar stay unchanged when umlauts
  are technically undesirable there.
