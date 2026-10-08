# Signing key and release build

## 1. Create the keystore

Created once, valid for decades. Record the path, the alias, and the
certificate validity window here — not the passwords.

## 2. Back it up

Losing this key after the first upload means no more updates, and the
application id cannot be reused. At least two copies, off this machine.

Verify each copy by **opening** it with the keystore tool. A corrupt
keystore is byte-identical in size to a good one, so comparing sizes
proves nothing.

Record when each copy was last verified.

## 3. Build

```sh
export <KEYSTORE_VAR>=<path>
export <KEYSTORE_PASS_VAR>=...
export <ALIAS_VAR>=...
export <ALIAS_PASS_VAR>=...

<engine binary> -batchmode -nographics -quit \
  -projectPath <project> -buildTarget Android \
  -executeMethod <BuildClass>.<Method> \
  -logFile <log path>
```

Passwords live only in the build process environment; the script clears
them from project settings afterwards, so they never reach a file. Do
not set them in the editor UI — from there they are saved into the
project.

An unsigned build is marked by a filename suffix. If you see it, the
environment variables were not set.

Check the environment before claiming a build will be unsigned — these
variables are often exported from a shell profile.

## 4. Verify the artifact

Never from the build log. Against the file itself: package id, version
code and name, minimum and target API, the full permission list, native
architectures, certificate owner and fingerprint, packaging flags, and
the estimated delivery size.

## 5. Check on a device

Build a sideload package from the same bundle that will be uploaded, so
the test covers the real artifact.

Uploading to an internal testing track and installing from the store is
closer to reality: real signature, real per-device delivery.

## 6. Next upload

Bump the build number with the project command, then rebuild. The
version code must increase with every upload; the version name changes
when the user-facing version does.

## Device checklist

- First launch on a clean install.
- Progress saved and surviving a restart.
- Backgrounding and returning mid-action.
- Different aspect ratios, camera cutout, gesture navigation.
- No debug interface and no debug logging.
- Heat and battery over a long session.

If the build changed after this run, repeat it. Shorter, but repeat it.
