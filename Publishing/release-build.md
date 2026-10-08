# Release build

How to produce and check the artifact. Every number quoted here is read from
the artifact itself — never from a build log. A Unity build summary once
reported a 35 MB bundle as "664 MB" because the field it printed measured the
intermediate Gradle project.

## Tools

Unity ships all of them inside the Android player, so nothing needs
installing:

```sh
U=~/Unity/Hub/Editor/6000.6.4f1/Editor
AP=$U/Data/PlaybackEngines/AndroidPlayer
JAVA=$AP/OpenJDK/bin/java
KEYTOOL=$AP/OpenJDK/bin/keytool
BT=$(ls $AP/Tools/bundletool-all-*.jar)
AAPT=$(ls -d $AP/SDK/build-tools/*/ | tail -1)aapt2
```

## Signing

The keystore is **not** in this repository and never will be — `.gitignore`
excludes `*.keystore` and `*.jks`.

| | |
| --- | --- |
| Keystore | `~/keystores/tilevault.keystore` |
| Backup | `~/keystores/tilevault.keystore.backup1` |
| Credentials | `~/keystores/tilevault-signing.env` (mode 600) |
| Alias | `tilevault` |
| Certificate | `CN=Gor Beglaryan, O=Gor903, C=AM`, RSA 2048, 10000 days |

Credentials reach the build only through the environment. `ReleaseTool`
reads them, and wipes them from `ProjectSettings` afterwards in a `finally`
block, so a failed build cannot leave a password in a tracked file.

```sh
set -a; . ~/keystores/tilevault-signing.env; set +a
```

Without those variables the build still runs, but the output is named
`tilevault-UNSIGNED.aab` so it cannot be mistaken for a releasable artifact.

### Losing this key ends the app

No key means no updates, ever, and the application id cannot be changed to
start again. Both copies currently sit on the same machine, which is not a
backup — **at least one copy belongs somewhere else entirely.**

Verify any copy by opening it, never by comparing file size. A corrupt
keystore is byte-identical in length to a good one:

```sh
$KEYTOOL -list -v -keystore <copy> -storepass "$TILEVAULT_KEYSTORE_PASS"
```

## Commands

```sh
# Apply every release setting (menu: Tilevault → Release → Apply Settings)
$U/Unity -batchmode -nographics -projectPath . \
  -executeMethod Tilevault.Editor.ReleaseTool.CIApply -logFile /dev/stdout

# Preflight only; exit 1 and a named list when anything is wrong
$U/Unity -batchmode -nographics -projectPath . \
  -executeMethod Tilevault.Editor.ReleaseTool.CIVerify -logFile /dev/stdout

# Build; refuses to run when preflight fails
set -a; . ~/keystores/tilevault-signing.env; set +a
$U/Unity -batchmode -nographics -projectPath . \
  -executeMethod Tilevault.Editor.ReleaseTool.CIBuild -logFile /dev/stdout

# Every upload needs a higher version code — bumped here, never by hand
$U/Unity -batchmode -nographics -projectPath . \
  -executeMethod Tilevault.Editor.ReleaseTool.CIBumpVersionCode -logFile /dev/stdout
```

An IL2CPP build for two architectures takes roughly 20–40 minutes from a cold
cache. `Library/Bee` makes later builds much faster; do not delete it to
"clean up".

## Preflight

`ReleaseTool.Verify()` returns a list of problems and `Build()` refuses on a
non-empty list. It checks application id, product and company name, min and
target API, ARM64 presence, IL2CPP, portrait lock, bundle output, the three
debug flags, the internet-permission setting, the scene list, the icons, the
version name and code — and that the custom manifest still exists.

That last one is the subtle one. Deleting
`Assets/Plugins/Android/AndroidManifest.xml` silently restores the network
permission with no warning from anywhere, which would contradict both the
privacy policy and the Data safety answers.

Proven to work in both directions: it passes on the clean project, and on a
deliberately broken one it named all four faults and the build produced
nothing.

## Verifying the artifact

Run all of it after **any** change that touches the build — removing a
package can change the merged manifest.

```sh
# manifest, versions, SDK levels, full permission list
$JAVA -jar $BT dump manifest --bundle=Build/tilevault.aab

# prove a permission is absent rather than eyeballing the list
$JAVA -jar $BT dump manifest --bundle=Build/tilevault.aab | grep -c "permission.INTERNET"

# packaging: uncompressNativeLibraries should be enabled
$JAVA -jar $BT dump config --bundle=Build/tilevault.aab

# size on disk and the largest entries
unzip -l Build/tilevault.aab | sort -rn | head -10

# what a device actually downloads — not the bundle size
$JAVA -jar $BT build-apks --bundle=Build/tilevault.aab --output=/tmp/tv.apks
$JAVA -jar $BT get-size total --apks=/tmp/tv.apks

# certificate owner must not be CN=Android Debug
$KEYTOOL -printcert -jarfile Build/tilevault.aab
```

A mismatch on any line means the settings did not apply. It does not mean the
check is too strict.

## Sideload package for device testing

Built from the **same bundle that will be uploaded**, so the device run
covers the real artifact:

```sh
$JAVA -jar $BT build-apks --bundle=Build/tilevault.aab \
  --output=/tmp/tv-universal.apks --mode=universal
unzip -o -q /tmp/tv-universal.apks -d /tmp/tv-universal
cp /tmp/tv-universal/universal.apk Build/tilevault-universal-debugsigned.apk
```

Two things to know when handing it over:

- It is signed with the local debug key, so it **cannot** be installed over a
  store build, or vice versa.
- It is roughly twice the bundle size, because a universal APK carries every
  configuration and the native libraries are stored uncompressed. Store
  delivery uses per-device splits and is unaffected. Quote the split size when
  talking about download size.
