using ClosedXML.Excel;

using DocumentFormat.OpenXml.Wordprocessing;

using Newtonsoft.Json;

using System.Text.RegularExpressions;

using Windows.Storage;

using Group = System.Text.RegularExpressions.Group;
using LanguageConfigurationStructure = System.Collections.Generic.Dictionary<string, System.Collections.Generic.Dictionary<string, System.Collections.Generic.Dictionary<string, string>>>;
using LanguageConfigurationStructureSelection =
    System.Collections.Generic.Dictionary<string,
        System.Collections.Generic.Dictionary<string, string>>;
using TabSettingJson = System.Collections.Generic.Dictionary<string, System.Collections.Generic.Dictionary<string, object>>;

namespace Peloton_IDE.Presentation
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class TranslatePage : Microsoft.UI.Xaml.Controls.Page
    {
        //List<PropertyBag>? OldPlexes;

        LanguageConfigurationStructure? Langs;
        string? SourceName { get; set; }
        string? SourceFolder { get; set; }
        string? DataPath { get; set; }

        long Quietude { get; set; }
        internal static List<PlexBlock>? PlexBlocks { get; private set; }

        TabSettingJson? SourceInFocusTabSettings { get; set; }

        [GeneratedRegex(@"<(?:#|@) (.+?)>(.*?)</(?:#|@)>", RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.Compiled, "en-AU")]
        private static partial Regex PelotonFullPattern();

        [GeneratedRegex(@"<# (.+?)>", RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.Compiled, "en-AU")]
        private static partial Regex PelotonVariableSpacedPattern();

        [GeneratedRegex(@"<@ (...\s{0,1})+?>", RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.Compiled, "en-AU")]
        private static partial Regex PelotonFixedSpacedPattern();

        public TranslatePage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            ApplicationData.Current.LocalSettings.Values["Where"] = "TranslatePage";

            NavigationData parameters = (NavigationData)e.Parameter;

            if (parameters.Source == "MainPage")
            {
                SourceInFocusTabSettings = (TabSettingJson?)parameters.KVPs["InFocusTabSettingsDict"];
                PlexBlocks = (List<PlexBlock>?)parameters.KVPs["PlexBlocks"];
                Langs = (LanguageConfigurationStructure)parameters.KVPs["Languages"];
                //string? tabLanguageName = parameters.KVPs["TabLanguageName"].ToString();
                int tabLanguageId = (int)(long)parameters.KVPs["TabLanguageID"];
                string? interfaceLanguageName = parameters.KVPs["ideOps.InterfaceLanguageName"].ToString();
                Quietude = (long)parameters.KVPs["pOps.Quietude"];
                //int interfaceLanguageID = (int)(long)parameters.KVPs["ideOps.InterfaceLanguageID"];
                SourceName = parameters.KVPs["SourceName"].ToString();
                SourceFolder = parameters.KVPs["SourceFolder"].ToString();
                //DataPath = parameters.KVPs["DataPath"].ToString();
                FillLanguagesIntoList(Langs, interfaceLanguageName!, sourceLanguageList);
                FillLanguagesIntoList(Langs, interfaceLanguageName!, targetLanguageList);

                LanguageConfigurationStructureSelection language = Langs[interfaceLanguageName!];
                cmdCancel.Content = language["frmMain"]["cmdCancel"];
                cmdSaveMemory.Content = language["frmMain"]["cmdSaveMemory"];
                chkSpaceOut.Content = language["frmMain"]["chkSpaceOut"];
                chkVarLengthFrom.Content = language["frmMain"]["chkVarLengthFrom"];
                chkVarLengthTo.Content = language["frmMain"]["chkVarLengthTo"];

                CustomRichEditBox rtb = ((CustomRichEditBox)parameters.KVPs["RichEditBox"]);
                rtb.Document.GetText(Microsoft.UI.Text.TextGetOptions.None, out string selectedText);
                while (selectedText.EndsWith('\r')) selectedText = selectedText.Remove(selectedText.Length - 1);
                sourceText.Document.SetText(Microsoft.UI.Text.TextSetOptions.None, selectedText);

                if (selectedText.Contains("</#>"))
                {
                    chkVarLengthFrom.IsChecked = true;
                }

                if (ProbablySpacedInstructions(selectedText))
                {
                    chkSpaceIn.IsChecked = true;
                }

                sourceLanguageList.SelectedIndex = tabLanguageId;
                sourceLanguageList.ScrollIntoView(sourceLanguageList.SelectedItem);
                (sourceLanguageList.ItemContainerGenerator.ContainerFromIndex(tabLanguageId) as ListBoxItem)?.Focus(FocusState.Programmatic);

            }
        }

        private bool ProbablySpacedInstructions(string selectedText)
        {
            int result = 0;
            Regex pattern = PelotonFullPattern();
            MatchCollection matches = pattern.Matches(selectedText);
            for (int i = 0; i < matches.Count; i++)
            {
                Match match = matches[i];
                ReadOnlySpan<char> group = match.Groups[1].ValueSpan;
                var enu = group.EnumerateRunes();
                do
                {
                    if (enu.Current.Value == ' ') result++;
                } while (enu.MoveNext());
            }
            return result > 0;
        }

        private static void FillLanguagesIntoList(LanguageConfigurationStructure languages, string interfaceLanguageName, ListBox listBox)
        {
            Telemetry.Disable();

            if (languages is null)
            {
                throw new ArgumentNullException(nameof(languages));
            }

            Telemetry.Transmit("listBox.Name=", listBox.Name);

            // what is current language?
            Dictionary<string, string> globals = languages[interfaceLanguageName]["GLOBAL"];
            for (int i = 0; i < languages.Keys.Count; i++)
            {
                var names = from lang in languages.Keys
                            where languages.ContainsKey(lang) && languages[lang]["GLOBAL"]["ID"] == i.ToString()
                            let name = languages[lang]["GLOBAL"]["Name"]
                            select name;
                if (names.Any())
                {
                    string name = names.First();
                    bool present = LanguageIsPresentInPlexBlocks(name);

                    ListBoxItem listBoxItem = new()
                    {
                        Content = globals[$"{100 + i + 1}"],
                        Name = name,
                        IsEnabled = present
                    };
                    Telemetry.Transmit("listBoxItem.Name=", listBoxItem.Name);
                    Telemetry.Transmit("listBoxItem.IsEnabled=", listBoxItem.IsEnabled);

                    listBox.Items.Add(listBoxItem);
                }
            }
        }

        private static bool LanguageIsPresentInPlexBlocks(string name)
        {
            return (from plex in PlexBlocks where plex.Plex.Meta.Language == name.Replace(" ", "") select plex).Any();
        }

        private string TranslateCode(string code, string sourceLanguageName, string targetLanguageName)
        {
            Telemetry.Enable();

            Telemetry.Transmit("code=", code);
            Telemetry.Transmit("sourceLanguageName=", sourceLanguageName);
            Telemetry.Transmit("targetLanguageName=", targetLanguageName);

            bool variableTargetTicked = chkVarLengthTo.IsChecked ?? false;
            bool variableSourceTicked = chkVarLengthFrom.IsChecked ?? false;
            bool fixedTargetTicked = chkVarLengthTo.IsChecked == false;
            bool fixedSourceTicked = chkVarLengthFrom.IsChecked == false;
            bool spacedTargetTicked = chkSpaceOut.IsChecked ?? false;

            string? sourceName = (sourceLanguageName).Replace(" ", "").ToUpperInvariant();
            string? targetName = (targetLanguageName).Replace(" ", "").ToUpperInvariant();

            Plex? englishFixed = (from plexblock in PlexBlocks where plexblock.Plex.Meta.Language == "English" && !plexblock.Plex.Meta.Variable select plexblock).First().Plex;

            IEnumerable<PlexBlock> sourcePlexVariable = from plex in PlexBlocks where plex.Plex.Meta.Language == sourceLanguageName.Replace(" ", "") && plex.Plex.Meta.Variable select plex;
            IEnumerable<PlexBlock> targetPlexVariable = from plex in PlexBlocks where plex.Plex.Meta.Language == targetLanguageName.Replace(" ", "") && plex.Plex.Meta.Variable select plex;

            IEnumerable<PlexBlock> sourcePlexFixed = from plex in PlexBlocks where plex.Plex.Meta.Language == sourceLanguageName.Replace(" ", "") && !plex.Plex.Meta.Variable select plex;
            IEnumerable<PlexBlock> targetPlexFixed = from plex in PlexBlocks where plex.Plex.Meta.Language == targetLanguageName.Replace(" ", "") && !plex.Plex.Meta.Variable select plex;


            Telemetry.Transmit("variableTargetTicked=", variableTargetTicked);
            Telemetry.Transmit("variableSourceTicked=", variableSourceTicked);
            Telemetry.Transmit("fixedTargetTicked=", fixedTargetTicked);
            Telemetry.Transmit("fixedSourceTicked=", fixedSourceTicked);
            Telemetry.Transmit("spacedTargetTicked=", spacedTargetTicked);

            //if (sourcePlexVariable.Any())
            //{
            //    var first = sourcePlexVariable.First();
            //    Telemetry.Transmit("sourcePlexVariable's PlexFile=", first.PlexFile);
            //}
            //if (targetPlexVariable.Any())
            //{
            //    var first = targetPlexVariable.First();
            //    Telemetry.Transmit("targetPlexVariable's PlexFile=", first.PlexFile);
            //}
            //if (sourcePlexFixed.Any())
            //{
            //    var first = sourcePlexFixed.First();
            //    Telemetry.Transmit("sourcePlexFixed's PlexFile=", first.PlexFile);
            //}
            //if (targetPlexFixed.Any())
            //{
            //    var first = targetPlexFixed.First();
            //    Telemetry.Transmit("targetPlexFixed's PlexFile=", first.PlexFile);
            //}

            // if variable source ticked we need source to point to a variable lexer IF IT EXISTS 
            // if it does not exist, we point to a fixed length one

            Plex? source = sourcePlexFixed.First().Plex;
            Plex? target = targetPlexFixed.First().Plex;

            if (variableSourceTicked)
            {
                if (sourcePlexVariable != null && sourcePlexVariable.Any())
                {
                    source = sourcePlexVariable.First().Plex;
                    Telemetry.Transmit("source plexfile=", sourcePlexVariable.First().PlexFile);
                }
            }
            else
            {
                Telemetry.Transmit("source plexfile=", sourcePlexFixed.First().PlexFile);
            }

            if (variableTargetTicked)
            {
                if (targetPlexVariable != null && targetPlexVariable.Any())
                {
                    target = targetPlexVariable.First().Plex;
                    Telemetry.Transmit("target plexfile=", targetPlexVariable.First().PlexFile);
                }
            }
            else
            {
                Telemetry.Transmit("target plexfile=", targetPlexFixed.First().PlexFile);
            }

            List<KeyValuePair<string, string>> kvpList =
            [
                .. from long key in englishFixed.OpcodesByValue.Keys
                                 select new KeyValuePair<string, string>($"{key:00000000}", target.OpcodesByValue[key]),
            ];

            string? translatedCode = string.Empty;

            if (variableSourceTicked && sourcePlexVariable != null && sourcePlexVariable.Any())
            {
                translatedCode = ProcessVariableToFixedOrVariable(code, source, target, spacedTargetTicked, variableTargetTicked);
            }
            else
            {
                translatedCode = ProcessFixedToFixedOrVariableWithOrWithoutSpace(code, source, target, spacedTargetTicked, variableTargetTicked);
            }



            //string translatedCode = variableSourceTicked && sourcePlexVariable.Any()
            //    ? ProcessVariableToFixedOrVariable(code, source, target, spacedTargetTicked, variableTargetTicked)
            //    : ProcessFixedToFixedOrVariableWithOrWithoutSpace(code, source, target, spacedTargetTicked, variableTargetTicked);

            //string? pathToSource = DataPath; // Path.GetDirectoryName(SourceFolder);
            string? pathToSource = SourceFolder;
            string? nameOfSource = Path.GetFileNameWithoutExtension(SourceName);

            string? xlsxPath = Path.Combine(pathToSource ?? ".", "p.xlsx");

            Telemetry.Transmit("pathToSource=", pathToSource);
            Telemetry.Transmit("nameOfSource=", nameOfSource);
            Telemetry.Transmit("xlsxPath=", xlsxPath);

            bool ok = false;

            (ok, XLWorkbook? workbook) = GetNamedExcelWorkbook(xlsxPath);
            if (!ok) return translatedCode;

            (ok, IXLWorksheet? worksheet) = GetNamedWorksheetInExcelWorkbook(workbook, nameOfSource, "Document#");
            if (!ok) return translatedCode;

            Telemetry.Transmit("Worksheet=", worksheet.Name);

            (ok, int sourceCol, int targetCol) = GetSourceAndTargetColumnsFromWorksheet(worksheet, source.Meta.LanguageId, target.Meta.LanguageId);
            if (!ok) return translatedCode;

            // iterate thru strings in source language, building dictionary of replacements ordered by length of sourceText
            SortedDictionary<string, (double _typeCode, string _text)> sortedDictionary = new(new LongestToShortestLengthComparer());
            (ok, SortedDictionary<string, (double _typeCode, string _text)> dict) = FillSortedDictionaryFromWorksheet(sortedDictionary, worksheet, sourceCol, targetCol);
            if (!ok) return translatedCode;

            //long DEF_opcode = englishFixed.OpcodesByKey["DEF"];
            //long KOP_opcode = englishFixed.OpcodesByKey["KOP"];
            //long RST_opcode = englishFixed.OpcodesByKey["RST"];
            //long SAY_opcode = englishFixed.OpcodesByKey["SAY"];
            //long GET_opcode = englishFixed.OpcodesByKey["GET"];
            //long UDR_opcode = englishFixed.OpcodesByKey["UDR"];
            //long UDO_opcode = englishFixed.OpcodesByKey["UDO"];
            //long ACT_opcode = englishFixed.OpcodesByKey["ACT"];
            //long KEY_opcode = englishFixed.OpcodesByKey["KEY"];
            //long LET_opcode = englishFixed.OpcodesByKey["LET"];
            //long VAR_opcode = englishFixed.OpcodesByKey["VAR"];

            foreach (string key in dict.Keys)
            {
                Telemetry.Transmit("key=", key);
                Telemetry.Transmit("dict[key]._typeCode=", dict[key]._typeCode);
                Telemetry.Transmit("dict[key]._text=", dict[key]._text);

                if (dict[key]._typeCode != 11)
                {
                    foreach (var patt in new string[] { ">?<", ">?|", "|?|", "|?<" })
                    {
                        string changeFrom = patt.Replace("?", key);
                        string changeTo = patt.Replace("?", dict[key]._text);
                        translatedCode = translatedCode.Replace(changeFrom, changeTo, StringComparison.CurrentCultureIgnoreCase);
                        Telemetry.Transmit("from=", changeFrom, "to=", changeTo);
                    }
                }
                else
                {
                    translatedCode = translatedCode.Replace(key, dict[key]._text, StringComparison.CurrentCultureIgnoreCase);
                    Telemetry.Transmit("from=", key, "to=", dict[key]._text);
                }
                //switch (dict[key]._typeCode)
                //{
                //    case 1: // undefined
                //        break;
                //    case 2: // KOP
                //        string kopPattern = $"<{(source.Meta.Variable ? "#" : "@")} {target.OpcodesByValue[DEF_opcode]}{target.OpcodesByValue[KOP_opcode]}.*?>(.*?{Regex.Escape(key)}[^<]*)";
                //        translatedCode = MorphTranslatedCodeUsingPattern(translatedCode, dict, key, kopPattern);
                //        break;
                //    case 3: // Code Block 
                //        string defudrPattern = $"<{(source.Meta.Variable ? "#" : "@")} {target.OpcodesByValue[DEF_opcode]}{target.OpcodesByValue[UDR_opcode]}.*?>(.*?{Regex.Escape(key)}[^<]*)";
                //        translatedCode = MorphTranslatedCodeUsingPattern(translatedCode, dict, key, defudrPattern);
                //        string defudoPattern = $"<{(source.Meta.Variable ? "#" : "@")} {target.OpcodesByValue[DEF_opcode]}{target.OpcodesByValue[UDO_opcode]}.*?>(.*?{Regex.Escape(key)}[^<]*)";
                //        translatedCode = MorphTranslatedCodeUsingPattern(translatedCode, dict, key, defudoPattern);
                //        break;
                //    case 4: // SQL
                //        foreach (var kwd in new string[] { "SQL", "RST", "DBF" })
                //        {
                //            var opcode = englishFixed.OpcodesByKey[kwd];
                //            string patt = $"<{(source.Meta.Variable ? "#" : "@")} {target.OpcodesByValue[opcode]}.*?>(.*?{Regex.Escape(key)}[^<]*)";
                //            translatedCode = MorphTranslatedCodeUsingPattern(translatedCode, dict, key, patt);
                //        }
                //        //string rstPattern = $"<{(source.Meta.Variable ? "#" : "@")} {target.OpcodesByValue[RST_opcode]}.*?>(.*?{Regex.Escape(key)}[^<]*)";
                //        //translatedCode = MorphTranslatedCodeUsingPattern(translatedCode, dict, key, rstPattern);

                //        break;
                //    case 5: // undefind
                //        break;
                //    case 6: // file extension
                //        break;
                //    case 7: // Pattern
                //        break;
                //    case 8: // Syskey
                //        string keyPattern = $"<{(source.Meta.Variable ? "#" : "@")} .*?{target.OpcodesByValue[KEY_opcode]}.*?>(.*?{Regex.Escape(key)}[^<]*)";
                //        translatedCode = MorphTranslatedCodeUsingPattern(translatedCode, dict, key, keyPattern);
                //        break;
                //    case 9: // Protium symbol
                //        string varPattern = $"<{(source.Meta.Variable ? "#" : "@")} .*?{target.OpcodesByValue[VAR_opcode]}.*?>(.*?{Regex.Escape(key)}[^<]*)";
                //        translatedCode = MorphTranslatedCodeUsingPattern(translatedCode, dict, key, varPattern);
                //        break;
                //    case 10: // Wildcard
                //        break;
                //    case 11: // String Literal
                //        //string sayPattern = $"<{(source.Meta.Variable ? "#" : "@")} {target.OpcodesByValue[SAY_opcode]}.*?>(.*?{Regex.Escape(key)}[^<]*)";
                //        //translatedCode = MorphTranslatedCodeUsingPattern(translatedCode, dict, key, sayPattern);
                //        //string getPattern = $"<{(source.Meta.Variable ? "#" : "@")} {target.OpcodesByValue[GET_opcode]}.*?>(.*?{Regex.Escape(key)}[^<]*)";
                //        //translatedCode = MorphTranslatedCodeUsingPattern(translatedCode, dict, key, getPattern);
                //        //string letPattern = $"<{(source.Meta.Variable ? "#" : "@")} {target.OpcodesByValue[LET_opcode]}.*?>(.*?{Regex.Escape(key)}[^<]*)";
                //        //translatedCode = MorphTranslatedCodeUsingPattern(translatedCode, dict, key, letPattern);
                //        //translatedCode = translatedCode.Replace();
                //        translatedCode = translatedCode.Replace(key, dict[key]._text);
                //        break;
                //    default:
                //        break;
                //}
            }
            while (translatedCode.EndsWith('\r')) translatedCode = translatedCode.Remove(translatedCode.Length - 1);
            return translatedCode;

            //static string MorphTranslatedCodeUsingPattern(string translatedCode, SortedDictionary<string, (double _typeCode, string _text)> dict, string key, string regexPattern)
            //{
            //    Regex sayRegex = new(regexPattern, RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.Compiled | RegexOptions.RightToLeft);
            //    MatchCollection regexMatches = sayRegex.Matches(translatedCode);
            //    if (regexMatches != null)
            //    {
            //        for (int matchNo = 0; matchNo < regexMatches.Count; matchNo++)
            //        {
            //            Match regexMatch = regexMatches[matchNo];
            //            if (regexMatch.Groups.Count > 1)
            //            {
            //                Group secondGroup = regexMatch.Groups[1];
            //                string value = secondGroup.Value;
            //                if (value != null)
            //                {
            //                    value = value.Replace(key, dict[key]._text, StringComparison.InvariantCultureIgnoreCase);
            //                    translatedCode = translatedCode.Remove(secondGroup.Index, secondGroup.Length);
            //                    translatedCode = translatedCode.Insert(secondGroup.Index, value);
            //                }
            //            }
            //        }
            //    }

            //    return translatedCode;
            //}
        }

        //private string UpdateInLabelSpace(string result, string sourceText, string targetText)
        //{
        //    var pattern = PelotonFullPattern();
        //    MatchCollection matches = pattern.Matches(result);
        //    for (int i = matches.Count - 1; i >= 0; i--)
        //    {
        //        Match match = matches[i];
        //        Group label = match.Groups[2];
        //        var index = label.Index;
        //        var length = label.Length;
        //        var value = label.Value;
        //        if (value.Contains(sourceText, StringComparison.CurrentCultureIgnoreCase))
        //        {
        //            result = result.Remove(index, length);
        //            value = value.Replace(sourceText, targetText, StringComparison.CurrentCultureIgnoreCase);
        //            result = result.Insert(index, value);
        //        }
        //    }
        //    return result;
        //}

        private static string ProcessVariableToFixedOrVariable(string code, Plex? source, Plex? target, bool spaced, bool variableTarget)
        {
            IOrderedEnumerable<string> variableLengthWords = from variableLengthWord in source.OpcodesByKey.Keys orderby -variableLengthWord.Length select variableLengthWord;

            Dictionary<string, string> fixedLengthEquivalents = (from word in variableLengthWords
                                                                 let sourceop = source.OpcodesByKey[word]
                                                                 let targetword = target.OpcodesByValue[sourceop]
                                                                 select (word, targetword)).ToDictionary(x => x.word, x => x.targetword);

            List<Capture> codeBlocks = GetCodeBlocks(code); // in reverse order

            foreach (Capture block in codeBlocks)
            {
                string codeChunk = block.Value;
                foreach (string? vlw in variableLengthWords)
                {
                    string spacedVlw = vlw + " ";

                    if (codeChunk.Contains(spacedVlw, StringComparison.CurrentCulture))
                    {
                        if (spaced)
                        {
                            codeChunk = codeChunk.Replace(spacedVlw, fixedLengthEquivalents[vlw] + " ").Trim();
                        }
                        else
                        {
                            codeChunk = codeChunk.Replace(spacedVlw, fixedLengthEquivalents[vlw]).Trim();
                        }
                        continue;
                    }
                    if (codeChunk.Contains(vlw, StringComparison.CurrentCulture))
                    {
                        if (spaced)
                        {
                            codeChunk = codeChunk.Replace(vlw, fixedLengthEquivalents[vlw] + " ").Trim();
                        }
                        else
                        {
                            codeChunk = codeChunk.Replace(vlw, fixedLengthEquivalents[vlw]).Trim();
                        }
                    }
                }
                code = code.Remove(block.Index, block.Length)
                    .Insert(block.Index, codeChunk);
            }
            return variableTarget ? code.Replace("<@", "<#").Replace("</@>", "</#>") : code.Replace("<#", "<@").Replace("</#>", "</@>");
        }

        private static List<Capture> GetCodeBlocks(string code)
        {
            List<Capture> codeBlocks = [];
            Regex pattern = PelotonVariableSpacedPattern();
            MatchCollection matches = pattern.Matches(code);
            for (int mi = matches.Count - 1; mi >= 0; mi--)
            {
                for (int i = matches[mi].Groups[1].Captures.Count - 1; i >= 0; i--)
                {
                    Capture cap = matches[mi].Groups[1].Captures[i];
                    if (cap == null) continue;
                    codeBlocks.Add(cap);
                }
            }
            return codeBlocks;
        }

        private static string ProcessFixedToFixedOrVariableWithOrWithoutSpace(string buff, Plex? sourcePlex, Plex? targetPlex, bool spaceOut, bool variableTarget)
        {
            var pattern = PelotonFixedSpacedPattern();
            MatchCollection matches = pattern.Matches(buff);
            for (int mi = matches.Count - 1; mi >= 0; mi--)
            {
                //var max = kopMatches[mi].Groups[2].Captures.Count - 1;
                for (int i = matches[mi].Groups[1].Captures.Count - 1; i >= 0; i--)
                {
                    Capture capture = matches[mi].Groups[1].Captures[i];
                    string key = capture.Value.ToUpper(System.Globalization.CultureInfo.InvariantCulture).Trim();
                    if (sourcePlex.OpcodesByKey.TryGetValue(key, out long opcode))
                    {
                        if (targetPlex.OpcodesByValue.TryGetValue(opcode, out string? value))
                        {
                            string newKey = value;
                            string next = buff.Substring(capture.Index + capture.Length, 1);
                            buff = buff.Remove(capture.Index, capture.Length)
                                .Insert(capture.Index, newKey + ((spaceOut && next != ">") ? " " : ""));
                        }
                    }
                }
                // var tag = kopMatches[mi].Groups[1].Captures[0];
            }
            return targetPlex.Meta.Variable ? buff.Replace("<@ ", "<# ").Replace("</@>", "</#>") : buff;
        }

        private void KeyboardAccelerator_Invoked(Microsoft.UI.Xaml.Input.KeyboardAccelerator sender, Microsoft.UI.Xaml.Input.KeyboardAcceleratorInvokedEventArgs args)
        {
            Frame.Navigate(typeof(MainPage), null);
        }
    }
}
