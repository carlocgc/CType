using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Type.Interfaces.Service;

namespace Type.Desktop.Source.Services
{
    /// <summary>
    /// Keeps the game's settings and progress in the user's roaming application data, at a path
    /// that does not change when the game is moved or reinstalled
    /// </summary>
    /// <remarks>
    /// Replaces the engine's <c>DataLoader</c> for the game's own values. That opens its store
    /// with <c>GetUserStoreForAssembly</c>, which for an assembly with no strong name resolves
    /// through the executable's path — so a second Steam library folder, a moved install, or a
    /// verify into a different directory produced a fresh empty store and silently lost the
    /// player's high score, settings and unlocks. See ROADMAP item S11.
    /// </remarks>
    public sealed class DesktopStorageProvider : IStorageProvider
    {
        /// <summary> Folder under the roaming profile that holds the save </summary>
        private const String FolderName = "CType";
        /// <summary> Name of the save file, matching what the engine's store used </summary>
        private const String FileName = "SavedData.txt";

        /// <summary>
        /// Value each character is exclusive-ored with, matching the engine's
        /// <c>EngineConstants.ENCRYPTION_KEY</c>
        /// </summary>
        /// <remarks>
        /// Duplicated rather than shared because the engine's constant is <c>internal</c>. It
        /// has to match, or a save migrated out of the old store cannot be read back. This is
        /// obfuscation and not encryption — see S11 — and is kept only so that moving the file
        /// changes nothing about how readable it is.
        /// </remarks>
        private const Int32 ObfuscationKey = 236;

        /// <summary>
        /// Folder the engine's isolated storage saves were written under, which is the
        /// <c>BaseGame</c> name as it stood when they were written
        /// </summary>
        /// <remarks>
        /// Deliberately a literal rather than <c>BaseGame.Name</c>. Migration has to find files
        /// written by past builds, so renaming the game — ROADMAP item S7 wants exactly that —
        /// must not move this needle. Once a player has migrated, this is never used again.
        /// </remarks>
        private const String LegacyStoreName = "Test Game";

        /// <summary> How far below the isolated storage root a legacy store is looked for </summary>
        private const Int32 MaximumStoreDepth = 5;

        /// <summary> Every value held in memory, saved in full on each write </summary>
        private readonly Dictionary<String, String> _Values = new Dictionary<String, String>();

