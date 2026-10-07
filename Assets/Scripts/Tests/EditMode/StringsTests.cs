using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using Tilevault.Game.Localisation;

namespace Tilevault.Tests
{
    /// <summary>
    /// Guards the localisation table. A missing entry is invisible until it
    /// reaches a screenshot, so it gets caught here instead.
    /// </summary>
    public class StringsTests
    {
        static IEnumerable<string> AllKeys()
        {
            foreach (FieldInfo field in typeof(Strings.Key).GetFields(BindingFlags.Public | BindingFlags.Static))
                if (field.IsLiteral && field.FieldType == typeof(string))
                    yield return (string)field.GetRawConstantValue();
        }

        [Test]
        public void EveryKeyResolvesInEveryShippedLanguage()
        {
            foreach (string language in Strings.AvailableLanguages)
            {
                Strings.SelectLanguage(language);
                IReadOnlyDictionary<string, string> table = Strings.TableFor(language);

                foreach (string key in AllKeys())
                {
                    Assert.IsTrue(table.ContainsKey(key),
                        $"Language '{language}' is missing the key '{key}'.");
                    Assert.IsNotEmpty(table[key], $"Language '{language}' has an empty value for '{key}'.");
                }
            }

            Strings.SelectLanguage(Strings.Lang_English);
        }

        [Test]
        public void NoOrphanedEntries()
        {
            var declared = new HashSet<string>(AllKeys());

            foreach (string language in Strings.AvailableLanguages)
            foreach (string key in Strings.TableFor(language).Keys)
                Assert.IsTrue(declared.Contains(key),
                    $"Language '{language}' defines '{key}', which no Key constant refers to.");
        }

        [Test]
        public void UnknownLanguageFallsBackToEnglish()
        {
            Strings.SelectLanguage("qq");
            Assert.AreEqual(Strings.Lang_English, Strings.CurrentLanguage);
        }

        [Test]
        public void MissingKeyReturnsTheKeyRatherThanThrowing()
        {
            Strings.SelectLanguage(Strings.Lang_English);
            Assert.AreEqual("no.such.key", Strings.Get("no.such.key"));
        }

        [Test]
        public void FormattedStringsAcceptTheirArguments()
        {
            Strings.SelectLanguage(Strings.Lang_English);

            Assert.AreEqual("Score 1234", Strings.Get(Strings.Key.ScoreValue, 1234));
            Assert.AreEqual("You reached 2048!", Strings.Get(Strings.Key.YouReached, 2048));
            Assert.AreEqual("Streak: 7", Strings.Get(Strings.Key.Streak, 7));
        }

        [Test]
        public void PlaceholderCountsMatchAcrossLanguages()
        {
            // A translation with the wrong number of placeholders throws at
            // string.Format time, which would be a crash in front of a player.
            IReadOnlyDictionary<string, string> english = Strings.TableFor(Strings.Lang_English);

            foreach (string language in Strings.AvailableLanguages)
            {
                if (language == Strings.Lang_English) continue;
                IReadOnlyDictionary<string, string> table = Strings.TableFor(language);

                foreach (KeyValuePair<string, string> entry in english)
                {
                    if (!table.TryGetValue(entry.Key, out string translated)) continue;
                    Assert.AreEqual(Placeholders(entry.Value), Placeholders(translated),
                        $"'{entry.Key}' has a different placeholder count in '{language}'.");
                }
            }
        }

        static int Placeholders(string value)
        {
            int count = 0;
            for (int i = 0; i < 4; i++)
                if (value.Contains("{" + i + "}")) count++;
            return count;
        }
    }
}
