# Store listing

Draft for the console. One section per locale. Every text block below is
copied verbatim into the console — keep them inside fenced blocks so
they can be diffed programmatically against the submission pack.

## Platform limits

| Field | Limit | Required |
| --- | --- | --- |
| App name | 30 characters | yes |
| Short description | 80 characters | yes |
| Full description | 4000 characters | yes |
| App icon | 512×512 PNG, no real transparency | yes |
| Feature graphic | 1024×500 PNG or JPEG | yes |
| Phone screenshots | 2–8, side 320–3840 px, ratio at most 2:1 | yes |
| Video | YouTube link | no |

Measure the counts; do not estimate them.

## Files

- `store-assets/<store icon>` — 512×512
- `store-assets/<feature graphic>` — 1024×500
- `store-assets/screenshots/<locale>/` — one set per locale

## Default locale

### App name

```
```

### Short description

```
```

### Full description

```
```

## Second locale

### App name

```
```

### Short description

```
```

### Full description

```
```

## Screenshots

Same filenames in every locale set, uploaded in this order:

| File | What is on the frame |
| --- | --- |
| | |

Captured by code through the editor seams, not played by hand — the set
has to regenerate after a balance change. Note where the state and
scores are defined so the next person edits the right file.

Record what the set does *not* show, and why it was not worth a new seam
in the game code.

Editor captures have no status bar, no cutout and no gesture bar. That
does not violate store rules, and it does not replace a device run.

## Countries and languages

Default listing language, additional locales, and whether the interface
picks its language automatically. If it does, releasing to all countries
is safe.