        /// <summary> Full path of the save file </summary>
        private static String FilePath => Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), FolderName, FileName);

        /// <inheritdoc />
        public void Load()
        {
            _Values.Clear();

            try
            {
                if (File.Exists(FilePath))
                {
                    ReadInto(_Values, Deobfuscate(File.ReadAllText(FilePath, Encoding.UTF8)));
                    return;
                }

                // No save here yet, so this is either a first run or the first run since the
                // move. Both are answered the same way: take whatever the old store still holds.
                if (!MigrateFromIsolatedStorage()) return;
                Save();
            }
            catch (Exception)
            {
                // A save that cannot be read must not stop the game starting. The player loses
                // their settings rather than the ability to play, and the next write repairs it.
                _Values.Clear();
            }
        }

        /// <inheritdoc />
        public Object GetValue(String key)
        {
            return _Values.TryGetValue(key, out String value) ? value : null;
        }

        /// <inheritdoc />
        public void SetValue(String key, Object value)
        {
            _Values[key] = value?.ToString() ?? String.Empty;
            Save();
        }

        /// <summary>
        /// Writes every held value to disk, creating the folder on first use
        /// </summary>
        private void Save()
        {
            try
            {
                String directory = Path.GetDirectoryName(FilePath);
                if (directory != null && !Directory.Exists(directory)) Directory.CreateDirectory(directory);

                StringBuilder contents = new StringBuilder();
                foreach (KeyValuePair<String, String> pair in _Values)
                {
                    // A newline or an equals sign in a key would make the file ambiguous. No
                    // caller writes either, so they are dropped rather than escaped.
                    if (pair.Key.IndexOfAny(new[] { '=', '\r', '\n' }) >= 0) continue;
                    contents.Append(pair.Key).Append('=').Append(pair.Value.Replace("\r", "").Replace("\n", "")).Append('\n');
                }

                File.WriteAllText(FilePath, Obfuscate(contents.ToString()), Encoding.UTF8);
            }
            catch (Exception)
            {
                // A read-only or full disk must not crash the game mid-play. The value stays in
                // memory for this session and the next write tries again.
            }
        }

        /// <summary>
        /// Reads key and value pairs, one per line, into the given dictionary
        /// </summary>
        /// <param name="values"> Dictionary to fill </param>
        /// <param name="contents"> Deobfuscated file contents </param>
        private static void ReadInto(IDictionary<String, String> values, String contents)
        {
            String[] lines = contents.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            for (Int32 index = 0; index < lines.Length; index++)
            {
                Int32 separator = lines[index].IndexOf('=');
                if (separator <= 0) continue;

                values[lines[index].Substring(0, separator)] = lines[index].Substring(separator + 1);
            }
        }

        /// <summary>
        /// Brings across whatever the engine's isolated storage still holds, so a player who
        /// already had a save keeps it
        /// </summary>
        /// <returns> Whether anything was found to bring across </returns>
        private Boolean MigrateFromIsolatedStorage()
        {
            String source = FindLegacySave();
            if (source == null) return false;

            ReadLegacyInto(_Values, Deobfuscate(File.ReadAllText(source, Encoding.UTF8)));
            return _Values.Count > 0;
        }

        /// <summary>
        /// Reads the engine's store format, which is a JSON object of scalars
        /// </summary>
        /// <param name="values"> Dictionary to fill </param>
        /// <param name="contents"> Deobfuscated legacy file contents </param>
        /// <remarks>
        /// Parsed by hand rather than with a JSON library. <c>Newtonsoft.Json</c> reaches the
        /// desktop output but not its compilation — it is a transitive runtime dependency of
        /// <c>AmosDesktop</c>, not a reference of <c>Type.Desktop</c> — and adding a package to
        /// this project to read a file that is only read once, ever, is a poor trade. The engine
        /// wrote these with <c>Formatting.Indented</c>, so every entry is its own line of
        /// <c>"KEY": VALUE,</c> with scalar values only. Anything that does not match that shape
        /// is skipped, which loses a value rather than the whole migration.
        /// </remarks>
        private static void ReadLegacyInto(IDictionary<String, String> values, String contents)
        {
            String[] lines = contents.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            for (Int32 index = 0; index < lines.Length; index++)
            {
                String line = lines[index].Trim().TrimEnd(',');

                Int32 separator = line.IndexOf("\":", StringComparison.Ordinal);
                if (!line.StartsWith("\"", StringComparison.Ordinal) || separator <= 0) continue;

                String key = line.Substring(1, separator - 1);
                String value = line.Substring(separator + 2).Trim().Trim('"');
                if (key.Length == 0 || value.Length == 0) continue;

                values[key] = value;
            }
        }

        /// <summary>
        /// Finds the most recently written isolated storage save, across every store the engine
        /// created for this game
        /// </summary>
        /// <returns> Path of the newest save found, or null if there is none </returns>
        /// <remarks>
        /// Searched rather than opened through <c>IsolatedStorageFile</c> on purpose. That API
        /// would hand back the store for the path the game is running from now, which is the
        /// empty one in the case this whole class exists to fix. The player's data is in a store
        /// keyed to wherever the game used to live, so every store has to be considered and the
        /// newest one wins.
        /// </remarks>
        private static String FindLegacySave()
        {
            String root = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "IsolatedStorage");
            if (!Directory.Exists(root)) return null;

            String newest = null;
            DateTime newestWritten = DateTime.MinValue;

            // A store sits at IsolatedStorage\<user>\<user>\<evidence>\AssemFiles\<name>, so the
            // save is normally three levels down. Every level is tested rather than only that
            // one, because the nesting depends on which evidence the runtime scoped the store
            // with, and a search that assumes a depth silently finds nothing when it guesses
            // wrong. The walk is still bounded, so it cannot wander off across the profile.
            List<String> level = new List<String> { root };

            for (Int32 depth = 0; depth < MaximumStoreDepth && level.Count > 0; depth++)
            {
                List<String> next = new List<String>();

                for (Int32 index = 0; index < level.Count; index++)
                {
                    String path = Path.Combine(level[index], "AssemFiles", LegacyStoreName, FileName);
                    if (File.Exists(path))
                    {
                        DateTime written = File.GetLastWriteTimeUtc(path);
                        if (written > newestWritten)
                        {
                            newest = path;
                            newestWritten = written;
                        }
                    }

                    try
                    {
                        next.AddRange(Directory.EnumerateDirectories(level[index]));
                    }
                    catch (Exception)
                    {
                        // An unreadable directory is simply not a place the save can be.
                    }
                }

                level = next;
            }

            return newest;
        }

        /// <summary>
        /// Obscures the file contents, so a save is not casually editable in a text editor
        /// </summary>
        /// <param name="text"> The text to obscure </param>
        private static String Obfuscate(String text)
        {
            StringBuilder result = new StringBuilder(text.Length);
            for (Int32 index = 0; index < text.Length; index++) result.Append((Char)(text[index] ^ ObfuscationKey));
            return result.ToString();
        }

        /// <summary>
        /// Reverses <see cref="Obfuscate"/>, which is its own inverse
        /// </summary>
        /// <param name="text"> The text to restore </param>
        private static String Deobfuscate(String text)
        {
            return Obfuscate(text);
        }
    }
}
